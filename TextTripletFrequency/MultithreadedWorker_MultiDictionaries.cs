using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TextTripletFrequency
{
    class MultithreadedWorker_MultiDictionaries
    {
        private const int NumberOfThreads = 2;
        private const int AlphabetSize = 27;
        private const int DictionarySize = AlphabetSize * AlphabetSize * AlphabetSize;

        private static Dictionary<string, int>[] _tripletFrequency;
        private static readonly BlockingCollection<string> _consumerProducer = new BlockingCollection<string>();

        public static void CountTriplets(object args)
        {
            Tuple<string, CancellationToken> tuple = args as Tuple<string, CancellationToken>;
            string path = tuple.Item1;
            CancellationToken ct = tuple.Item2;

            _tripletFrequency = new Dictionary<string, int>[NumberOfThreads];
            for (int i = 0; i < NumberOfThreads; ++i)
                _tripletFrequency[i] = new Dictionary<string, int>(DictionarySize);

            Task[] tasks = new Task[NumberOfThreads];
            for (int i = 0; i < NumberOfThreads; ++i)
            {
                int dictionaryIndex = i;
                tasks[i] = Task.Factory.StartNew(() => RegexWorker(dictionaryIndex, ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null && ct.IsCancellationRequested == false)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    _consumerProducer.Add(line);
                }
                _consumerProducer.CompleteAdding();
            }

            Task.WaitAll(tasks);

            Dictionary<string, int> result = _tripletFrequency[0];
            for (int i = 1; i < _tripletFrequency.Length; ++i)
            {
                foreach (var kvp in _tripletFrequency[i])
                {
                    if (result.ContainsKey(kvp.Key) == false)
                        result.Add(kvp.Key, kvp.Value);
                    else
                        result[kvp.Key] += kvp.Value;
                }
            }

            foreach (var kvp in result.ToArray().OrderByDescending(kvp => kvp.Value).Take(10))
            {
                Console.WriteLine(kvp.Key + "\t" + kvp.Value);
            }
            Console.WriteLine();

            var top10 = result.ToArray().OrderByDescending(kvp => kvp.Value).Take(10).Select(kvp => kvp.Key);

            Console.WriteLine(string.Join(',', top10));
        }

        private static void RegexWorker(int dictionaryIndex, CancellationToken ct)
        {
            Dictionary<string, int> tripletFrequency = _tripletFrequency[dictionaryIndex];

            char[] separator = " \t\r\n\x85\xA0.,;:!?#()[]{}-\"1234567890".ToCharArray();

            while (_consumerProducer.IsCompleted == false && ct.IsCancellationRequested == false)
            {
                if (_consumerProducer.TryTake(out string line) == false)
                    continue;

                string[] words = line.ToUpper().Split(separator, StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in words)
                {
                    if (word.Length < 3)
                        continue;

                    for (int start = 0, end = 3; end <= word.Length; ++start, ++end)
                    {
                        string triplet = word[start..end];

                        if (tripletFrequency.ContainsKey(triplet) == false)
                            tripletFrequency.Add(triplet, 1);
                        else
                            ++tripletFrequency[triplet];
                    }
                }
            }
        }
    }
}
