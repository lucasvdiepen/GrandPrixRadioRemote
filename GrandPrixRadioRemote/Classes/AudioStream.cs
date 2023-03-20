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

        public void WriteSample(string filename, int seconds)
        {
            int bytesToRead = streamReader.WaveFormat.AverageBytesPerSecond * seconds;
            WriteStreamToFile(filename, bytesToRead);
        }

        public void WriteSample(string filename)
        {
            long bytesToRead = streamReader.Position - previousBufferPosition;
            WriteStreamToFile(filename, bytesToRead);
        }

        private void WriteStreamToFile(string filename, long bytesToRead)
        {
            byte[] buffer = new byte[bytesToRead];

            RawSourceWaveStream rawSourceStream = new RawSourceWaveStream(streamReader, streamReader.WaveFormat);

            try
            {
                rawSourceStream.Position -= bytesToRead;
                int l = rawSourceStream.Read(buffer, 0, buffer.Length);
                previousBufferPosition = rawSourceStream.Position;

                using (WaveFileWriter writer = new WaveFileWriter(filename, rawSourceStream.WaveFormat))
                {
                    writer.Write(buffer, 0, buffer.Length);
                }
            }
            catch (COMException)
            {
                rawSourceStream.Position += bytesToRead;
            }
        }

        public AudioSamples GetAudioSamples()
        {
            int bytesToRead = streamReader.WaveFormat.AverageBytesPerSecond * 5;

            if (streamReader.Position < bytesToRead) return null;

            RawSourceWaveStream rawSourceStream = new RawSourceWaveStream(streamReader, streamReader.WaveFormat);

            byte[] buffer = new byte[bytesToRead];
            rawSourceStream.Position -= bytesToRead;
            int l = rawSourceStream.Read(buffer, 0, buffer.Length);

            //return new AudioSamples(waveBuffer.FloatBuffer, "GrandPrixRadioAudio", 5512);
            return new AudioSamples(SamplesConverter.GetFloatSamplesFromByte(l, buffer), "GrandPrixRadioAudio", 5512);
        }

        /*public AudioSamples GetAudioSamplesWithoutDownsample(WaveStream waveStream)
        {
            int bytesToRead = waveStream.WaveFormat.AverageBytesPerSecond * 5;

            if (waveStream.Position < bytesToRead) return null;

            byte[] buffer = new byte[bytesToRead];
            waveStream.Position -= bytesToRead;
            int l = waveStream.Read(buffer, 0, buffer.Length);

            List<float> waveBuffer = new List<float>();

            WaveBuffer waveBuffers = new WaveBuffer(buffer);

            return new AudioSamples(waveBuffers.FloatBuffer, "GrandPrixRadioAudio", waveStream.WaveFormat.SampleRate);
        }*/

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
