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

            try
            {
                streamReader.Position -= bytesToRead;
                int l = streamReader.Read(buffer, 0, buffer.Length);
                previousBufferPosition = streamReader.Position;

                using (WaveFileWriter writer = new WaveFileWriter(filename, streamReader.WaveFormat))
                {
                    writer.Write(buffer, 0, buffer.Length);
                }
            }
            catch (COMException)
            {
                streamReader.Position += bytesToRead;
            }
        }

        /*public AudioSamples GetAudioSamples()
        {
            int bytesToRead = streamReader.WaveFormat.AverageBytesPerSecond * 5;

            if (streamReader.Position < bytesToRead) return null;

            byte[] buffer = new byte[bytesToRead];
            streamReader.Position -= bytesToRead;
            int l = streamReader.Read(buffer, 0, buffer.Length);

            //List<float> floatBuffer = new List<float>();
            //WaveBuffer waveBuffer;
            float[] floatBuffer;

            using (var rawSourceStream = new RawSourceWaveStream(new MemoryStream(buffer), streamReader.WaveFormat))
            {
                //using (var downSample = new WaveFormatConversionStream(new NAudio.Wave.WaveFormat(5512, rawSourceStream.WaveFormat.BitsPerSample, rawSourceStream.WaveFormat.Channels), rawSourceStream))
                using(var downSample = new MediaFoundationResampler(rawSourceStream, new NAudio.Wave.WaveFormat(5512, rawSourceStream.WaveFormat.BitsPerSample, rawSourceStream.WaveFormat.Channels)))
                {
                    //int downSampledBytesToRead = downSample.WaveFormat.AverageBytesPerSecond * 5;
                    int downSampledBytesToRead = downSample.WaveFormat.SampleRate * 5;
                    byte[] downSampledBuffer = new byte[downSampledBytesToRead];
                    downSample.Read(downSampledBuffer, 0, downSampledBytesToRead);

                    var newBuffer = MemoryMarshal.Cast<byte, float>(downSampledBuffer);
                    float[] newFloatBuffer = new float[newBuffer.Length];

                    for(int i = 0; i < newBuffer.Length; i++)
                    {
                        waveBuffer.Add(newBuffer[i]);
                    }

                    WaveBuffer waveBuffer = new WaveBuffer(downSampledBuffer);
                    float[] floatBufferTemp = waveBuffer.FloatBuffer;
                    //floatBuffer = (float[])waveBuffer.FloatBuffer.Clone();
                    //floatBuffer.AddRange(waveBuffer.FloatBuffer);

                    //floatBuffer = SamplesConverter.GetFloatSamplesFromByte(downSampledBuffer.Length, downSampledBuffer);
                    float[] sourceArray = waveBuffers.FloatBuffer;

                    waveBuffer.AddRange(sourceArray);

                    floatBuffer = new float[floatBufferTemp.Length];

                    for (int i = 0; i < floatBuffer.Length; i++)
                    {
                        floatBuffer[i] = floatBufferTemp[i] / 255;
                    }
                }
            }

            //return new AudioSamples(waveBuffer.FloatBuffer, "GrandPrixRadioAudio", 5512);
            return new AudioSamples(floatBuffer, "GrandPrixRadioAudio", 5512);
        }*/

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
