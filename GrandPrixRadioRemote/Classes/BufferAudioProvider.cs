using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
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
        public Action OnStoppedUnexpectedly;

        private WaveStream waveStream;
        private WaveFormat waveFormat;
        private byte[] buffer;
        private long position;
        private long writePosition;
        private long positionToAdd;
        private int targetBufferLength;
        private int targetBeforeBufferLength;
        private CancellationTokenSource readerCancellationTokenSource;
        private bool isReadOverWritePosition;

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
            if (GetDistanceForward() < targetBufferLength) return;

            Console.WriteLine("Buffer is full enough. Playing...");

            Play();

            OnDataAvailable -= WaitForBufferFill;

            isReadOverWritePosition = false;
        }

        private Task ReadSource()
        {
            while(!readerCancellationTokenSource.Token.IsCancellationRequested)
            {
                // todo: find a better way of doing this
                long deltaPosition = GetDistanceBackward();
                if (deltaPosition <= targetBeforeBufferLength && deltaPosition >= 1024 && !isReadOverWritePosition)
                {
                    Console.WriteLine("Reader waiting...");
                    Thread.Sleep(100);
                    continue;
                }

                byte[] buffer = new byte[1024];

                int l = 0;

                try
                {
                    l = waveStream.Read(buffer, 0, buffer.Length);
                }
                catch(COMException)
                {
                    Console.WriteLine("Audio stream has crashed. Reloading automatically...");
                    OnStoppedUnexpectedly.Invoke();

                    readerCancellationTokenSource.Cancel();

                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Access denied. Reloading automatically...");
                    OnStoppedUnexpectedly.Invoke();

                    readerCancellationTokenSource.Cancel();

                    break;
                }

                if(l == 0)
                {
                    readerCancellationTokenSource.Cancel();
                    break;
                }

                AddSamples(buffer, 0, l);

                OnDataAvailable?.Invoke(l, buffer);
            }

            waveStream.Dispose();

            //Console.WriteLine("Reader task cancelled");

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

            long bytesToEnd = Math.Min(count, this.buffer.Length - position);
            Array.Copy(this.buffer, position, buffer, offset, bytesToEnd);
            Array.Copy(this.buffer, 0, buffer, offset + bytesToEnd, count - bytesToEnd);
            AddPosition(count);

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
            long bytesToEnd = Math.Min(count, this.buffer.Length - writePosition);
            Array.Copy(buffer, offset, this.buffer, writePosition, bytesToEnd);
            Array.Copy(buffer, 0, this.buffer, offset + bytesToEnd, count - bytesToEnd);
            writePosition += count;

            Console.WriteLine("Added samples. Write position at: " + writePosition);

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
            // Check if read went over write position here
            if(amount > 0)
            {
                if(amount > GetDistanceForward())
                {
                    //Went over
                    PauseAndWait();
                }
            }
            else if(amount < 0)
            {
                if(amount * -1 > GetDistanceBackward())
                {
                    return;
                }
            }

            long newPosition = position + amount;
            if(newPosition < 0)
            {
                newPosition += buffer.Length;
            }
            else if(newPosition >= buffer.Length)
            {
                newPosition -= buffer.Length;
            }

            position = newPosition;

            Console.WriteLine("Read position at: " + position);
        }

        private void PauseAndWait()
        {
            Console.WriteLine("Read position went over write position");

            Pause();

            isReadOverWritePosition = true;

            OnDataAvailable += WaitForBufferFill;
        }

        private long GetDistanceForward()
        {
            long currentWritePosition = writePosition;

            if(writePosition < position && !isReadOverWritePosition) currentWritePosition += buffer.Length;

            return currentWritePosition - position;
        }

        private long GetDistanceBackward()
        {
            long currentPosition = position;

            if(position < writePosition) currentPosition += buffer.Length;

            return currentPosition - writePosition;
        }

        private long SecondsToBytes(double seconds) => (long)(WaveFormat.AverageBytesPerSecond * seconds);

        public void Dispose()
        {
            OnDataAvailable -= WaitForBufferFill;

            readerCancellationTokenSource.Cancel();
        }
    }
}
