using GrandPrixRadioRemote.Classes.SoundFingerprinting;
using NAudio.Wave;
using SoundFingerprinting.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AudioConverter
    {
        private ILowPassFilter2 lowPassFilter;
        private IAudioSamplesNormalizer audioSamplesNormalizer;

        public AudioConverter()
        {
            lowPassFilter = new LowPassFilter2();
            audioSamplesNormalizer = new AudioSamplesNormalizer();
        }

        public AudioSamples ReadMonoSamplesFromFile(RawSourceWaveStream streamReader, int sampleRate, double seconds)
        {
            //WaveFormat waveFormat = WaveFormat.FromFile(pathToSourceFile);
            WaveFormat2 waveFormat = new WaveFormat2
            {
                Channels = (short)streamReader.WaveFormat.Channels,
                SampleRate = streamReader.WaveFormat.SampleRate,
                BitsPerSample = (short)streamReader.WaveFormat.BitsPerSample,
                Length = streamReader.Length // todo: might need to add -44 here
            };

            float[] samples = ToSamples(streamReader, waveFormat, seconds, 0);
            float[] monoSamples = ToMonoSamples(samples, waveFormat);
            float[] samples2 = ToTargetSampleRate(monoSamples, waveFormat.SampleRate, sampleRate);
            audioSamplesNormalizer.NormalizeInPlace(samples2);
            return new AudioSamples(samples2, string.Empty, sampleRate);
        }

        private float[] ToTargetSampleRate(float[] monoSamples, int sourceSampleRate, int sampleRate)
        {
            return lowPassFilter.FilterAndDownsample(monoSamples, sourceSampleRate, sampleRate);
        }

        private float[] ToMonoSamples(float[] samples, WaveFormat2 format)
        {
            if (format.Channels == 1)
            {
                return samples;
            }

            float[] array = new float[samples.Length / 2];
            int num = 0;
            int num2 = 0;
            while (num < samples.Length - 1)
            {
                int num3 = num;
                int num4 = num + 1;
                array[num2] = (samples[num3] + samples[num4]) / 2f;
                num += 2;
                num2++;
            }

            return array;
        }

        private float[] ToSamples(WaveStream streamReader, WaveFormat2 format, double seconds, double startsAt)
        {
            streamReader.Seek(44L, SeekOrigin.Begin);
            int num = (int)(startsAt * (double)format.SampleRate * (double)format.Channels);
            int num2 = format.BitsPerSample / 8;
            streamReader.Seek(num2 * num, SeekOrigin.Current);
            return GetInts(streamReader, format, seconds, startsAt);
        }

        private float[] GetInts(Stream reader, WaveFormat2 format, double seconds, double startsAt)
        {
            int num = format.BitsPerSample / 8;
            long samplesToRead = GetSamplesToRead(format, seconds, startsAt);
            byte[] array = new byte[num];

            int num2 = 0;
            if(num == 1)
            {
                num2 = 127;
            }
            else if(num == 2)
            {
                num2 = 32767;
            }
            else if (num == 3)
            {
                num2 = (int)System.Math.Pow(2.0, 24.0) / 2 - 1;
            }
            else
            {
                num2 = int.MaxValue;
            }

            int num3 = 0;
            float[] array2 = new float[samplesToRead];
            while (reader.CanRead && num3 < samplesToRead)
            {
                if (reader.Read(array, 0, num) != num)
                {
                    return array2;
                }

                switch (num)
                {
                    case 1:
                        array2[num3] = (float)(int)array[0] / (float)num2;
                        break;
                    case 2:
                        {
                            short num6 = (short)(array[0] | (array[1] << 8));
                            array2[num3] = (float)num6 / (float)num2;
                            break;
                        }
                    case 3:
                        {
                            int num5 = array[0] | (array[1] << 8) | (array[2] << 16);
                            array2[num3] = (float)num5 / (float)num2;
                            break;
                        }
                    default:
                        {
                            int num4 = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
                            array2[num3] = (float)num4 / (float)num2;
                            break;
                        }
                }

                array2[num3] = System.Math.Min(1f, array2[num3]);
                num3++;
            }

            return array2;
        }

        private static long GetSamplesToRead(WaveFormat2 format, double seconds, double startsAt)
        {
            int num = format.SampleRate * format.Channels;
            int num2 = ((System.Math.Abs(seconds) < 0.001) ? int.MaxValue : ((int)seconds * num));
            int num3 = format.BitsPerSample / 8;
            int num4 = (int)format.Length / num3 - (int)startsAt * num;
            if (num4 > num2)
            {
                return num2;
            }

            return num4;
        }
    }
}
