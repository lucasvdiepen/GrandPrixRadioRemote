using GrandPrixRadioRemote.DataClasses;
using GrandPrixRadioRemote.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class SiteFunctions2
    {
        private AudioStream audioStream;
        private double currentVolume = 100;

        public SiteFunctions2(AudioStream audioStream)
        {
            this.audioStream = audioStream;
        }

        public void Play(string data)
        {
            audioStream.Play();
        }

        public void Pause(string data)
        {
            audioStream.Pause();
        }

        public void TimeForward(string data)
        {
            audioStream.ChangePosition((long)-0.5);
        }

        public void TimeBackward(string data)
        {
            audioStream.ChangePosition((long)0.5);
        }

        public void AudioPositionChange(string data)
        {
            if (data == null) return;

            TimeData timeData = JsonConvert.DeserializeObject<TimeData>(data);

            audioStream.ChangePosition((long)timeData.time);
        }

        public void ChangeVolume(string data)
        {
            if (data == null) return;

            VolumeData volumeData = JsonConvert.DeserializeObject<VolumeData>(data);

            currentVolume = volumeData.volume * 100;

            audioStream.SetVolume((float)currentVolume / 100f);
        }

        public GetRequestData GetCurrentVolume()
        {
            VolumeData volumeData = new VolumeData() { volume = currentVolume };
            string jsonData = JsonConvert.SerializeObject(volumeData);

            return new GetRequestData(ContentType.Json, jsonData);
        }
    }
}
