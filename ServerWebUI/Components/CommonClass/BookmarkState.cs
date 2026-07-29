using DomainModel.SchoolMaster;
using MyApp.Common;
using ServerWebUI.Shared;

namespace ServerWebUI.Components.Common
{

    public class BookmarkState
    {
        private readonly IHttpService _http;
       
        public List<BookMarkModel> Bookmarks { get; private set; } = new();

        public event Action? OnChange;

        public BookmarkState(IHttpService http)
        {
            _http = http;
        }

        public async Task LoadAsync(string username)
        {
            if (Bookmarks.Any()) return;   // 🔥 already loaded → no API call

            Bookmarks = await _http.Get<List<BookMarkModel>>("BookMark/GetBookMarks?createdby="+ username) ?? new();
            OnChange?.Invoke();
        }

        public bool IsBookmarked(string url)
        {
            var bookmark = Bookmarks.FirstOrDefault(x => x.Url == url && x.IsValid == true);
            return bookmark != null && bookmark.IsValid;
        }
        public async Task ToggleAsync(BookMarkModel model)
        {
            var existing = Bookmarks.FirstOrDefault(x => x.Url == model.Url);

            if (existing != null)
            {
               await _http.Post<ApiResponse<string>>("BookMark/DeleteBookMarks",existing.BookMarkId);
            
               Bookmarks.Remove(existing);
    
            }
            else
            {
             
               await _http.Post<ApiResponse<string>>("BookMark/AddOrUpdateBookMarks", model);
                Bookmarks.Add(model);
            }
        
            Notify();
        }

        private void Notify() => OnChange?.Invoke();
    }

}