namespace ServerWebUI.Components.CommonClass
{
    /// <summary>
    /// In-app notifications for the header bell (e.g. a message sent from View Registration).
    /// Scoped = one instance per user circuit, so a page and the header share it and the bell
    /// updates instantly without any API call or page refresh.
    /// </summary>
    public class NotificationState
    {
        private const int MaxItems = 20;
        private readonly List<NotificationItem> _items = new();
        private readonly object _lock = new();

        public event Action? OnChange;

        public IReadOnlyList<NotificationItem> Items
        {
            get { lock (_lock) return _items.ToList(); }
        }

        public int UnreadCount
        {
            get { lock (_lock) return _items.Count(x => !x.IsRead); }
        }

        public void Add(string title, string? message, string icon = "bi-chat-dots")
        {
            lock (_lock)
            {
                _items.Insert(0, new NotificationItem
                {
                    Title = title,
                    Message = message ?? "",
                    Icon = icon,
                    CreatedAt = DateTime.Now
                });
                if (_items.Count > MaxItems)
                    _items.RemoveRange(MaxItems, _items.Count - MaxItems);
            }
            OnChange?.Invoke();
        }

        public void MarkAllRead()
        {
            lock (_lock)
            {
                if (_items.All(x => x.IsRead))
                    return;
                foreach (var item in _items)
                    item.IsRead = true;
            }
            OnChange?.Invoke();
        }

        public void Clear()
        {
            lock (_lock) _items.Clear();
            OnChange?.Invoke();
        }
    }

    public class NotificationItem
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Icon { get; set; } = "bi-chat-dots";
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
