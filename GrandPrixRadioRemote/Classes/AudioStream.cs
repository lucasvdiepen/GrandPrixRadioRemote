using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundFingerprinting.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        private float currentVolume = 1;

        private long previousBufferPosition = 0;

        public AudioStream(string url)
        {
            this.url = url;
            Init();
        }

        private void Init()
        {
            previousBufferPosition = 0;

            streamReader = new MediaFoundationReader(url, new MediaFoundationReader.MediaFoundationReaderSettings() { RepositionInRead = true });
            volumeSampleProvider = new VolumeSampleProvider(streamReader.ToSampleProvider());
            waveOut = new WaveOutEvent();

            waveOut.Init(volumeSampleProvider);
            waveOut.Play();
        }

        public AudioSamples GetSamples(double seconds)
        {
            double bytesToRead = streamReader.WaveFormat.AverageBytesPerSecond * seconds;
            return GetAudioSamples((long)bytesToRead);
        }

        public AudioSamples GetSamples()
        {
            long bytesToRead = streamReader.Position - previousBufferPosition;
            return GetAudioSamples(bytesToRead);
        }

        private AudioSamples GetAudioSamples(long bytesToRead)
        {
            if (streamReader.Position < bytesToRead) return null;

            /*MemoryStream memoryStream = new MemoryStream();
            streamReader.CopyTo(memoryStream);

            RawSourceWaveStream rawSourceStream = new RawSourceWaveStream(memoryStream.ToArray(), 0, (int)memoryStream.Length, streamReader.WaveFormat);*/

            RawSourceWaveStream rawSourceStream = new RawSourceWaveStream(streamReader, streamReader.WaveFormat);

            byte[] buffer = new byte[bytesToRead];
            rawSourceStream.Position -= bytesToRead;
            int l = rawSourceStream.Read(buffer, 0, buffer.Length);
            previousBufferPosition = rawSourceStream.Position;

            return AudioConverter.ReadMonoSamplesFromFile(new RawSourceWaveStream(new MemoryStream(buffer), rawSourceStream.WaveFormat), 5512, (double)l / (double)rawSourceStream.WaveFormat.AverageBytesPerSecond); ;
        }

        public void ChangePosition(double time)
        {
            //waveOut.Stop();

            double totalBytes = time * streamReader.WaveFormat.AverageBytesPerSecond;

            streamReader.Position = Math.Max(0, streamReader.Position + (long)totalBytes);

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

        public void Mute()
        {
            SetVolume(0, false);
        }

        public void Unmute()
        {
            SetVolume(currentVolume, false);
        }

        public void SetVolume(float targetVolume, bool saveVolume = true)
        {
            volumeSampleProvider.Volume = targetVolume;

            if (!saveVolume) return;

            currentVolume = targetVolume;
        }

        public void Reload()
        {
            streamReader.Dispose();
            waveOut.Dispose();

            Init();
        }
    }
}
