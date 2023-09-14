
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using RecaptchaV3Bypasser.Recaptcha;

namespace RecaptchaV3Bypasser.Server
{
    internal sealed class HttpServer
    {
        private TcpListener Listener;
        public bool IsStarted { get; set; } = false;
        private Thread ServerThread;
        private void ClientThread(object client)
        {
            new Client((TcpClient)client);
        }

        public HttpServer(int port)
        {

            ServerThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    Listener = new TcpListener(IPAddress.Any, port);
                    Listener.Start();
                }
                catch (SocketException ex)
                {
                    Console.Clear();
                    Console.WriteLine($"[Server Starting Error] : {ex.Message}\n[+] Press anykey to continue");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return;
                }

                IsStarted = true;

                while(IsStarted)
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    new Thread(new ParameterizedThreadStart(ClientThread)).Start(client);

                }

                Listener.Stop();

            }));
            ServerThread.IsBackground = false;
            ServerThread.Priority = ThreadPriority.Highest;
            ServerThread.Start();
        }


        protected class Client
        {
            private void SendErrorResponse(TcpClient client, int code, string message)
            {

              string CodeStr = code.ToString() + " " + ((HttpStatusCode)code).ToString();
              string Html = message + " " + CodeStr;
              string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
              byte[] Buffer = Encoding.ASCII.GetBytes(Str);

                client.GetStream().Write(Buffer, 0, Buffer.Length);
                client.Close();
            }

            public Client(TcpClient client) 
            { 
                StringBuilder builder = new StringBuilder();

                byte[] buffer = new byte[1024];

                int counter;

                while((counter = client.GetStream().Read(buffer,0,buffer.Length)) > 0)
                {
                    builder.Append(Encoding.ASCII.GetString(buffer,0,counter));
                    if (builder.ToString().IndexOf("\r\n\r\n") >= 0 || builder.Length > 4096)
                        break;
                }

                Match ReqMatch = Regex.Match(builder.ToString(), @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

                if (ReqMatch == Match.Empty)
                {
                    SendErrorResponse(client, 400,"Invalid Body");
                    return;
                }

                string requestUri = ReqMatch.Groups[1].Value;

                requestUri = Uri.EscapeUriString(requestUri);

                if (!requestUri.EndsWith("/generatetoken"))
                {
                    SendErrorResponse(client, 400, "Invalid path (Only available path is /generatetoken)");
                    return;
                }

                string token;
                try
                {
                    token = RecaptchaV3.ReloadRecaptcha();
                }
                catch(RecaptchaConnectionException ex)
                {
                    SendErrorResponse(client, 400, $"[RecaptchaConnectionException]:  {ex.Message}");
                    return;
                }
                catch(TokenGenerationException ex)
                {
                    SendErrorResponse(client, 400, $"[TokenGenerationException]:  {ex.Message}");
                    
                    return;
                }
                
                string Headers = "HTTP/1.1 200 OK\nContent-Type: " + "text/html" + "\nContent-Length: " + token.Length + "\n\n";
                byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Headers);
                byte[] body = Encoding.ASCII.GetBytes(token);
                using (NetworkStream stream = client.GetStream())
                {
                    stream.Write(HeadersBuffer, 0, HeadersBuffer.Length);
                    stream.Write(body,0,body.Length);
                    client.Close();
                }
            }
        }
    }
}
