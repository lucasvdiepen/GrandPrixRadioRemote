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
        private double currentVolume = 100;

        public SiteFunctions(SeleniumDriver driver)
        {
            this.driver = driver;
        }

        public void TimeForward(string data)
        {
            Debug.WriteLine("Time forward requested");

            driver.ClickAndHoldButton(driver.GetWebElement(driver.GetBy(XMLReaderUtility.GetWebElement("ForwardButton"))), 0);
        }

        public void TimeBackward(string data)
        {
            Debug.WriteLine("Time backward requested");

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
            Debug.WriteLine("Time change requested");

            if (data == null) return;

            TimeData timeData = JsonConvert.DeserializeObject<TimeData>(data);
            Debug.WriteLine(timeData.time);
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

            return new GetRequestData(ContentTypes.Json, jsonData);
        }
    }
}
