using System.Collections.Concurrent;

namespace ServerWebAPI.Authorization
{
    public class SessionManager
    {
        private readonly ConcurrentDictionary<string, DateTime> _lastActivity = new();

        public void UpdateActivity(string sessionId)
        {
            _lastActivity[sessionId] = DateTime.UtcNow;
        }

        public void RemoveSession(string sessionId)
        {
            _lastActivity.TryRemove(sessionId, out _);         
        }

        public bool IsSessionActive(string sessionId, TimeSpan timeout)
        {
            if (_lastActivity.TryGetValue(sessionId, out var lastActive))
            {
                return DateTime.UtcNow - lastActive < timeout;
            }
            return false;
        }

        // optional cleanup to remove old sessions
        public void CleanupExpiredSessions(TimeSpan timeout)
        {
            var now = DateTime.UtcNow;
            foreach (var session in _lastActivity)
            {
                if (now - session.Value >= timeout)
                    _lastActivity.TryRemove(session.Key, out _);
            }
        }
    }
}
