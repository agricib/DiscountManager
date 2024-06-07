using System.Net.Sockets;
public class Client
{
    private const string ServerAddress = "localhost";
    private const int Port = 5000;

    public static void Main(string[] args)
    {
        var client = new TcpClient(ServerAddress, Port);
        Console.WriteLine("Connected to server");

        using (var networkStream = client.GetStream())
        using (var reader = new StreamReader(networkStream))
        using (var writer = new StreamWriter(networkStream) { AutoFlush = true })
        {
            while (true)
            {
                Console.Write("Enter request (Generate count length | UseCode code): ");
                var request = Console.ReadLine();
                if (request == null || request.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                writer.WriteLine(request);
                var response = reader.ReadLine();
                Console.WriteLine("Server response: {0}", response);
            }
        }

        client.Close();
        Console.WriteLine("Disconnected from server");
    }
}