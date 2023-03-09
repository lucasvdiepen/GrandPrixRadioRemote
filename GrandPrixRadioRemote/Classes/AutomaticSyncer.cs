using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Audio.NAudio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AutomaticSyncer
    {
        private int sampleId;

        private IModelService modelService = new InMemoryModelService();
        private IAudioService audioService = new NAudioService();

        public async Task CreateFingerprintsAudioSamples(AudioSamples audioSamples)
        {
            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(audioSamples)
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo(sampleId.ToString(), "GrandPrixRadioAudioSample" + sampleId, ""), avHashes);

            sampleId++;

            Console.WriteLine($"Generate hashes {avHashes}");
        }
    }
}
