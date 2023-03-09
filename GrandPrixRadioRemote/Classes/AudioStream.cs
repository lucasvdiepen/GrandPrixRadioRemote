using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundFingerprinting.Audio;
using System;
using System.Collections.Generic;
using System.IO;
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
        private AutomaticSyncer automaticSyncer = new AutomaticSyncer();

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

                var audioSamples = GetAudioSamplesWithoutDownsample(streamReader);
                if (audioSamples == null) return;

                var task = automaticSyncer.CreateFingerprintsAudioSamples(audioSamples);
                task.Wait();

                //Console.WriteLine("Samples: " + streamReader.WaveFormat.SampleRate + " Bits: " + streamReader.WaveFormat.BitsPerSample + " Channels: " + streamReader.WaveFormat.Channels);
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

        public AudioSamples GetAudioSamples(WaveStream waveStream)
        {
            int bytesToRead = waveStream.WaveFormat.AverageBytesPerSecond * 5;

            if (waveStream.Position < bytesToRead) return null;

            byte[] buffer = new byte[bytesToRead];
            waveStream.Position -= bytesToRead;
            int l = waveStream.Read(buffer, 0, buffer.Length);

            List<float> waveBuffer = new List<float>();

            using (var rawSourceStream = new RawSourceWaveStream(new MemoryStream(buffer), waveStream.WaveFormat))
            {
                //using (var downSample = new WaveFormatConversionStream(new NAudio.Wave.WaveFormat(5512, rawSourceStream.WaveFormat.BitsPerSample, rawSourceStream.WaveFormat.Channels), rawSourceStream))
                using(var downSample = new MediaFoundationResampler(rawSourceStream, new NAudio.Wave.WaveFormat(5512, rawSourceStream.WaveFormat.BitsPerSample, rawSourceStream.WaveFormat.Channels)))
                {
                    //int downSampledBytesToRead = downSample.WaveFormat.AverageBytesPerSecond * 5;
                    int downSampledBytesToRead = downSample.WaveFormat.SampleRate * 5;
                    byte[] downSampledBuffer = new byte[downSampledBytesToRead];
                    downSample.Read(downSampledBuffer, 0, downSampledBytesToRead);

                    WaveBuffer waveBuffers = new WaveBuffer(downSampledBuffer);
                    //waveBuffers.BindTo(downSampledBuffer);

                    float[] sourceArray = waveBuffers.FloatBuffer;

                    waveBuffer.AddRange(sourceArray);
                }
            }
            
            return new AudioSamples(waveBuffer.ToArray(), "GrandPrixRadioAudio", 5512);
        }

        public AudioSamples GetAudioSamplesWithoutDownsample(WaveStream waveStream)
        {
            int bytesToRead = waveStream.WaveFormat.AverageBytesPerSecond * 5;

            if (waveStream.Position < bytesToRead) return null;

            byte[] buffer = new byte[bytesToRead];
            waveStream.Position -= bytesToRead;
            int l = waveStream.Read(buffer, 0, buffer.Length);

            List<float> waveBuffer = new List<float>();

            WaveBuffer waveBuffers = new WaveBuffer(buffer);

            return new AudioSamples(waveBuffers.FloatBuffer, "GrandPrixRadioAudio", waveStream.WaveFormat.SampleRate);
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
