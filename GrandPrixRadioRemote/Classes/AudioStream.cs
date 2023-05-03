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
using System.Threading;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AudioStream
    {
        private readonly string url;
        private BufferAudioProvider bufferAudioProvider;
        private WaveOutEvent waveOut;
        private VolumeSampleProvider volumeSampleProvider;

        private float currentVolume = 1;

        private long previousBufferPosition = 0;

        private bool isInitialized;

        private bool isLoopingInitialization;
        private bool hasAskedInitializationLoop;
        public AudioStream(string url)
        {
            this.url = url;
            Init();
        }

        private void Init()
        {
            GC.Collect();

            previousBufferPosition = 0;

            try
            {
                bufferAudioProvider = new BufferAudioProvider(new MediaFoundationReader(url, new MediaFoundationReader.MediaFoundationReaderSettings() { RepositionInRead = true }), 600, 3, 120);
                bufferAudioProvider.OnStoppedUnexpectedly += () => Reload();
            }
            catch (COMException)
            {
                Console.WriteLine("Audio stream could not start. Please check your internet connection.");
                return;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Please enable your VPN.");

                // todo: Rework asking for a initialization loop

                // Ask for a initialization loop
                if(!hasAskedInitializationLoop)
                {
                    isLoopingInitialization = ConditionalInput.GetInput("Do you want to want to try again until success? (y/n): ", new string[] { "y", "yes" }, new string[] { "n", "no" });
                    hasAskedInitializationLoop = true;
                }

                if(isLoopingInitialization)
                {
                    Thread.Sleep(1000);

                    Init();
                }
                else Environment.Exit(0);

                return;
            }

            volumeSampleProvider = new VolumeSampleProvider(bufferAudioProvider.ToSampleProvider());
            volumeSampleProvider.Volume = currentVolume;

            waveOut = new WaveOutEvent();
            waveOut.Init(volumeSampleProvider);
            waveOut.Play();

            isInitialized = true;
        }

        public AudioSamples GetSamples(double seconds)
        {
            if (!isInitialized) return null;

            previousBufferPosition = bufferAudioProvider.Position;

            double bytesToRead = bufferAudioProvider.WaveFormat.AverageBytesPerSecond * seconds;
            return GetAudioSamples((long)bytesToRead);
        }

        public AudioSamples GetSamples()
        {
            if (!isInitialized) return null;

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
            if(!isInitialized) return;

            bufferAudioProvider.ChangePosition(TimeSpan.FromSeconds(time));
        }

        public void Play()
        {
            if (!isInitialized) return;

            bufferAudioProvider.Play();
        }

        public void Pause()
        {
            if (!isInitialized) return;

            bufferAudioProvider.Pause();
        }

        public void Mute()
        {
            if (!isInitialized) return;

            SetVolume(0, false);
        }

        public void Unmute()
        {
            if (!isInitialized) return;

            SetVolume(currentVolume, false);
        }

        public void SetVolume(float targetVolume, bool saveVolume = true)
        {
            if (!isInitialized) return;

            volumeSampleProvider.Volume = targetVolume;

            if (!saveVolume) return;

            currentVolume = targetVolume;
        }

        public void Reload()
        {
            isInitialized = false;

            if(bufferAudioProvider != null) bufferAudioProvider.Dispose();

            if(waveOut != null) waveOut.Dispose();

            Init();
        }
    }
}
