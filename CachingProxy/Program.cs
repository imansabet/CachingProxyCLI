class Program
{
    static async Task Main(string[] args)
    {
        if (args.Contains("--clear-cache"))
        {
            CacheService.Clear();
            Console.WriteLine(" Cache cleared successfully.");
            return;
        }

        int port = 0;
        string origin = "";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--port" && i + 1 < args.Length)
                int.TryParse(args[i + 1], out port);
                
            if (args[i] == "--origin" && i + 1 < args.Length)
                origin = args[i + 1];
        }

        if (port == 0 || string.IsNullOrEmpty(origin))
        {
            Console.WriteLine(" Usage: caching-proxy --port <number> --origin <url>");
            return;
        }

        Console.WriteLine($" Starting caching proxy server on http://localhost:{port}/");
        Console.WriteLine($" Origin server: {origin}");

        var server = new ProxyServer(port, origin);
        await server.StartAsync();
    }
}