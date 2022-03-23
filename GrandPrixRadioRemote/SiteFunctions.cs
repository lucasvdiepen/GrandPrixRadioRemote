using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote
{
    public class SiteFunctions
    {
        private SeleniumDriver driver;

        public SiteFunctions(SeleniumDriver driver)
        {
            this.driver = driver;
        }

        public void TimeForward(string data)
        {
            Debug.WriteLine("Time forward requested");
        }

        public void TimeBackward(string data)
        {
            Debug.WriteLine("Time backward requested");

            driver.ClickButton(XMLReaderUtility.GetWebElement("BackwardButton"));
        }

        public void Play(string data)
        {
            driver.ClickButton(XMLReaderUtility.GetWebElement("PlayButton"));
        }

        public void Pause(string data)
        {
            driver.ClickButton(XMLReaderUtility.GetWebElement("PauseButton"));
        }

        public void TimeChange(string data)
        {
            Debug.WriteLine("Time change requested");

            if (data == null) return;

            TimeRequest timeRequest = JsonConvert.DeserializeObject<TimeRequest>(data);
            Debug.WriteLine(timeRequest.time);
        }
    }
}
