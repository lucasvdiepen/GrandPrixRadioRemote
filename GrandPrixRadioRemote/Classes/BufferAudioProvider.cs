﻿using NAudio.Wave;
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

        private readonly object lockObject;

        public BufferAudioProvider(WaveStream waveStream, double bufferSeconds)
        {
            this.waveStream = waveStream;

            int bufferSize = (int)(WaveFormat.AverageBytesPerSecond * bufferSeconds);

            buffer = new byte[bufferSize];
            lockObject = new object();

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
            // todo: Keep the position clamped between 0 and the length of the buffer
            position += positionToAdd;
            positionToAdd = 0;

            long bytesRead = Math.Min(count, this.buffer.Length - position);
            Array.Copy(this.buffer, position, buffer, offset, bytesRead);
            position += bytesRead;
            if(position >= this.buffer.Length)
            {
                position = 0;
            }

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
            if (writePosition >= this.buffer.Length)
            {
                writePosition = 0;
            }
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
