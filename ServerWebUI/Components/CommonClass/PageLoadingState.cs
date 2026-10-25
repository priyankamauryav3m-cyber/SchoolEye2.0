namespace ServerWebUI.Components.CommonClass
{
    /// <summary>
    /// Full-page loader shown when a page is opened (menu / link navigation) until its first data is bound.
    /// HttpService reports every API call here; the loader hides once no call has been running for a short quiet time.
    /// Scoped = one instance per user circuit.
    /// </summary>
    public class PageLoadingState : IDisposable
    {
        // Minimum time the loader stays, so the page gets time to start its first API calls
        private const int MinVisibleMs = 400;
        // Hide only after no API call has been running for this long (pages often make calls one after another)
        private const int QuietMs = 250;
        // Safety cap - never block the screen longer than this
        private const int MaxVisibleMs = 15000;
        private const int CheckIntervalMs = 50;

        private int _pendingRequests;
        private CancellationTokenSource? _cts;

        public bool IsVisible { get; private set; }

        public event Action? OnChange;

        /// <summary>Called on page navigation: show the loader and hide it when the page data has loaded.</summary>
        public void StartPageLoad()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            IsVisible = true;
            OnChange?.Invoke();
            _ = WaitForPageDataAsync(_cts.Token);
        }

        public void RequestStarted() => Interlocked.Increment(ref _pendingRequests);

        public void RequestEnded()
        {
            if (Interlocked.Decrement(ref _pendingRequests) < 0)
                Interlocked.Exchange(ref _pendingRequests, 0);
        }

        private async Task WaitForPageDataAsync(CancellationToken token)
        {
            try
            {
                var startedAt = DateTime.UtcNow;
                await Task.Delay(MinVisibleMs, token);

                var quietSince = DateTime.UtcNow;
                while ((DateTime.UtcNow - startedAt).TotalMilliseconds < MaxVisibleMs)
                {
                    if (Volatile.Read(ref _pendingRequests) > 0)
                        quietSince = DateTime.UtcNow;
                    else if ((DateTime.UtcNow - quietSince).TotalMilliseconds >= QuietMs)
                        break;

                    await Task.Delay(CheckIntervalMs, token);
                }
            }
            catch (TaskCanceledException)
            {
                // A newer navigation took over
                return;
            }

            IsVisible = false;
            OnChange?.Invoke();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
