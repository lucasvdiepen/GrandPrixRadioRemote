using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundFingerprinting.Audio;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AudioStream
    {
        private readonly string url;
        private MediaFoundationReader streamReader;
        private BufferAudioProvider bufferAudioProvider;
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

            bufferAudioProvider = new BufferAudioProvider(streamReader, 20);

            volumeSampleProvider = new VolumeSampleProvider(bufferAudioProvider.ToSampleProvider());

            waveOut = new WaveOutEvent();
            waveOut.Init(volumeSampleProvider);
            waveOut.Play();
        }

        public AudioSamples GetSamples(double seconds)
        {
            previousBufferPosition = bufferAudioProvider.Position;

            double bytesToRead = bufferAudioProvider.WaveFormat.AverageBytesPerSecond * seconds;
            return GetAudioSamples((long)bytesToRead);
        }

        public AudioSamples GetSamples()
        {
            // todo: we need a new way for getting the end of the buffer
            long position = bufferAudioProvider.Position;

            long bytesToRead = position - previousBufferPosition;

            if(position < previousBufferPosition)
            {
                bytesToRead += bufferAudioProvider.Length;
            }

            previousBufferPosition = position;

            return GetAudioSamples(bytesToRead);
        }

        private AudioSamples GetAudioSamples(long bytesToRead)
        {
            byte[] samples = bufferAudioProvider.GetSamples(bufferAudioProvider.Position - bytesToRead, bytesToRead);
            return AudioConverter.ReadMonoSamplesFromFile(new RawSourceWaveStream(new MemoryStream(samples), bufferAudioProvider.WaveFormat), 5512, (double)bytesToRead / (double)bufferAudioProvider.WaveFormat.AverageBytesPerSecond);
        }

        public void ChangePosition(double time)
        {
            bufferAudioProvider.ChangePosition(TimeSpan.FromSeconds(time));
        }

        public void Play()
        {
            bufferAudioProvider.Play();
        }

        public void Pause()
        {
            bufferAudioProvider.Pause();
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
