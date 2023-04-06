using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class BufferAudioProvider : IWaveProvider, IDisposable
    {
        public WaveFormat WaveFormat => waveStream.WaveFormat;

        public long Position => position + positionToAdd;

        public long WritePosition => writePosition;

        public int Length => buffer.Length;

        public bool IsPlaying { get; private set; } = true;

        private WaveStream waveStream;
        private byte[] buffer;
        private long position;
        private long writePosition;
        private long positionToAdd;
        private CancellationTokenSource readerCancellationTokenSource;

        private readonly object lockObject;

        public BufferAudioProvider(WaveStream waveStream, double bufferSeconds)
        {
            this.waveStream = waveStream;

            int bufferSize = (int)(WaveFormat.AverageBytesPerSecond * bufferSeconds);

            buffer = new byte[bufferSize];
            lockObject = new object();

            readerCancellationTokenSource = new CancellationTokenSource();
            //Start reader thread
            Task.Factory.StartNew(ReadSource);
        }

        private Task ReadSource()
        {
            while(!readerCancellationTokenSource.Token.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024];
                int l = waveStream.Read(buffer, 0, buffer.Length);

                AddSamples(buffer, 0, l);
            }

            return null;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            long newPosition = position + positionToAdd;
            position = ClampPosition(newPosition);
            positionToAdd = 0;

            if (!IsPlaying)
            {
                for(int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }

                return buffer.Length;
            }


            long bytesRead = Math.Min(count, this.buffer.Length - position);
            Array.Copy(this.buffer, position, buffer, offset, bytesRead);
            position += bytesRead;
            if(position >= this.buffer.Length)
            {
                position = 0;
            }

            return (int)bytesRead;
        }

        public byte[] GetSamples(long position, long samples)
        {
            position = ClampPosition(position);

            byte[] bytes = new byte[samples];

            long endPosition = position + samples;
            if(endPosition >= buffer.Length)
            {
                Array.Copy(buffer, position, bytes, 0, buffer.Length - position);
                Array.Copy(buffer, 0, bytes, buffer.Length - position, endPosition - buffer.Length);
                return bytes;
            }

            Array.Copy(buffer, position, bytes, 0, samples);

            return bytes;
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
            IsPlaying = true;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void ChangePosition(TimeSpan time)
        {
            long value = (long)(WaveFormat.AverageBytesPerSecond * time.TotalSeconds);

            positionToAdd += value;
        }

        private long ClampPosition(long position)
        {
            if(position < 0)
            {
                return position + buffer.Length;
            }

            return position % buffer.Length;
        }

        public void Dispose()
        {
            readerCancellationTokenSource.Cancel();
            waveStream.Dispose();
        }
    }
}
