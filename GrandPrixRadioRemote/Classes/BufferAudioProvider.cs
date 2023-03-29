using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class BufferAudioProvider : IWaveProvider
    {
        public WaveFormat WaveFormat => waveStream.WaveFormat;

        public long Position => position + positionToAdd;

        private WaveStream waveStream;
        private byte[] buffer;
        private long position;
        private long positionToAdd;

        public void Init(WaveStream waveStream)
        {
            this.waveStream = waveStream;

            //Start reader thread
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            position += positionToAdd;
            positionToAdd = 0;

            //Do read functionality
            throw new NotImplementedException();
        }

        private byte[] GetSample()
        {
            throw new NotImplementedException();
        }

        private void AddSamples()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void ChangePosition(long value)
        {
            positionToAdd += value;
        }
    }
}
