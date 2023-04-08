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

        private static DateTime oldDateTime = DateTime.Now;
        private static double timer;

        private static double timer2;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new AssemblyResolverUtility().ResolveAssembly;

            Run();
        }

        private static void Run()
        {
            //AudioStream audioStream = new AudioStream("https://eu-player-redirect.streamtheworld.com/api/livestream-redirect/GPRDANCEAAC.aac");
            //AudioStream audioStream = new AudioStream("https://eu-player-redirect.streamtheworld.com/api/livestream-redirect/GPRCLASSICSAAC.aac");
            AudioStream audioStream = new AudioStream("f1test4.wav");
            
            AutomaticSyncer automaticSyncer = new AutomaticSyncer(audioStream);

            SiteFunctions siteFunctions = new SiteFunctions(audioStream);

            Dictionary<string, Func<GetRequestData>> getListener = new Dictionary<string, Func<GetRequestData>>();
            getListener.Add("/currentvolume", siteFunctions.GetCurrentVolume);

            Dictionary<string, Action<string>> postListener = new Dictionary<string, Action<string>>();
            postListener.Add("/play", siteFunctions.Play);
            postListener.Add("/pause", siteFunctions.Pause);
            postListener.Add("/volume", siteFunctions.ChangeVolume);
            postListener.Add("/mute", siteFunctions.Mute);
            postListener.Add("/unmute", siteFunctions.Unmute);
            postListener.Add("/reload", (data) => { audioStream.Reload(); });
            postListener.Add("/audioposition", siteFunctions.AudioPositionChange);
            postListener.Add("/syncaudio", (data) => { automaticSyncer.Sync(); });

            string[] urls = { "http://localhost", "http://*" };

            Console.Clear();

            HTTPListener httpListener = new HTTPListener(urls, ConfigHelper.GetConfig().Port, getListener, postListener);

            while (true)
            {
                var currentDateTime = DateTime.Now;
                timer += (currentDateTime - oldDateTime).TotalMilliseconds;
                timer2 += (currentDateTime - oldDateTime).TotalMilliseconds;
                oldDateTime = currentDateTime;

                if (timer >= 10000)
                {
                    timer = 0;
                    timer2 = 0;

                    automaticSyncer.Sync();
                }

                if (timer2 >= 5000)
                {
                    timer2 = 0;

                    automaticSyncer.ProvideAudioData();
                }
            }

            Console.ReadLine();
        }
    }
}
