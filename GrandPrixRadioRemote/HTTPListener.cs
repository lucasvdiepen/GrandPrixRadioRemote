using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote
{
    public class HTTPListener
    {
        private HttpListener listener;
        private string url = "";
        private int requestCount = 0;
        private string pageData = "";

        private Dictionary<string, Action<string>> listenToAdresses;

        public HTTPListener(string url, Dictionary<string, Action<string>> listenToAdresses)
        {
            this.url = url;
            this.listenToAdresses = new Dictionary<string, Action<string>>(listenToAdresses);

            StartHttpServer();
        }

        private void StartHttpServer()
        {
            //Read html file
            pageData = File.ReadAllText("index.html");

            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Prefixes.Add("http://127.0.0.1:8080/");
            //listener.Prefixes.Add("http://+:8080/");
            //listener.Prefixes.Add("http://*:8080/");
            //listener.Prefixes.Add("http://192.168.178.25:8080/");
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }

        private string PostRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                Console.WriteLine("No client data was sent with the request.");
                return null;
            }

            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);

            if (request.ContentType != null)
            {
                Console.WriteLine("Client data content type {0}", request.ContentType);
            }

            Console.WriteLine("Client data content length {0}", request.ContentLength64);

            Console.WriteLine("Start of client data:");
            string s = reader.ReadToEnd();
            Console.WriteLine(s);
            Console.WriteLine("End of client data:");

            body.Close();
            reader.Close();

            return s;
        }

        private async Task HandleIncomingConnections()
        {
            while (true)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();

                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                //Handle listeners
                if (req.HttpMethod == "POST" && listenToAdresses.TryGetValue(req.Url.AbsolutePath, out Action<string> action)) action.Invoke(PostRequestData(req));

                byte[] data = Encoding.UTF8.GetBytes(pageData);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }
    }
}
