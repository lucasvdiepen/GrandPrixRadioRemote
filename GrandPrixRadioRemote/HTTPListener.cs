using GrandPrixRadioRemote.Data;
using GrandPrixRadioRemote.Utils;
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
        private int requestCount = 0;
        private string pageData = "";

        private Dictionary<string, Func<GetRequestData>> getListener;
        private Dictionary<string, Action<string>> postListener;

        public HTTPListener(string[] urls, Dictionary<string, Func<GetRequestData>> getListener, Dictionary<string, Action<string>> postListener)
        {
            this.getListener = new Dictionary<string, Func<GetRequestData>>(getListener);
            this.postListener = new Dictionary<string, Action<string>>(postListener);

            StartHttpServer(urls);
        }

        private void StartHttpServer(string[] urls)
        {
            //Read html file
            pageData = File.ReadAllText("index.html");

            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            foreach (string url in urls) listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", NetworkUtility.GetLocalIPAddress() + ":9191/");

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

                //Handle GET request
                if (req.HttpMethod == "GET" && getListener.TryGetValue(req.Url.AbsolutePath, out Func<GetRequestData> getRequestData))
                {
                    GetRequestData getData = getRequestData.Invoke();

                    WriteOutput(resp, getData.data, getData.contentType);

                    continue;
                }

                //Handle POST request
                if (req.HttpMethod == "POST" && postListener.TryGetValue(req.Url.AbsolutePath, out Action<string> action)) action.Invoke(PostRequestData(req));

                WriteOutput(resp, pageData, ContentType.Html);
            }
        }

        private async void WriteOutput(HttpListenerResponse resp, string pageData, string contentType)
        {
            byte[] data = Encoding.UTF8.GetBytes(pageData);
            resp.ContentType = contentType;
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;

            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
        }
    }
}
