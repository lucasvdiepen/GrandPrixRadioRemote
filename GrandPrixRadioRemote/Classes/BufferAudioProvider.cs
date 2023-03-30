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
        private long writePosition;
        private long positionToAdd;
        private Task readerTask;

        public BufferAudioProvider(WaveStream waveStream, int bufferSize = 2646000, double bufferTime = 5)
        {
            this.waveStream = waveStream;

            buffer = new byte[bufferSize];

            //Start reader thread
            readerTask = Task.Factory.StartNew(ReadSource);
        }

        private Task ReadSource()
        {
            while(true)
            {
                byte[] buffer = new byte[1024];
                int l = waveStream.Read(buffer, 0, buffer.Length);

                AddSamples(buffer, 0, l);
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            position += positionToAdd;
            positionToAdd = 0;

            //Do read functionality

            long bytesRead = Math.Min(count, this.buffer.Length - position);
            Array.Copy(this.buffer, position, buffer, offset, bytesRead);
            position += bytesRead;
            return (int)bytesRead;
        }

        private byte[] GetSample()
        {
            throw new NotImplementedException();
        }

        private void AddSamples(byte[] buffer, int offset, int count)
        {
            long bytesToAdd = Math.Min(count, this.buffer.Length - writePosition);
            Array.Copy(buffer, offset, this.buffer, writePosition, bytesToAdd);
            writePosition += bytesToAdd;
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void ChangePosition(TimeSpan time)
        {
            long value = (long)(WaveFormat.AverageBytesPerSecond * time.TotalSeconds);

            positionToAdd += value;

            //Math.Max(0, Math.Min(newPosition, buffer.Length)
        }
    }
}
