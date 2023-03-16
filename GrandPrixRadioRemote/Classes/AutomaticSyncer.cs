using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GrandPrixRadioRemote.Classes
{
    public class AutomaticSyncer
    {
        private SoundFingerprintingSystem soundFingerprintingSystem = new SoundFingerprintingSystem();
        private AudioRecorder audioRecorder = new AudioRecorder();
        private Timer timer = new Timer();
        private AudioStream audioStream;

        private bool isSyncing;

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

            ProvideAudioData();

            audioStream.Mute();

            audioRecorder.StartRecording();

            soundFingerprintingSystem.GetBestMatchForStream();

            timer.Start();
        }

        public void ProvideAudioData()
        {
            Console.WriteLine("Providing audio sample");

            if (!isSyncing) return;

            audioStream.WriteSample(5);

            var task = soundFingerprintingSystem.CreateFingerprintFromFile("test.wav");
            task.Wait();
        }

        private void OnMatch(double delay)
        {
            timer.Stop();

            audioRecorder.StopRecording();

            Console.WriteLine("Delay is " + delay);

            //audioStream.ChangePosition((long)delay);

            audioStream.Unmute();

            isSyncing = false;
        }
    }
}
