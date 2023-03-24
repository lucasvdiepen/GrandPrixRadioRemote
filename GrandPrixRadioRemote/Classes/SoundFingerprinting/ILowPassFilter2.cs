using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes.SoundFingerprinting
{
    public interface ILowPassFilter2
    {
        float[] FilterAndDownsample(float[] samples, int sourceSampleRate, int targetSampleRate);
    }
}
