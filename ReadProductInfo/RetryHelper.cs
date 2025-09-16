using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadProductInfo
{
    public static class RetryHelper
    {
        public static async Task<T?> RunWithRetryAsync<T>(Func<Task<T>> action, int retries = 3, int delayMilliseconds = 3000)
        {
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Attempt {attempt} failed: {ex.Message}");
                    if (attempt < retries)
                        await Task.Delay(delayMilliseconds);
                }
            }

            return default;
        }

        public static async Task RunWithRetryAsync(Func<Task> action, int retries = 3, int delayMilliseconds = 3000)
        {
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Attempt {attempt} failed: {ex.Message}");
                    if (attempt < retries)
                        await Task.Delay(delayMilliseconds);
                }
            }
        }
    }


}
