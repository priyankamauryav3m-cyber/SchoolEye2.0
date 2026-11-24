using System.Text;

namespace ServerWebUI.Components.CommonClass
{
    /// <summary>
    /// Writes UI logs to text files in a date-wise folder structure:
    ///   {ContentRoot}/Logs/{yyyy-MM-dd}/{FileName}.txt
    /// A new folder is created automatically for each day.
    /// Optional config key "UILog:Path" overrides the root log folder.
    /// Never pass passwords, tokens or other secrets to this service.
    /// </summary>
    public interface IUILogService
    {
        Task LogInfoAsync(string fileName, string message);
        Task LogWarningAsync(string fileName, string message);
        Task LogErrorAsync(string fileName, string message, Exception? ex = null);
    }

    public class UILogService : IUILogService
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private readonly string _rootPath;

        public UILogService(IWebHostEnvironment env, IConfiguration configuration)
        {
            var configured = configuration["UILog:Path"];
            _rootPath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(env.ContentRootPath, "Logs")
                : configured;
        }

        public Task LogInfoAsync(string fileName, string message) => WriteAsync(fileName, "INFO", message, null);

        public Task LogWarningAsync(string fileName, string message) => WriteAsync(fileName, "WARN", message, null);

        public Task LogErrorAsync(string fileName, string message, Exception? ex = null) => WriteAsync(fileName, "ERROR", message, ex);

        private async Task WriteAsync(string fileName, string level, string message, Exception? ex)
        {
            try
            {
                var now = DateTime.Now;
                var folder = Path.Combine(_rootPath, now.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(folder);

                var safeName = string.Concat((fileName ?? "General").Split(Path.GetInvalidFileNameChars()));
                if (string.IsNullOrWhiteSpace(safeName)) safeName = "General";
                var filePath = Path.Combine(folder, safeName + ".txt");

                var sb = new StringBuilder();
                sb.Append(now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(" [").Append(level).Append("] ").AppendLine(message);
                if (ex != null)
                {
                    sb.AppendLine(ex.ToString());
                }

                await _lock.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(filePath, sb.ToString());
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch
            {
                // Logging must never break the UI flow.
            }
        }
    }
}
