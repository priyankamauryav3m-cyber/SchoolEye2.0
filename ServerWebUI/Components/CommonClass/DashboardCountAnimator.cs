using System.Diagnostics;
using System.Globalization;

namespace ServerWebUI.Components.CommonClass
{
    /// <summary>
    /// Dashboard entrance + count-up animation shared by all dashboards.
    /// Usage: create one per component, call PlayAsync after data loads, render numbers with Count()/Amount().
    /// Cards wrapped in @if (animator.IsVisible) with the CSS class "dash-animate" slide in (see NewStyle.css).
    /// </summary>
    public class DashboardCountAnimator : IDisposable
    {
        private const int CardsEntranceDelayMs = 250;
        private const int CountDurationMs = 1400;
        private const int CountFrameMs = 30;
        private static readonly CultureInfo IndianCulture = new("en-IN");

        private CancellationTokenSource? _cts;

        /// <summary>Animation progress 0 -> 1. Every displayed number is target * Progress.</summary>
        public double Progress { get; private set; } = 0;

        /// <summary>False while loading, so the cards are re-created and the CSS entrance animation replays.</summary>
        public bool IsVisible { get; private set; } = false;

        /// <summary>Hide the cards (call when a new load starts).</summary>
        public void Hide()
        {
            _cts?.Cancel();
            IsVisible = false;
            Progress = 0;
        }

        /// <summary>Show the cards, then count every number from 0 up to its value.</summary>
        public async Task PlayAsync(Func<Task> render)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                Progress = 0;
                IsVisible = true;
                await render();
                await Task.Delay(CardsEntranceDelayMs, token);

                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.ElapsedMilliseconds < CountDurationMs)
                {
                    double t = stopwatch.ElapsedMilliseconds / (double)CountDurationMs;
                    Progress = 1 - Math.Pow(1 - t, 3); // ease-out cubic
                    await render();
                    await Task.Delay(CountFrameMs, token);
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }

            Progress = 1;
            await render();
        }

        public string Count(int target) => Math.Round(target * Progress).ToString("N0", IndianCulture);

        public string Amount(decimal target) => (target * (decimal)Progress).ToString("N2", IndianCulture);

        /// <summary>Counts the number inside a percentage text like "20.5%" and keeps the rest of the text.</summary>
        public string Percent(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "0%";

            var numberPart = value.Trim().TrimEnd('%').Trim();
            if (!decimal.TryParse(numberPart, NumberStyles.Number, CultureInfo.InvariantCulture, out var target))
                return value;

            int decimals = numberPart.Contains('.') ? numberPart.Length - numberPart.IndexOf('.') - 1 : 0;
            return (target * (decimal)Progress).ToString("F" + decimals, CultureInfo.InvariantCulture) + "%";
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
