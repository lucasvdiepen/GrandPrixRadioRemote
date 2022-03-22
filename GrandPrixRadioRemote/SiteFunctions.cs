using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote
{
    public static class SiteFunctions
    {
        public static void TimeForward(string data)
        {
            Debug.WriteLine("Time forward requested");
        }

        public static void TimeBackward(string data)
        {
            Debug.WriteLine("Time backward requested");
        }

        public static void TimeChange(string data)
        {
            Debug.WriteLine("Time change requested");

            if (data == null) return;

            TimeRequest timeRequest = JsonConvert.DeserializeObject<TimeRequest>(data);
            Debug.WriteLine(timeRequest.time);
        }
    }
}
