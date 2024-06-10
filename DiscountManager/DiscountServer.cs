using DiscountManager;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class DiscountServer
{
    private readonly DiscountCodeManager _discountManager;
    static async Task Main()
    {
        int port = 12345;
        string storageFile = "discounts.dat";
        var server = new DiscountServer(port, storageFile);

        try
        {
            await server.StartAsync();
            Console.WriteLine("Server started on port {0}. Press any key to stop...", port);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting server: {ex.Message}");
        }
        finally
        {
            await server.StopAsync(); // graceful shutdown
            Console.WriteLine("Server stopped.");
        }
    }

    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public DiscountServer(int port, string storageFile)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _discountManager = new DiscountCodeManager(storageFile);
    }

    public async Task StartAsync()
    {
        _listener.Start();

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _ = ProcessRequestAsync(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting connection: {ex.Message}");
            }
        }
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
    }

    private async Task ProcessRequestAsync(TcpClient client)
    {
        using (var networkStream = client.GetStream())
        using (var reader = new StreamReader(networkStream, Encoding.ASCII))
        using (var writer = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true })
        {
            try
            {
                string requestString = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(requestString))
                    return;

                string response = ProcessRequest(requestString);
                await writer.WriteLineAsync(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }

    private string ProcessRequest(string request)
    {
        var parts = request.Split(' ');
        if (parts.Length < 2)
            return "Invalid request format";

        switch (parts[0])
        {
            case "Generate":
                try
                {
                    var count = ushort.Parse(parts[1]);
                    var length = byte.Parse(parts[2]);
                    return _discountManager.GenerateCodes(count, length).Count + " codes generated";
                }
                catch (ArgumentException ex)
                {
                    return ex.Message;
                }
            case "UseCode":
                return _discountManager.UseCode(parts[1]) ? "Code applied successfully" : "Invalid code";
            default:
                return "Invalid command";
        }
    }
}