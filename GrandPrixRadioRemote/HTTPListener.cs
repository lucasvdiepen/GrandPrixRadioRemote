using GrandPrixRadioRemote.DataClasses;
using GrandPrixRadioRemote.Enums;
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
        private string pageData = "";
        private bool isRunning = true;

        private Dictionary<string, Func<GetRequestData>> getListener;
        private Dictionary<string, Action<string>> postListener;

        public HTTPListener(string[] urls, Config config, Dictionary<string, Func<GetRequestData>> getListener, Dictionary<string, Action<string>> postListener)
        {
            this.getListener = new Dictionary<string, Func<GetRequestData>>(getListener);
            this.postListener = new Dictionary<string, Action<string>>(postListener);

            StartHttpServer(urls, config);
        }

        private void Stop()
        {
            isRunning = false;
        }

        private void StartHttpServer(string[] urls, Config config)
        {
            //Read html file
            pageData = EmbeddedFileReaderUtility.ReadFile(FilePath.Index);

            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            foreach (string url in urls) listener.Prefixes.Add(url + ":" + config.Port + "/");
            listener.Start();

            Console.WriteLine("Listening for connections on " + NetworkUtility.GetLocalIPAddress() + ":" + config.Port);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }

        private string PostRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody) return null;

            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);

            string s = reader.ReadToEnd();

            body.Close();
            reader.Close();

            return s;
        }

        private async Task HandleIncomingConnections()
        {
            while (isRunning)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();

                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                //Handle GET request
                if (req.HttpMethod == "GET" && getListener.TryGetValue(req.Url.AbsolutePath, out Func<GetRequestData> getRequestData))
                {
                    GetRequestData getData = getRequestData.Invoke();

                    //Write requested data
                    WriteOutput(resp, getData.data, getData.contentType);

                    continue;
                }

                //Handle POST request
                if (req.HttpMethod == "POST" && postListener.TryGetValue(req.Url.AbsolutePath, out Action<string> action)) action.Invoke(PostRequestData(req));

                //Write default page
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
