using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthWin
{
    public class OtpSignalServer
    {
        public const int Port = 58712;

        private TcpListener listener;
        private CancellationTokenSource cts;

        public event Action ShowRequested;

        public void Start()
        {
            cts = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();
            _ = AcceptLoopAsync(cts.Token);
        }

        public void Stop()
        {
            cts?.Cancel();
            try { listener?.Stop(); } catch { }
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync();
                }
                catch
                {
                    break;
                }
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    byte[] buffer = new byte[2048];
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string request = Encoding.ASCII.GetString(buffer, 0, read);
                    string requestLine = request.Split('\n')[0];

                    if (requestLine.StartsWith("GET /show"))
                    {
                        ShowRequested?.Invoke();
                        await WriteResponseAsync(stream, 200, "OK");
                    }
                    else if (requestLine.StartsWith("OPTIONS"))
                    {
                        await WriteResponseAsync(stream, 204, "");
                    }
                    else
                    {
                        await WriteResponseAsync(stream, 404, "Not Found");
                    }
                }
            }
            catch { }
        }

        private static async Task WriteResponseAsync(NetworkStream stream, int statusCode, string body)
        {
            string statusText = statusCode == 200 ? "OK" : statusCode == 204 ? "No Content" : "Not Found";
            string response = $"HTTP/1.1 {statusCode} {statusText}\r\n" +
                "Access-Control-Allow-Origin: *\r\n" +
                "Access-Control-Allow-Methods: GET, OPTIONS\r\n" +
                "Access-Control-Allow-Headers: *\r\n" +
                "Content-Type: text/plain\r\n" +
                $"Content-Length: {Encoding.ASCII.GetByteCount(body)}\r\n" +
                "Connection: close\r\n\r\n" + body;
            byte[] respBytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(respBytes, 0, respBytes.Length);
        }
    }
}
