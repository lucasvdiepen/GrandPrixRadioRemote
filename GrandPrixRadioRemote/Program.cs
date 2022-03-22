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
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, Action<string>> listenToAdresses = new Dictionary<string, Action<string>>();
            listenToAdresses.Add("/timeforward", SiteFunctions.TimeForward);
            listenToAdresses.Add("/timebackward", SiteFunctions.TimeBackward);
            listenToAdresses.Add("/timechange", SiteFunctions.TimeChange);

            HTTPListener httpListener = new HTTPListener("http://localhost:8000/", listenToAdresses);
        }
    }
}
