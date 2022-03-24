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

            driver.ClickButton(XMLReaderUtility.GetWebElement("ForwardButton"));
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

            TimeData timeData = JsonConvert.DeserializeObject<TimeData>(data);
            Debug.WriteLine(timeData.time);
        }

        public void ChangeVolume(string data)
        {
            if (data == null) return;

            VolumeData volumeData = JsonConvert.DeserializeObject<VolumeData>(data);

            Debug.WriteLine(volumeData.volume.ToString());

            driver.ExecuteScript("document.querySelector('." + XMLReaderUtility.GetWebElement("AudioPlayer").Name + "').volume = " + volumeData.volume.ToString().Replace(",", ".") + ";");
        }

        public void Mute(string data)
        {
            Mute(true);
        }

        public void Unmute(string data)
        {
            Mute(false);
        }

        private void Mute(bool mute)
        {
            driver.ExecuteScript("document.querySelector('." + XMLReaderUtility.GetWebElement("AudioPlayer").Name + "').muted = " + mute.ToString().ToLower() + ";");
        }
    }
}
