using NAudio.Wave;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Audio.NAudio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Command;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Strides;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.Classes
{
    public class SoundFingerprintingSystem
    {
        public Action<double, DateTime> onMatch;

        private int sampleId;

        private IModelService modelService = new InMemoryModelService();
        private IAudioService audioService = new NAudioService();

        private BlockingCollection<AudioSamples> realtimeSource = new BlockingCollection<AudioSamples>();

        private List<double> trackDurations = new List<double>();
        private Task<Task<double>> task;
        private CancellationTokenSource tokenSource;

        public async Task CreateFingerprintFromAudioSamples(AudioSamples audioSamples)
        {
            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(audioSamples)
                .WithFingerprintConfig(config => {
                    config.Audio.Stride = new IncrementalRandomStride(64, 128);

                    return config;
                })
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo(sampleId.ToString(), "GrandPrixRadioAudioSample" + sampleId, ""), avHashes);

            trackDurations.Add(avHashes.Audio.DurationInSeconds);

            sampleId++;

            Console.WriteLine($"Generate hashes {avHashes}");
        }

        public void GetBestMatchForStream()
        {
            if(tokenSource != null) tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            task = Task.Factory.StartNew(() => GetBestMatchForStream(realtimeSource, modelService, tokenSource.Token));
        }

        public void Stop()
        {
            tokenSource.Cancel();

            task.Dispose();
            realtimeSource = new BlockingCollection<AudioSamples>();
            trackDurations.Clear();

            for(int i = 0; i < sampleId; i++)
            {
                modelService.DeleteTrack(i.ToString());
            }

            sampleId = 0;
        }

        public async Task<double> GetBestMatchForStream(BlockingCollection<AudioSamples> audioSamples, IModelService modelService, CancellationToken token)
        {
            double seconds = await QueryCommandBuilder.Instance
                .BuildRealtimeQueryCommand()
                .From(new BlockingRealtimeCollection<AudioSamples>(audioSamples))
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryConfiguration.Audio.Stride = new IncrementalRandomStride(64, 128);
                    config.ResultEntryFilter = new TrackMatchLengthEntryFilter(0.05d);
                    config.SuccessCallback = result =>
                    {
                        foreach (var entry in result.ResultEntries)
                        {
                            Console.WriteLine($"Successfully matched {entry.TrackId}");
                            Console.WriteLine($"Match starts at {entry.Audio.TrackMatchStartsAt}");
                            Console.WriteLine($"Query match starts at {entry.Audio.QueryMatchStartsAt}");
                            Console.WriteLine($"Matched at {entry.Audio.MatchedAt}");
                            Console.WriteLine($"Confidence {entry.Audio.Confidence}");

                            double totalMatchTime = entry.Audio.TrackMatchStartsAt + GetLength(int.Parse(entry.TrackId));

                            onMatch?.Invoke(totalMatchTime, entry.Audio.MatchedAt);

                            Stop();
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

        public void DataAvailable(WaveInEventArgs e)
        {
            // using short because 16 bits per sample is used as input wave format
            short[] samples = new short[e.BytesRecorded / 2];
            Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);
            // converting to [-1, +1] range
            float[] floats = Array.ConvertAll(samples, (sample => (float)sample / short.MaxValue));
            realtimeSource.Add(new AudioSamples(floats, string.Empty, 5512));
        }

        private double GetLength(int id)
        {
            double length = 0;

            for(int i = 0; i < id; i++)
            {
                length += trackDurations[i];
            }

            return length;
        }
    }
}
