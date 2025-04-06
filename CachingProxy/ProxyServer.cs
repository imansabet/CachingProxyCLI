using System.Net;
using System.Text;

public class ProxyServer
{
    private readonly int _port;
    private readonly string _origin;
    private readonly HttpListener _listener = new();

    public ProxyServer(int port, string origin)
    {
        _port = port;
        _origin = origin.TrimEnd('/');
        _listener.Prefixes.Add($"http://localhost:{_port}/");
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine(" Proxy server is running. Press Ctrl+C to stop.");

        while (true)
        {
            var context = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleRequestAsync(context));
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        string path = context.Request.RawUrl ?? "/";
        Console.WriteLine($"> Received request: {path}");

        if (CacheService.TryGet(path, out var cached))
        {
            Console.WriteLine(" Cache HIT");
            await RespondAsync(context, cached, fromCache: true);
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CachingProxy");

            var originUrl = $"{_origin}{path}";
            var response = await client.GetAsync(originUrl);
            var body = await response.Content.ReadAsStringAsync();

            var headers = response.Headers
                .ToDictionary(h => h.Key, h => string.Join(",", h.Value));

            var cachedItem = new CachedItem
            {
                ResponseBody = body,
                Headers = headers
            };

            CacheService.Set(path, cachedItem);
            Console.WriteLine("🌐 Cache MISS → Fetched from origin");

            await RespondAsync(context, cachedItem, fromCache: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Error: {ex.Message}");
            context.Response.StatusCode = 500;
            var errorBytes = Encoding.UTF8.GetBytes("Internal Server Error");
            await context.Response.OutputStream.WriteAsync(errorBytes);
            context.Response.Close();
        }
    }

    private async Task RespondAsync(HttpListenerContext context, CachedItem item, bool fromCache)
    {
        context.Response.StatusCode = 200;

        foreach (var header in item.Headers)
        {
            try
            {
                context.Response.Headers[header.Key] = header.Value;
            }
            catch { /* بعضی هدرها قابل ست نیستن */ }
        }

        context.Response.Headers["X-Cache"] = fromCache ? "HIT" : "MISS";

        var bytes = Encoding.UTF8.GetBytes(item.ResponseBody);
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();
    }
}