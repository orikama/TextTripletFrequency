using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace TextTripletFrequency
{
    class SimpleWorker
    {
        private const int AlphabetSize = 27;
        private const int DictionarySize = AlphabetSize * AlphabetSize * AlphabetSize;

        public static void CountTriplets(object args)
        {
            Tuple<string, CancellationToken> tuple = args as Tuple<string, CancellationToken>;
            string path = tuple.Item1;
            CancellationToken ct = tuple.Item2;

            Dictionary<string, int> tripletFrequency = new Dictionary<string, int>(DictionarySize);

            //Regex regexWord = new Regex(@"\b([A-Z]{3,})\b", RegexOptions.Compiled);
            //Regex split = new Regex(@"[^A-Z]", RegexOptions.Compiled);
            char[] separator = " \t\r\n\x85\xA0.,;:!?#()[]{}-\"1234567890".ToCharArray();

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null && ct.IsCancellationRequested == false)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    //var matches = regexWord.Matches(line.ToUpper());
                    //var matches = split.Split(line.ToUpper());
                    var matches = line.ToUpper().Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string match in matches)
                    {
                        //string word = match.Groups[1].Value;

                        for (int start = 0, end = 3; end <= match.Length; ++start, ++end)
                        {
                            string triplet = match[start..end];

                            if (tripletFrequency.ContainsKey(triplet) == false)
                                tripletFrequency.Add(triplet, 1);
                            else
                                ++tripletFrequency[triplet];
                        }
                    }
                }
            }

            var top10 = tripletFrequency.OrderByDescending(kvp => kvp.Value).Take(10).Select(kvp => kvp.Key);

            Console.WriteLine(string.Join(',', top10));
        }
    }
}
