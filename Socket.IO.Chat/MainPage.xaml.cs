using SocketIOClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using SocketIOClient.WebSocketClient;
using System.Text;



namespace Socket.IO.Chat
{

    public sealed partial class MainPage : Page
    {
        SocketIO client = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Init1().Wait(); //your sample from repository inserted more or less into an UWP project
            await Init2(); //very simple attempt to connect to localhost:3000
        }

        private async Task Init2()
        {
            client = new SocketIO("http://localhost:3000", new SocketIOOptions
            {
                EIO = 4,
                ConnectionTimeout = TimeSpan.FromSeconds(5)
            });
            client.On("chat message", response =>
            {
                string text = response.GetValue<string>();
            });
            client.On("connection", response =>
            {
                string text = response.GetValue<string>();
            });
            client.OnConnected += async (s, ev) =>
            {
                await client.EmitAsync("chat message", ".net core");
            };
            try
            {
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                string h = ex.ToString();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await client.EmitAsync("chat message", ".net core");
        }

        private async Task Init1()
        {
            //var client = new SocketIO(endpointUrl);
            //client.OnConnected += async (s, ev) =>
            //{
            //	await client.EmitAsync("ack", response =>
            //	{
            //		var result = response.GetValue();
            //	}, ".net core");
            //};
            //await client.ConnectAsync();


            //var uri = new Uri("http://localhost:11000");
            var uri = new Uri("http://localhost:3000");

            var socket = new SocketIO(uri, new SocketIOOptions
            {
                Query = new Dictionary<string, string>
                {
                    {"token", "io" }
                },
                //EnabledSslProtocols = System.Security.Authentication.SslProtocols.None,
                //RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                //{
                //    if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                //    {
                //        return true;
                //    }
                //    return false;
                //}
            });

            var client = socket.Socket as ClientWebSocket;
            //client.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            //{
            //    Console.WriteLine("SslPolicyErrors: " + sslPolicyErrors);
            //    if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            //    {
            //        return true;
            //    }
            //    return true;
            //};

            socket.OnConnected += Socket_OnConnected;
            socket.OnPing += Socket_OnPing;
            socket.OnPong += Socket_OnPong;
            socket.OnDisconnected += Socket_OnDisconnected;
            socket.OnReconnecting += Socket_OnReconnecting;
            await socket.ConnectAsync();

            socket.On("chat message", response =>
            {
                Console.WriteLine($"server: {response.GetValue<string>()}");
            });

            socket.On("bytes", response =>
            {
                var bytes = response.GetValue<ByteResponse>();
                Console.WriteLine($"bytes.Source = {bytes.Source}");
                Console.WriteLine($"bytes.ClientSource = {bytes.ClientSource}");
                Console.WriteLine($"bytes.Buffer.Length = {bytes.Buffer.Length}");
                Console.WriteLine($"bytes.Buffer.ToString() = {Encoding.UTF8.GetString(bytes.Buffer)}");
            });
            socket.OnReceivedEvent += (sender, e) =>
            {
                if (e.Event == "bytes")
                {
                    var bytes = e.Response.GetValue<ByteResponse>();
                    Console.WriteLine($"OnReceivedEvent.Source = {bytes.Source}");
                    Console.WriteLine($"OnReceivedEvent.ClientSource = {bytes.ClientSource}");
                    Console.WriteLine($"OnReceivedEvent.Buffer.Length = {bytes.Buffer.Length}");
                    Console.WriteLine($"OnReceivedEvent.Buffer.ToString() = {Encoding.UTF8.GetString(bytes.Buffer)}");
                }
            };

        }


        private static void Socket_OnReconnecting(object sender, int e)
        {
            Console.WriteLine($"Reconnecting: attempt = {e}");
        }

        private static void Socket_OnDisconnected(object sender, string e)
        {
            Console.WriteLine("disconnect: " + e);
        }

        private static async void Socket_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Socket_OnConnected");
            var socket = sender as SocketIO;
            Console.WriteLine("Socket.Id:" + socket.Id);
            await socket.EmitAsync("hi", "SocketIOClient.Sample");

            //await socket.EmitAsync("ack", response =>
            //{
            //    Console.WriteLine(response.ToString());
            //}, "SocketIOClient.Sample");

            //await socket.EmitAsync("bytes", "c#", new
            //{
            //    source = "client007",
            //    bytes = Encoding.UTF8.GetBytes("dot net")
            //});

            //socket.On("client binary callback", async response =>
            //{
            //    await response.CallbackAsync();
            //});

            //await socket.EmitAsync("client binary callback", Encoding.UTF8.GetBytes("SocketIOClient.Sample"));

            //socket.On("client message callback", async response =>
            //{
            //    await response.CallbackAsync(Encoding.UTF8.GetBytes("CallbackAsync();"));
            //});
            //await socket.EmitAsync("client message callback", "SocketIOClient.Sample");
        }

        private static void Socket_OnPing(object sender, EventArgs e)
        {
            Console.WriteLine("Ping");
        }

        private static void Socket_OnPong(object sender, TimeSpan e)
        {
            Console.WriteLine("Pong: " + e.TotalMilliseconds);
        }
    }

    class ByteResponse
    {
        public string ClientSource { get; set; }

        public string Source { get; set; }

        [JsonProperty("bytes")]
        public byte[] Buffer { get; set; }
    }

    class ClientCallbackResponse
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("bytes")]
        public byte[] Bytes { get; set; }
    }
}

