using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;

namespace ServerWebUI.Components.CommonClass
{
    // Shows page routes encrypted in the address bar: /ViewStudentsLedger -> /p/CfDJ8...
    // Nothing is stored in the database: the token itself is the encrypted path
    // (ASP.NET Core Data Protection, keys already persisted in Program.cs).
    // Only the page name is encrypted; a query string stays as it is.
    public class SecureUrlService
    {
        public const string Prefix = "/p/";

        private readonly IDataProtector _protector;
        private readonly ConcurrentDictionary<string, string> _tokenByPath = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, string> _pathByToken = new(StringComparer.Ordinal);
        private readonly Lazy<Dictionary<string, Type>> _pages = new(BuildPageTable);

        public SecureUrlService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("SchoolEye.SecurePageUrl.v1");
        }

        // "/ViewStudentsLedger?x=1" -> "/p/{token}?x=1"; anything that is not an app page is returned unchanged
        public string ToSecure(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url ?? string.Empty;

            var (path, query) = Split(url);
            if (path.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) || !_pages.Value.ContainsKey(path))
                return url;

            // one token per page while the app runs, so links and NavLink "active" stay stable
            var token = _tokenByPath.GetOrAdd(path, p =>
            {
                var t = _protector.Protect(p.ToLowerInvariant());
                _pathByToken[t] = p;
                return t;
            });
            return Prefix + token + query;
        }

        // full uri / relative url (encrypted or not) -> real page path with query, e.g. "/ViewStudentsLedger"
        public string ToRealPath(string? uriOrPath)
        {
            if (string.IsNullOrWhiteSpace(uriOrPath))
                return string.Empty;

            var relative = uriOrPath;
            if (Uri.TryCreate(uriOrPath, UriKind.Absolute, out var abs))
                relative = abs.PathAndQuery;

            var (path, query) = Split(relative);
            if (!path.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                return path + query;

            return TryDecrypt(path.Substring(Prefix.Length), out var real) ? real + query : path + query;
        }

        public bool TryDecrypt(string? token, out string realPath)
        {
            realPath = string.Empty;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            if (_pathByToken.TryGetValue(token, out var cached))
            {
                realPath = cached;
                return true;
            }
            try
            {
                // token from an earlier run (bookmark / refresh after restart)
                var path = _protector.Unprotect(token);
                if (!_pages.Value.ContainsKey(path))
                    return false;
                realPath = path;
                return true;
            }
            catch
            {
                return false;   // tampered or foreign token
            }
        }

        public bool IsSecurablePage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            var (path, _) = Split(url);
            return _pages.Value.ContainsKey(path);
        }

        public Type? GetPageType(string realPath)
        {
            var (path, _) = Split(realPath);
            return _pages.Value.TryGetValue(path, out var type) ? type : null;
        }

        private static (string Path, string Query) Split(string url)
        {
            var relative = url;
            if (Uri.TryCreate(url, UriKind.Absolute, out var abs))
                relative = abs.PathAndQuery;

            var q = relative.IndexOfAny(new[] { '?', '#' });
            var path = q >= 0 ? relative[..q] : relative;
            var query = q >= 0 ? relative[q..] : string.Empty;
            path = "/" + path.Trim().Trim('/');
            return (path, query);
        }

        // app pages that can be encrypted: fixed routes (no {parameters}) using the main layout.
        // Login / public pages (LoginLayout) and "/" stay as they are.
        private static Dictionary<string, Type> BuildPageTable()
        {
            var table = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var type in typeof(SecureUrlService).Assembly.GetTypes())
            {
                if (!typeof(IComponent).IsAssignableFrom(type) || type.IsAbstract)
                    continue;
                if (type.GetCustomAttribute<LayoutAttribute>() != null)
                    continue;

                foreach (var route in type.GetCustomAttributes<RouteAttribute>())
                {
                    var template = "/" + route.Template.Trim().Trim('/');
                    if (template == "/" || template.Contains('{'))
                        continue;
                    table.TryAdd(template, type);
                }
            }
            return table;
        }
    }
}
