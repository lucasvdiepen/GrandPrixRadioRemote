using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes.SoundFingerprinting
{
    public class WaveFormat2
    {
        public int SampleRate { get; set; }

        public short Channels { get; set; }

        public short BitsPerSample { get; set; }

        public long Length { get; set; }

        public float LengthInSeconds => (float)Length / (float)(SampleRate * (BitsPerSample / 8) * Channels);
    }
}
