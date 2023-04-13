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
        public WaveFormat WaveFormat => waveFormat;

        public long Position => position + positionToAdd;

        public long WritePosition => writePosition;

        public int Length => buffer.Length;

        public bool IsPlaying { get; private set; }

        public Action<int, byte[]> OnDataAvailable;

        private WaveStream waveStream;
        private WaveFormat waveFormat;
        private byte[] buffer;
        private long position;
        private long writePosition;
        private long positionToAdd;
        private int targetBufferLength;
        private CancellationTokenSource readerCancellationTokenSource;
        private bool readReachedEnd = false;
        private bool writeReachedEnd = false;

        private readonly object lockObject;

        public BufferAudioProvider(WaveStream waveStream, double bufferLengthInSeconds, double bufferSeconds)
        {
            this.waveStream = waveStream;
            waveFormat = waveStream.WaveFormat;

            int bufferSize = (int)SecondsToBytes(bufferLengthInSeconds);
            targetBufferLength = (int)SecondsToBytes(bufferSeconds);

            buffer = new byte[bufferSize];
            lockObject = new object();

            readerCancellationTokenSource = new CancellationTokenSource();

            OnDataAvailable += CheckStartBufferFilled;

            //Start reader thread
            Task.Factory.StartNew(ReadSource, readerCancellationTokenSource.Token);
        }

        private void CheckStartBufferFilled(int length, byte[] buffer)
        {
            if (GetDistance(position, readReachedEnd, writePosition, writeReachedEnd) < targetBufferLength) return;

            Play();

            OnDataAvailable -= CheckStartBufferFilled;
        }

        private Task ReadSource()
        {
            while(!readerCancellationTokenSource.Token.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024];
                int l = waveStream.Read(buffer, 0, buffer.Length);

                if(l == 0)
                {
                    readerCancellationTokenSource.Cancel();
                    break;
                }

                AddSamples(buffer, 0, l);

                OnDataAvailable?.Invoke(l, buffer);
            }

            waveStream.Dispose();

            Console.WriteLine("Reader task cancelled");

            return null;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            long newPosition = position + positionToAdd;
            position = ClampPosition(newPosition);
            positionToAdd = 0;

            if (position < newPosition) readReachedEnd = !readReachedEnd;

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
            Array.Copy(this.buffer, 0, buffer, offset + bytesRead, count - bytesRead);
            position += bytesRead;
            if (position >= this.buffer.Length)
            {
                readReachedEnd = !readReachedEnd;
                position -= this.buffer.Length;
            }

            return count;
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
                writeReachedEnd = !writeReachedEnd;
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
            long value = SecondsToBytes(time.TotalSeconds);
            if (Math.Abs(value) % 2 == 1) value++;

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

        private long GetDistance(long readPosition, bool readReachedEnd, long writePosition, bool writeReachedEnd)
        {
            if(readReachedEnd ^ writeReachedEnd) writePosition += buffer.Length;

            return writePosition - readPosition;
        }

        private long SecondsToBytes(double seconds) => (long)(WaveFormat.AverageBytesPerSecond * seconds);

        public void Dispose()
        {
            readerCancellationTokenSource.Cancel();
            readerCancellationTokenSource.Dispose();
        }
    }
}
