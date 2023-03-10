using NAudio.Wave;
using SoundFingerprinting.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AudioRecorder
    {
        public Action<WaveInEventArgs> dataAvailable;

        private WaveInEvent waveInEvent;
        private const int SAMPLE_RATE = 5512;

        public AudioRecorder()
        {
            _ = Task.Factory.StartNew(RecordMicNAudio);
        }

        public void RecordMicNAudio()
        {
            Console.WriteLine($"Available devices {WaveIn.DeviceCount}. Will use device 0 for recording.");
            for (int device = 0; device < WaveIn.DeviceCount; ++device)
            {
                var capabilities = WaveIn.GetCapabilities(device);
                Console.WriteLine($"Device {device} Name {capabilities.ProductName}, Channels {capabilities.Channels}");
            }

            waveInEvent = new WaveInEvent();
            waveInEvent.DeviceNumber = 0;
            waveInEvent.WaveFormat = new NAudio.Wave.WaveFormat(rate: SAMPLE_RATE, bits: 16, channels: 1);
            waveInEvent.DataAvailable += (o, e) => dataAvailable.Invoke(e);
            waveInEvent.RecordingStopped += (o, e) => Console.WriteLine("Recording stopped.");
            waveInEvent.BufferMilliseconds = 1000;
            waveInEvent.StartRecording();
        }
    }
}
