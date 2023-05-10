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
        public Action<WaveInEventArgs> onDataAvailable;

        private IWaveIn waveInEvent;
        private const int SAMPLE_RATE = 5512;

        private Task recordingTask;

        private void RecordMicNAudio()
        {
            waveInEvent = new WasapiLoopbackCapture();
            waveInEvent.WaveFormat = new NAudio.Wave.WaveFormat(rate: SAMPLE_RATE, bits: 16, channels: 1);
            waveInEvent.DataAvailable += (o, e) => onDataAvailable.Invoke(e);
            waveInEvent.RecordingStopped += (o, e) => Console.WriteLine("Recording stopped.");
            waveInEvent.StartRecording();
        }

        public void StartRecording()
        {
            if (recordingTask != null) return;

            recordingTask = Task.Factory.StartNew(RecordMicNAudio);
        }

        public void StopRecording()
        {
            if (recordingTask == null) return;

            waveInEvent.StopRecording();
            waveInEvent.Dispose();

            recordingTask.Dispose();
            recordingTask = null;
        }
    }
}
