using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundFingerprinting.Audio;
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
        private WaveOutEvent waveOut;
        private VolumeSampleProvider volumeSampleProvider;

        public AudioStream(string url)
        {
            this.url = url;
            Init();
        }

        private void Init()
        {
            streamReader = new MediaFoundationReader(url);
            volumeSampleProvider = new VolumeSampleProvider(streamReader.ToSampleProvider());
            waveOut = new WaveOutEvent();

            waveOut.Init(streamReader);
            waveOut.Play();
        }

        private DateTime oldDateTime = DateTime.Now;
        private double timer;

        public void Update()
        {
            var currentDateTime = DateTime.Now;
            timer += (currentDateTime - oldDateTime).TotalMilliseconds;
            oldDateTime = currentDateTime;

            if (timer >= 10000)
            {
                timer = 0;
                WriteSample();
            }
        }

        public void WriteSample()
        {
            Console.WriteLine("Start writing data");

            int bytesToRead = streamReader.WaveFormat.AverageBytesPerSecond * 5;
            byte[] buffer = new byte[bytesToRead];
            streamReader.Position -= bytesToRead;
            int l = streamReader.Read(buffer, 0, buffer.Length);
            using (WaveFileWriter writer = new WaveFileWriter("test.wav", streamReader.WaveFormat))
            {
                writer.Write(buffer, 0, buffer.Length);
            }

            Console.WriteLine("Wrote data");
        }

        public AudioSamples GetAudioSamples()
        {
            int bytesToRead = streamReader.WaveFormat.AverageBytesPerSecond * 5;
            byte[] buffer = new byte[bytesToRead];
            streamReader.Position -= bytesToRead;
            int l = streamReader.Read(buffer, 0, buffer.Length);

            WaveBuffer waveBuffers = new WaveBuffer(buffer.Length);
            waveBuffers.BindTo(buffer);

            return new AudioSamples(waveBuffers.FloatBuffer, "GrandPrixRadioSample", streamReader.WaveFormat.SampleRate);
        }

        public void ChangePosition(long time)
        {
            //waveOut.Stop();

            streamReader.Position = Math.Max(0, streamReader.Position + time * streamReader.WaveFormat.AverageBytesPerSecond);

            //waveOut.Play();
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
            volumeSampleProvider.Volume = targetVolume;
        }

        public void Reload()
        {
            streamReader.Dispose();
            waveOut.Dispose();

            Init();
        }
    }
}
