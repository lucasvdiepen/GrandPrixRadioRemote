using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AutomaticSyncer
    {
        private SoundFingerprintingSystem soundFingerprintingSystem = new SoundFingerprintingSystem();
        private AudioRecorder audioRecorder = new AudioRecorder();
        private AudioStream audioStream;

        public AutomaticSyncer(/*AudioStream audioStream*/)
        {
            audioRecorder.onDataAvailable += soundFingerprintingSystem.DataAvailable;
            soundFingerprintingSystem.onMatch += OnMatch;

            //this.audioStream = audioStream;
        }

        public void Sync()
        {
            //audioStream.WriteSample(10);

            var task = soundFingerprintingSystem.CreateFingerprintFromFile("f1test2.wav");
            task.Wait();

            //audioStream.Mute();

            audioRecorder.StartRecording();

            soundFingerprintingSystem.GetBestMatchForStream();
        }

        private void OnMatch(double delay)
        {
            audioRecorder.StopRecording();

            Console.WriteLine("Delay is " + delay);

            //audioStream.ChangePosition((long)delay);

            //audioStream.Unmute();
        }
    }
}
