using DiscountManager;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class DiscountServer
{
    private const int Port = 5000;
    private readonly DiscountCodeManager _discountManager;

    public DiscountServer(string storageFile)
    {
        _discountManager = new DiscountCodeManager(storageFile);
    }

    public static void Main(string[] args)
    {
        var server = new DiscountServer("discounts.dat");
        server.Start();
    }

    public void Start()
    {
        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine("Server started on port: {0}", Port);

        while (true)
        {
            var client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");
            HandleClient(client);
        }
    }

    private void HandleClient(TcpClient client)
    {
        using (var networkStream = client.GetStream())
        using (var reader = new StreamReader(networkStream))
        using (var writer = new StreamWriter(networkStream) { AutoFlush = true })
        {
            while (true)
            {
                var request = reader.ReadLine();
                if (request == null)
                    break;

                var response = ProcessRequest(request);
                writer.WriteLine(response);
            }
        }

        client.Close();
        Console.WriteLine("Client disconnected");
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