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
using GrandPrixRadioRemote.Selenium;
using GrandPrixRadioRemote.Classes;

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
            /*SeleniumDriver seleniumDriver = new SeleniumDriver("https://grandprixradio.nl/radio-luisteren");
            if (!seleniumDriver.Initialized) return;*/

            AudioStream audioStream = new AudioStream("https://eu-player-redirect.streamtheworld.com/api/livestream-redirect/GPRCLASSICSAAC.aac");

            SiteFunctions2 siteFunctions = new SiteFunctions2(audioStream);

            Dictionary<string, Func<GetRequestData>> getListener = new Dictionary<string, Func<GetRequestData>>();
            getListener.Add("/currentvolume", siteFunctions.GetCurrentVolume);

            Dictionary<string, Action<string>> postListener = new Dictionary<string, Action<string>>();
            postListener.Add("/play", siteFunctions.Play);
            postListener.Add("/pause", siteFunctions.Pause);
            postListener.Add("/volume", siteFunctions.ChangeVolume);
            //postListener.Add("/mute", siteFunctions.Mute);
            //postListener.Add("/unmute", siteFunctions.Unmute);
            postListener.Add("/reload", (data) => { audioStream.Reload(); });
            postListener.Add("/audioposition", siteFunctions.AudioPositionChange);

            string[] urls = { "http://localhost", "http://*" };

            Console.Clear();

            HTTPListener httpListener = new HTTPListener(urls, ConfigHelper.GetConfig(), getListener, postListener);
        }
    }
}
