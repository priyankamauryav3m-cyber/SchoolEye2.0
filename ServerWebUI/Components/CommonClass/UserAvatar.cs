namespace ServerWebUI.Components.CommonClass
{
    /// <summary>
    /// Profile photo fallback: first letter of the first name + first letter of the last name
    /// ("Akshaay Singh" -> "AS"). Single word -> its first letter ("Kushabu" -> "K").
    /// </summary>
    public static class UserAvatar
    {
        public static string GetInitials(string? fullName, string? userName = null)
        {
            var name = !string.IsNullOrWhiteSpace(fullName) ? fullName : userName;
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            var parts = name.Trim().Split(new[] { ' ', '.', '_', '@', '-' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(p => char.IsLetterOrDigit(p[0]))
                            .ToArray();
            if (parts.Length == 0)
                return "?";

            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
                : parts[0][0].ToString().ToUpperInvariant();
        }
    }
}
