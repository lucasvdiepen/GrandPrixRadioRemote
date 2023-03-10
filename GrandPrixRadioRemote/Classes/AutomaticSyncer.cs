using NAudio.Wave;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Audio.NAudio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Command;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class AutomaticSyncer
    {
        private int sampleId;

        private IModelService modelService = new InMemoryModelService();
        private IAudioService audioService = new SoundFingerprintingAudioService();

        private BlockingCollection<AudioSamples> realtimeSource = new BlockingCollection<AudioSamples>();

        public async Task CreateFingerprintFromAudioSamples(AudioSamples audioSamples)
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

        public async Task CreateFingerprintFromFile(string path)
        {
            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(path)
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo(sampleId.ToString(), "GrandPrixRadioAudioSample" + sampleId, ""), avHashes);

            sampleId++;

            Console.WriteLine($"Generate hashes {avHashes}");
        }

        public async Task<double> GetBestMatchForStream(BlockingCollection<AudioSamples> audioSamples, IModelService modelService, CancellationToken token)
        {
            double seconds = await QueryCommandBuilder.Instance
                .BuildRealtimeQueryCommand()
                .From(new BlockingRealtimeCollection<AudioSamples>(audioSamples))
                .WithRealtimeQueryConfig(config =>
                {
                // match only those entries got at least 5 seconds of query match
                config.ResultEntryFilter = new TrackMatchLengthEntryFilter(2d);

                // provide a success callback that will be invoked for matches that pass the result entry filter
                config.SuccessCallback = result =>
                    {
                        foreach (var entry in result.ResultEntries)
                        {
                            Console.WriteLine($"Successfully matched {entry.TrackId}");
                        }
                    };

                    config.DidNotPassFilterCallback = (queryResult) =>
                    {
                        foreach (var result in queryResult.ResultEntries)
                        {
                            Console.WriteLine($"Did not pass filter {result.TrackId}");
                        }
                    };

                    return config;
                })
                .UsingServices(modelService)
                .Query(token);

            Console.WriteLine($"Realtime query stopped. Issued {seconds} seconds of query.");
            return seconds;
        }

        public void DataReceived(WaveInEventArgs e)
        {
            // using short because 16 bits per sample is used as input wave format
            short[] samples = new short[e.BytesRecorded / 2];
            Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);
            // converting to [-1, +1] range
            float[] floats = Array.ConvertAll(samples, (sample => (float)sample / short.MaxValue));
            realtimeSource.Add(new AudioSamples(floats, string.Empty, 5512));
        }
    }
}
