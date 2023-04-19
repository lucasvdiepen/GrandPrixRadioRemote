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
        private int targetBeforeBufferLength;
        private CancellationTokenSource readerCancellationTokenSource;
        private bool readReachedEnd = false;
        private bool writeReachedEnd = false;

        private readonly object lockObject;

        public BufferAudioProvider(WaveStream waveStream, double bufferLengthInSeconds, double bufferSeconds, double beforeBufferSeconds)
        {
            this.waveStream = waveStream;
            waveFormat = waveStream.WaveFormat;

            int bufferSize = (int)SecondsToBytes(bufferLengthInSeconds);
            targetBufferLength = (int)SecondsToBytes(bufferSeconds);
            targetBeforeBufferLength = (int)SecondsToBytes(beforeBufferSeconds);

            buffer = new byte[bufferSize];
            lockObject = new object();

            readerCancellationTokenSource = new CancellationTokenSource();

            OnDataAvailable += WaitForBufferFill;

            //Start reader thread
            Task.Factory.StartNew(ReadSource, readerCancellationTokenSource.Token);
        }

        private void WaitForBufferFill(int length, byte[] buffer)
        {
            if (GetDistance(position, readReachedEnd, writePosition, writeReachedEnd) < targetBufferLength) return;

            Console.WriteLine("Buffer is full enough. Playing...");

            Play();

            OnDataAvailable -= WaitForBufferFill;
        }

        private Task ReadSource()
        {
            while(!readerCancellationTokenSource.Token.IsCancellationRequested)
            {
                // todo: find a better way of doing this
                long deltaPosition = writePosition - position;
                if (deltaPosition >= -targetBeforeBufferLength && deltaPosition <= -1024)
                {
                    continue;
                }

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
            AddPosition(positionToAdd);
            positionToAdd = 0;

            if (!IsPlaying)
            {
                // todo: find a better way of doing this

                for(int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }

                return buffer.Length;
            }

            if (GetDistance(position, readReachedEnd, writePosition, writeReachedEnd) <= 0)
            {
                Console.WriteLine("Read position went over write position");

                Pause();

                //Read position is past the write position
                OnDataAvailable += WaitForBufferFill;

                return count;
            }

            long bytesRead = Math.Min(count, this.buffer.Length - position);
            Array.Copy(this.buffer, position, buffer, offset, bytesRead);
            Array.Copy(this.buffer, 0, buffer, offset + bytesRead, count - bytesRead);
            //position += bytesRead;
            AddPosition(bytesRead);

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
            // todo: this leaves some bytes that are past the buffer length unwritten

            long bytesToAdd = Math.Min(count, this.buffer.Length - writePosition);
            Array.Copy(buffer, offset, this.buffer, writePosition, bytesToAdd);
            writePosition += bytesToAdd;
            if (writePosition >= this.buffer.Length)
            {
                writeReachedEnd = !writeReachedEnd;
                writePosition = 0;
                Console.WriteLine("Write reached end");
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
            if (position < 0)
            {
                return position + buffer.Length;
            }

            return position % buffer.Length;
        }

        private void AddPosition(long amount)
        {
            long newPosition = position + amount;
            if(newPosition < 0)
            {
                readReachedEnd = !readReachedEnd;
                newPosition += buffer.Length;
            }
            else if(newPosition >= buffer.Length)
            {
                readReachedEnd = !readReachedEnd;
                newPosition -= buffer.Length;
                Console.WriteLine("Read reached end");
            }

            position = newPosition;
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
