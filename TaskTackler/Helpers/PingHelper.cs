namespace TaskTackler.Helpers;

public static class PingHelper
{
    public static async Task<bool> PingApi(HttpClient httpClient)
    {
        try
        {
            var response = await httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error pinging API: {ex.Message}");
            return false;
        }
    }

    public static async Task PingApiWithRetry(HttpClient httpClient)
    {
        const int maxAttempts = 5;
        const int delay = 5000; // 5 seconds delay between retries

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (await PingApi(httpClient))
            {
                Console.WriteLine("API is up");
                return;
            }
            await Task.Delay(delay);
        }
        Console.WriteLine("API is down after multiple attempts");
    }
}
