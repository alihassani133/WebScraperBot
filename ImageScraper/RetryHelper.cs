using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    internal sealed class RetryHelper
    {
        internal sealed class GiveUpException : Exception
        {
            public GiveUpException(string message, Exception inner) : base(message, inner) { }
        }

        private readonly int _maxAttempts;
        private readonly TimeSpan _delay;

        public RetryHelper(int maxAttempts, TimeSpan delay)
        {
            _maxAttempts = Math.Max(1, maxAttempts);
            _delay = delay;
        }

        public async Task ExecuteAsync(Func<Task> action) =>
            await ExecuteAsync<object?>(async () => { await action(); return null; });

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
        {
            Exception? last = null;

            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex) when (IsTransient(ex))
                {
                    last = ex;

                    if (attempt == _maxAttempts)
                        break;

                    Console.WriteLine($"    Retry {attempt}/{_maxAttempts - 1} after error: {ex.Message}");

                    await Task.Delay(_delay);

                }
            }

            throw new GiveUpException($"Operation failed after {_maxAttempts} attempts.", last ?? new Exception("Unknown error"));
        }
        private static bool IsTransient(Exception ex)
        {
            var m = ex.Message ?? "";
            return ex is TimeoutException
                || ex is TaskCanceledException
                || ex is HttpRequestException
                || m.Contains("net::", StringComparison.OrdinalIgnoreCase)
                || m.Contains("Target closed", StringComparison.OrdinalIgnoreCase)
                || m.Contains("HTTP 5", StringComparison.OrdinalIgnoreCase)   // 5xx
                || m.Contains("HTTP 429", StringComparison.OrdinalIgnoreCase); // rate limit

        }
    }
}
