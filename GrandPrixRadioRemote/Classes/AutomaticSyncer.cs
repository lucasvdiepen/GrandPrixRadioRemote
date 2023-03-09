using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
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
        private InMemoryModelService modelService = new InMemoryModelService();

        public async Task CreateFingerprintsAudioSamples(AudioSamples audioSamples)
        {
            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(audioSamples)
                .Hash();

            Console.WriteLine($"Generate hashes {avHashes}");
        }
    }
}
