using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AudioStream
    {
        private readonly string url;
        private MediaFoundationReader streamReader;
        private WasapiOut waveOut;

        public AudioStream(string url)
        {
            this.url = url;
            Init();
        }

        private void Init()
        {
            streamReader = new MediaFoundationReader(url);
            waveOut = new WasapiOut();

            waveOut.Init(streamReader);
            waveOut.Play();
        }

        public void ChangePosition(long time)
        {
            waveOut.Stop();

            streamReader.Position = Math.Max(0, streamReader.Position + time * streamReader.WaveFormat.AverageBytesPerSecond);

            waveOut.Play();
        }

        public void Play()
        {
            waveOut.Play();
        }

        public void Pause()
        {
            waveOut.Pause();
        }

        public void SetVolume(float targetVolume)
        {
            waveOut.Volume = targetVolume;
        }

        public void Reload()
        {
            streamReader.Dispose();
            waveOut.Dispose();

            Init();
        }
    }
}
