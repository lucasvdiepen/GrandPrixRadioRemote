using GrandPrixRadioRemote.DataClasses;
using GrandPrixRadioRemote.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GrandPrixRadioRemote
{
    public class SiteFunctions
    {
        private SeleniumDriver driver;
        private double currentVolume = 100;
        private bool isChangingTime;

        public SiteFunctions(SeleniumDriver driver)
        {
            this.driver = driver;
        }

        public void TimeForward(string data)
        {
            driver.ClickAndHoldButton(driver.GetWebElement(driver.GetBy(XMLReaderUtility.GetWebElement("ForwardButton"))), 0);
        }

        public void TimeBackward(string data)
        {
            driver.ClickAndHoldButton(driver.GetWebElement(driver.GetBy(XMLReaderUtility.GetWebElement("BackwardButton"))), 0);
        }

        public void Play(string data)
        {
            driver.ClickButton(driver.GetWebElement(driver.GetBy(XMLReaderUtility.GetWebElement("PlayButton"))));
        }

        public void Pause(string data)
        {
            driver.ClickButton(driver.GetWebElement(driver.GetBy(XMLReaderUtility.GetWebElement("PauseButton"))));
        }

        public void TimeChange(string data)
        {
            if (data == null || isChangingTime) return;

            isChangingTime = true;

            TimeData timeData = JsonConvert.DeserializeObject<TimeData>(data);

            Pause(null);

            Timer timer = new Timer();
            timer.Interval = timeData.time * 1000;
            timer.AutoReset = false;
            timer.Elapsed += (o, e) => 
            { 
                Play(null);
                isChangingTime = false;
                timer.Stop();
            };
            timer.Start();
        }

        public void ChangeVolume(string data)
        {
            if (data == null) return;

            VolumeData volumeData = JsonConvert.DeserializeObject<VolumeData>(data);

            currentVolume = volumeData.volume * 100;

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

        public void Reload(string data)
        {
            driver.Reload();
        }

        public void ChangeStation(string data)
        {
            if (data == null) return;

            StationData stationData = JsonConvert.DeserializeObject<StationData>(data);

            driver.ClickButton(driver.GetWebElements(driver.GetBy(XMLReaderUtility.GetWebElement("StationButton")))[stationData.id]);
        }

        public GetRequestData GetCurrentVolume()
        {
            VolumeData volumeData = new VolumeData() { volume = currentVolume };
            string jsonData = JsonConvert.SerializeObject(volumeData);

            return new GetRequestData(ContentType.Json, jsonData);
        }
    }
}
