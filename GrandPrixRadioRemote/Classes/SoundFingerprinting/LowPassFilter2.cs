using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes.SoundFingerprinting
{
    public class LowPassFilter2 : ILowPassFilter2
    {
        private static readonly float[] LpFilter96KhzTo48Khz = new float[31]
        {
            -0.0016977f, 1.7552E-18f, 0.0029326f, -3.2716E-18f, -0.0067192f, 6.0422E-18f, 0.014071f, -9.5879E-18f, -0.026742f, 1.3296E-17f,
            0.04902f, -1.6524E-17f, -0.096782f, 1.8716E-17f, 0.31511f, 0.5f, 0.31511f, 1.8716E-17f, -0.096782f, -1.6524E-17f,
            0.04902f, 1.3296E-17f, -0.026742f, -9.5879E-18f, 0.014071f, 6.0422E-18f, -0.0067192f, -3.2716E-18f, 0.0029326f, 1.7552E-18f,
            -0.0016977f
        };

        private static readonly float[] LpFilter72KHz64 = new float[62]
        {
            0.00075736f, 0.0007019f, 0.00062634f, 0.00050156f, 0.00028968f, -4.9681E-05f, -0.00055166f, -0.0012376f, -0.0021067f, -0.0031292f,
            -0.0042414f, -0.0053443f, -0.0063058f, -0.0069665f, -0.0071501f, -0.006676f, -0.0053745f, -0.0031034f, 0.00023628f, 0.0046867f,
            0.010222f, 0.016745f, 0.024081f, 0.031989f, 0.040169f, 0.048282f, 0.055963f, 0.062852f, 0.068611f, 0.07295f,
            0.075647f, 0.076563f, 0.075647f, 0.07295f, 0.068611f, 0.062852f, 0.055963f, 0.048282f, 0.040169f, 0.031989f,
            0.024081f, 0.016745f, 0.010222f, 0.0046867f, 0.00023628f, -0.0031034f, -0.0053745f, -0.006676f, -0.0071501f, -0.0069665f,
            -0.0063058f, -0.0053443f, -0.0042414f, -0.0031292f, -0.0033443002f, -0.00055166f, -4.9681E-05f, 0.00028968f, 0.00050156f, 0.00062634f,
            0.0007019f, 0.00075736f
        };

        private static readonly float[] LpFilter336KHz128 = new float[127]
        {
            -4.258E-05f, -2.2325E-05f, -1.0539E-06f, 2.2162E-05f, 4.8302E-05f, 7.8386E-05f, 0.00011347f, 0.00015463f, 0.00020296f, 0.00025957f,
            0.00032554f, 0.00040195f, 0.00048987f, 0.00059031f, 0.00070424f, 0.00083258f, 0.00097618f, 0.0011358f, 0.0013122f, 0.0015059f,
            0.0017174f, 0.001947f, 0.0021952f, 0.002462f, 0.0027474f, 0.0030514f, 0.0033737f, 0.0037138f, 0.0040714f, 0.0044457f,
            0.0048359f, 0.0052412f, 0.0056604f, 0.0060925f, 0.006536f, 0.0069896f, 0.0074517f, 0.0079207f, 0.008395f, 0.0088726f,
            0.0093517f, 0.0098304f, 0.010307f, 0.010779f, 0.011244f, 0.011701f, 0.012147f, 0.012581f, 0.013f, 0.013403f,
            0.013787f, 0.014151f, 0.014492f, 0.014811f, 0.015103f, 0.015369f, 0.015607f, 0.015816f, 0.015994f, 0.016142f,
            0.016257f, 0.01634f, 0.01639f, 0.016406f, 0.01639f, 0.01634f, 0.016257f, 0.016142f, 0.015994f, 0.015816f,
            0.015607f, 0.015369f, 0.015103f, 0.014811f, 0.014492f, 0.014151f, 0.013787f, 0.013403f, 0.013f, 0.012581f,
            0.012147f, 0.011701f, 0.011244f, 0.010779f, 0.010307f, 0.0098304f, 0.0093517f, 0.0088726f, 0.008395f, 0.0079207f,
            0.0074517f, 0.0069896f, 0.006536f, 0.0060925f, 0.0056604f, 0.0052412f, 0.0048359f, 0.0044457f, 0.0040714f, 0.0037138f,
            0.0033737f, 0.0030514f, 0.0027474f, 0.002462f, 0.0021952f, 0.001947f, 0.0017174f, 0.0015059f, 0.0013122f, 0.0011358f,
            0.00097618f, 0.00083258f, 0.00070424f, 0.00059031f, 0.00048987f, 0.00040195f, 0.00032554f, 0.00025957f, 0.00020296f, 0.00015463f,
            0.00011347f, 7.8386E-05f, 4.8302E-05f, 2.2162E-05f, -1.0539E-06f, -2.2325E-05f, -4.258E-05f
        };

        private static readonly float[] LpFilter44 = new float[31]
        {
            -0.00064966f, -0.0014478f, -0.0027094f, -0.0044524f, -0.0062078f, -0.0069775f, -0.0053848f, 2.397E-18f, 0.010234f, 0.02559f,
            0.045288f, 0.067466f, 0.089415f, 0.10806f, 0.12059f, 0.125f, 0.12059f, 0.10806f, 0.089415f, 0.067466f,
            0.045288f, 0.02559f, 0.010234f, 2.397E-18f, -0.0053848f, -0.0069775f, -0.0062078f, -0.0044524f, -0.0027094f, -0.0014478f,
            -0.00064966f
        };

        private static readonly float[] LpFilter22 = new float[31]
        {
            -0.0012004f, -0.0020475f, -0.0020737f, 1.6358E-18f, 0.0047512f, 0.0098676f, 0.0099498f, -4.7939E-18f, -0.018909f, -0.036189f,
            -0.034662f, 8.2622E-18f, 0.068435f, 0.15283f, 0.22282f, 0.25f, 0.22282f, 0.15283f, 0.068435f, 8.2622E-18f,
            -0.034662f, -0.036189f, -0.018909f, -4.7939E-18f, 0.0099498f, 0.0098676f, 0.0047512f, 1.6358E-18f, -0.0020737f, -0.0020475f,
            -0.0012004f
        };

        private static readonly float[] LpFilter11 = new float[31]
        {
            -0.0016977f, 1.7552E-18f, 0.0029326f, -3.2716E-18f, -0.0067192f, 6.0422E-18f, 0.014071f, -9.5879E-18f, -0.026742f, 1.3296E-17f,
            0.04902f, -1.6524E-17f, -0.096782f, 1.8716E-17f, 0.31511f, 0.5f, 0.31511f, 1.8716E-17f, -0.096782f, -1.6524E-17f,
            0.04902f, 1.3296E-17f, -0.026742f, -9.5879E-18f, 0.014071f, 6.0422E-18f, -0.0067192f, -3.2716E-18f, 0.0029326f, 1.7552E-18f,
            -0.0016977f
        };

        public float[] FilterAndDownsample(float[] samples, int sourceSampleRate, int targetSampleRate)
        {
            if (targetSampleRate != 5512)
            {
                throw new ArgumentException($"Target sample {targetSampleRate} rate not supported!");
            }

            switch (sourceSampleRate)
            {
                case 96000:
                    {
                        float[] samples2 = Resample(samples, samples.Length / 2, 2, LpFilter96KhzTo48Khz);
                        return ResampleNonIntegerFactor(samples2, 7, 61, LpFilter336KHz128);
                    }
                case 48000:
                    return ResampleNonIntegerFactor(samples, 7, 61, LpFilter336KHz128);
                case 44100:
                    return Resample(samples, samples.Length / 8, 8, LpFilter44);
                case 22050:
                    return Resample(samples, samples.Length / 4, 4, LpFilter22);
                case 16000:
                    return ResampleNonIntegerFactor(samples, 21, 61, LpFilter336KHz128);
                case 11025:
                    return Resample(samples, samples.Length / 2, 2, LpFilter11);
                case 8000:
                    return ResampleNonIntegerFactor(samples, 9, 13, LpFilter72KHz64);
                case 5512:
                    return samples;
                default:
                    throw new ArgumentException($"Not supported sample rate {sourceSampleRate}");
            }
        }

        private float[] ResampleNonIntegerFactor(float[] samples, int p, int q, float[] filter)
        {
            float[] array = new float[samples.Length * p];
            for (int i = 0; i < array.Length; i += p)
            {
                array[i] = samples[i / p];
            }

            return Resample(array, array.Length / q - filter.Length, q, filter);
        }

        private float[] Resample(float[] samples, int newSamplesCount, int mult, float[] filter)
        {
            float[] array = new float[newSamplesCount];
            for (int i = 0; i < newSamplesCount; i++)
            {
                array[i] = Convolve(samples, mult * i, filter, filter.Length);
            }

            return array;
        }

        private float Convolve(float[] buffer, int begin, float[] filter, int flen)
        {
            float num = 0f;
            for (int i = 0; i < flen && begin + i < buffer.Length; i++)
            {
                num += buffer[begin + i] * filter[i];
            }

            return num;
        }
    }
}
