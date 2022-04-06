using GrandPrixRadioRemote.DataClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GrandPrixRadioRemote.Utils;
using System.Reflection;

namespace GrandPrixRadioRemote
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new AssemblyResolverUtility().ResolveAssembly;

            Run();
        }

        private static void Run()
        {
            SeleniumDriver seleniumDriver = new SeleniumDriver("https://grandprixradio.nl/radio-luisteren");
            SiteFunctions siteFunctions = new SiteFunctions(seleniumDriver);

            Dictionary<string, Func<GetRequestData>> getListener = new Dictionary<string, Func<GetRequestData>>();
            getListener.Add("/currentvolume", siteFunctions.GetCurrentVolume);

            Dictionary<string, Action<string>> postListener = new Dictionary<string, Action<string>>();
            postListener.Add("/timeforward", siteFunctions.TimeForward);
            postListener.Add("/timebackward", siteFunctions.TimeBackward);
            postListener.Add("/timechange", siteFunctions.TimeChange);
            postListener.Add("/play", siteFunctions.Play);
            postListener.Add("/pause", siteFunctions.Pause);
            postListener.Add("/volume", siteFunctions.ChangeVolume);
            postListener.Add("/mute", siteFunctions.Mute);
            postListener.Add("/unmute", siteFunctions.Unmute);
            postListener.Add("/reload", siteFunctions.Reload);
            postListener.Add("/changestation", siteFunctions.ChangeStation);

            string[] urls = { "http://localhost:9191/", "http://*:9191/" };

            HTTPListener httpListener = new HTTPListener(urls, getListener, postListener);
        }
    }
}
