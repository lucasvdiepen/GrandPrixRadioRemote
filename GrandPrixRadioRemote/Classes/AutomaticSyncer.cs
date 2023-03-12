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

        public AutomaticSyncer()
        {
            audioRecorder.onDataAvailable += soundFingerprintingSystem.DataAvailable;
        }

        public void Sync(/*AudioStream audioStream*/)
        {
            //audioStream.WriteSample(10);

            var task = soundFingerprintingSystem.CreateFingerprintFromFile("f1test2.wav");
            task.Wait();

            audioRecorder.StartRecording();

            _ = soundFingerprintingSystem.GetBestMatchForStream();
        }
    }
}
