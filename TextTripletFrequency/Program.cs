using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TextTripletFrequency
{
    class Program
    {
        static void Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            Tuple<string, CancellationToken> pathAndToken = Tuple.Create(args[0], cts.Token);
            Task fileProccessing = Task.Factory.StartNew(
                () => SimpleWorker.CountTriplets(pathAndToken),
                //() => MultithreadedWorker.CountTriplets(pathAndToken),
                //() => MultithreadedWorker_MultiDictionaries.CountTriplets(pathAndToken),
                cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            Task waitConsoleKey = Task.Factory.StartNew(() => Console.ReadKey(true));

            Task.WaitAny(fileProccessing, waitConsoleKey);
            if (waitConsoleKey.IsCompleted && !fileProccessing.IsCompleted)
            {
                cts.Cancel();
                fileProccessing.Wait();
            }

            stopwatch.Stop();
            Console.WriteLine($"Time:{stopwatch.ElapsedMilliseconds}");
        }
    }
}
