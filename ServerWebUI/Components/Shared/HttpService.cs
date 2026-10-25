using DomainModel.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using MyApp.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerWebUI.Shared
{
    public interface IHttpService
    {
        Task<T> Get<T>(string uri);
        Task Post(string uri, object value);
        Task<T> Post<T>(string uri, object value);
        Task Put(string uri, object value);
        Task<T> Put<T>(string uri, object value);
        Task Delete(string uri);
        Task<T> Delete<T>(string uri);
    }
    public class HttpService : IHttpService
    {
        private NavigationManager _navigationManager;

        private readonly ProtectedSessionStorage _sessionStorages;
        //private IConfiguration _configuration;
        private HttpClient _httpClient;
        // dates sent by the API as strings are formatted by AppConfigurationSettings:DateFormat
        private readonly JsonSerializerOptions _jsonOptions;
        // tracks running API calls so the page-open loader knows when the page data is bound
        private readonly ServerWebUI.Components.CommonClass.PageLoadingState? _pageLoading;
        public HttpService(HttpClient httpClient, NavigationManager navigationManager, IOptions<ApplicationConfiguration> settings, ProtectedSessionStorage sessionStorages = null, ServerWebUI.Components.CommonClass.PageLoadingState? pageLoading = null)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _sessionStorages = sessionStorages;
            _pageLoading = pageLoading;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _jsonOptions.Converters.Add(new DateStringConverter(settings.Value));
        }
        public async Task<T> Get<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await sendRequest<T>(request);
        }
        public async Task Post(string uri, object value)
        {
            var request = createRequest(HttpMethod.Post, uri, value);
            await sendRequest(request);
        }
        public async Task<T> Post<T>(string uri, object value)
        {

            var request = createRequest(HttpMethod.Post, uri, value);
            return await sendRequest<T>(request);
        }
        public async Task Put(string uri, object value)
        {
            var request = createRequest(HttpMethod.Put, uri, value);
            await sendRequest(request);
        }
        public async Task<T> Put<T>(string uri, object value)
        {
            var request = createRequest(HttpMethod.Put, uri, value);
            return await sendRequest<T>(request);
        }
        public async Task Delete(string uri)
        {
            var request = createRequest(HttpMethod.Delete, uri);
            await sendRequest(request);
        }
        public async Task<T> Delete<T>(string uri)
        {
            var request = createRequest(HttpMethod.Delete, uri);
            return await sendRequest<T>(request);
        }
       
        private HttpRequestMessage createRequest(HttpMethod method, string uri, object value = null)
        {
            var request = new HttpRequestMessage(method, uri);
            if (value != null)
                if (value is HttpContent httpContent)
                {
                    request.Content = httpContent;
                }
                else
                {
                    // Normal JSON request
                    request.Content = new StringContent(
                        JsonSerializer.Serialize(value, value.GetType(), _jsonOptions),
                        Encoding.UTF8,
                        "application/json");
                }
            //request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            return request;
        }
        private async Task sendRequest(HttpRequestMessage request)
        {
            _pageLoading?.RequestStarted();
            try
            {
                await addJwtHeader(request);
                using var response = await _httpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _navigationManager.NavigateTo("/");
                    return;
                }
                //await handleErrors(response);
            }
            finally
            {
                _pageLoading?.RequestEnded();
            }
        }
        //private async Task<T> sendRequest<T>(HttpRequestMessage request)
        //{
        //    await addJwtHeader(request);

        //    // send request
        //    using var response = await _httpClient.SendAsync(request);

        //    // auto logout on 401 response
        //    if (response.StatusCode == HttpStatusCode.Unauthorized)
        //    {
        //        _navigationManager.NavigateTo("/");
        //        return default;
        //    }

        //    await handleErrors(response);

        //    var options = new JsonSerializerOptions();
        //    options.PropertyNameCaseInsensitive = true;
        //    options.Converters.Add(new StringConverter());
        //    return await response.Content.ReadFromJsonAsync<T>(options);
        //}
        private async Task<T> sendRequest<T>(HttpRequestMessage request)
        {
            _pageLoading?.RequestStarted();
            try
            {
                return await sendRequestCore<T>(request);
            }
            finally
            {
                _pageLoading?.RequestEnded();
            }
        }

        private async Task<T> sendRequestCore<T>(HttpRequestMessage request)
        {
            await addJwtHeader(request);

            try
            {
                using var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _navigationManager.NavigateTo("/");
                    throw new Exception("Unauthorized");
                }

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var error = JsonSerializer.Deserialize<ApiResponse<object>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    throw new Exception(error?.Message ?? "Server error");
                }

                return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        private async Task addJwtHeader(HttpRequestMessage request)
        {
            try
            {
                var result = await _sessionStorages.GetAsync<string>("authToken");
                var token = result.Success ? result.Value : null;

                var isApiUrl = !request.RequestUri.IsAbsoluteUri;

                if (!string.IsNullOrWhiteSpace(token) && isApiUrl)
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (JSDisconnectedException)
            {
                return;
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (JSException)
            {
                return;
            }
        }

        private async Task handleErrors(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                throw new Exception(error["message"]);
            }
        }
    }

}

