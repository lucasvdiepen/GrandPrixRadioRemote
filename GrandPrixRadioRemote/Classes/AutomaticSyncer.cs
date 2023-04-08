using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GrandPrixRadioRemote.Classes
{
    public class AutomaticSyncer
    {
        private const int INITIAL_SAMPLE_LENGTH = 5;

        private SoundFingerprintingSystem soundFingerprintingSystem = new SoundFingerprintingSystem();
        private AudioRecorder audioRecorder = new AudioRecorder();
        private Timer timer = new Timer();
        private AudioStream audioStream;

        private bool isSyncing;
        private DateTime initialSampleTime;

        public AutomaticSyncer(AudioStream audioStream)
        {
            audioRecorder.onDataAvailable += soundFingerprintingSystem.DataAvailable;
            soundFingerprintingSystem.onMatch += OnMatch;

            timer.Interval = 5000;
            timer.AutoReset = true;
            timer.Elapsed += (o, e) => { ProvideAudioData(); };

            this.audioStream = audioStream;
        }

        public void Sync()
        {
            if (isSyncing) return;

            isSyncing = true;

            initialSampleTime = DateTime.Now - TimeSpan.FromSeconds(INITIAL_SAMPLE_LENGTH);

            var audioSamples = audioStream.GetSamples(INITIAL_SAMPLE_LENGTH);

            var task = soundFingerprintingSystem.CreateFingerprintFromAudioSamples(audioSamples);
            task.Wait();

            audioStream.Mute();

            audioRecorder.StartRecording();

            soundFingerprintingSystem.GetBestMatchForStream();

            timer.Start();
        }

        public void ProvideAudioData()
        {
            Console.WriteLine("Providing audio sample");

            if (!isSyncing) return;

            var audioSamples = audioStream.GetSamples();

            var task = soundFingerprintingSystem.CreateFingerprintFromAudioSamples(audioSamples);
            task.Wait();
        }

        private void OnMatch(double delay, DateTime matchedAt)
        {
            double totalDelay = (matchedAt - initialSampleTime.ToUniversalTime() - TimeSpan.FromSeconds(delay)).TotalSeconds;

            Console.WriteLine("Delay is " + totalDelay);

            audioStream.ChangePosition(totalDelay * -1);

            Stop();
        }

        public void Stop()
        {
            timer.Stop();
            audioRecorder.StopRecording();

            audioStream.Unmute();

            isSyncing = false;
        }
    }
}
