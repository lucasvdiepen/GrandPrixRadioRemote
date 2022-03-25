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
            SeleniumDriver seleniumDriver = new SeleniumDriver("https://grandprixradio.nl/radio-luisteren");
            SiteFunctions siteFunctions = new SiteFunctions(seleniumDriver);

            Dictionary<string, Action<string>> listenToAdresses = new Dictionary<string, Action<string>>();
            listenToAdresses.Add("/timeforward", siteFunctions.TimeForward);
            listenToAdresses.Add("/timebackward", siteFunctions.TimeBackward);
            listenToAdresses.Add("/timechange", siteFunctions.TimeChange);
            listenToAdresses.Add("/play", siteFunctions.Play);
            listenToAdresses.Add("/pause", siteFunctions.Pause);
            listenToAdresses.Add("/volume", siteFunctions.ChangeVolume);
            listenToAdresses.Add("/mute", siteFunctions.Mute);
            listenToAdresses.Add("/unmute", siteFunctions.Unmute);
            listenToAdresses.Add("/reload", siteFunctions.Reload);
            listenToAdresses.Add("/changestation", siteFunctions.ChangeStation);

            HTTPListener httpListener = new HTTPListener("http://localhost:9191/", listenToAdresses);
        }
    }
}
