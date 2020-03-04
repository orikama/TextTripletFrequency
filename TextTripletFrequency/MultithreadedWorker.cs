using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TextTripletFrequency
{
    class MultithreadedWorker
    {
        private const int NumberOfThreads = 2;
        private const int AlphabetSize = 27;
        private const int DictionarySize = AlphabetSize * AlphabetSize * AlphabetSize;

        private static readonly ConcurrentDictionary<string, int> _tripletFrequency = new ConcurrentDictionary<string, int>(NumberOfThreads, DictionarySize);
        private static readonly BlockingCollection<string> _consumerProducer = new BlockingCollection<string>();

        public static void CountTriplets(object args)
        {
            Tuple<string, CancellationToken> tuple = args as Tuple<string, CancellationToken>;
            string path = tuple.Item1;
            CancellationToken ct = tuple.Item2;

            Task[] tasks = new Task[NumberOfThreads];
            for (int i = 0; i < NumberOfThreads; ++i)
            {
                int dictionaryIndex = i;
                tasks[i] = Task.Factory.StartNew(() => RegexWorker(ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
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

            foreach (var kvp in _tripletFrequency.ToArray().OrderByDescending(kvp => kvp.Value).Take(10))
            {
                Console.WriteLine(kvp.Key + "\t" + kvp.Value);
            }
            Console.WriteLine();

            var top10 = _tripletFrequency.ToArray().OrderByDescending(kvp => kvp.Value).Take(10).Select(kvp => kvp.Key);

            Console.WriteLine(string.Join(',', top10));
        }

        private static void RegexWorker(CancellationToken ct)
        {
            char[] separator = " \t\r\n\x85\xA0.,;:!?#()[]{}-\"1234567890".ToCharArray();

            while (_consumerProducer.IsCompleted == false && ct.IsCancellationRequested == false)
            {
                if (_consumerProducer.TryTake(out string line) == false)
                    continue;

                var matches = line.ToUpper().Split(separator, StringSplitOptions.RemoveEmptyEntries);

                foreach (var match in matches)
                {
                    string word = match;
                    if (word.Length < 3)
                        continue;

                    for (int start = 0, end = 3; end <= word.Length; ++start, ++end)
                    {
                        string triplet = word[start..end];

                        _tripletFrequency.AddOrUpdate(triplet, 1, (key, oldValue) => oldValue + 1);
                    }
                }
            }
        }
    }
}
