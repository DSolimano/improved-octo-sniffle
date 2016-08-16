using improved_octo_sniffle.Library;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace improved_octo_sniffle.Tests.Timing
{
    [TestFixture]
    public class GetAndPutRandomData
    {
        TimingBuckets tb = new TimingBuckets();
        SpikeExpiringHashTable<string, int> foobar = new SpikeExpiringHashTable<string, int>();

        [SetUp]
        public void SetUp()
        {
            tb = new TimingBuckets();
            foobar = new SpikeExpiringHashTable<string, int>();
        }

        [Test]
        public void OneThreadReadTimes()
        {
            List<string> keys = new List<string>();
            
            Random r = new Random();
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 5000000; i++)
            {
                string key = Guid.NewGuid().ToString();
                int value = r.Next();
                keys.Add(key);
                if (i % 3 == 0)
                {
                    DateTime expiry = DateTime.UtcNow.AddMinutes(3);
                    foobar.PutWithExpiration(key, value, expiry);
                }
                else
                {
                    foobar.Put(key, value);
                }
            }
            keys.Shuffle();

            foreach(string key in keys)
            {
                int val;
                sw.Start();
                val = foobar.Get(key);
                sw.Stop();
                tb.AddResult(sw.Elapsed);
                sw.Reset();
            }

            AssertTimesAreGood();

        }

        [Test]
        public void MultiThreadsWrite()
        {
            
            System.Threading.Barrier startBarrier = new System.Threading.Barrier(4);
            System.Threading.Barrier endBarrier = new System.Threading.Barrier(5);

            for (int threadcount = 0; threadcount < 4; threadcount++)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(
                    delegate (object x_)
                    {
                        startBarrier.SignalAndWait();
                        Random r = new Random();
                        Stopwatch sw = new Stopwatch();
                        for (int datumCount = 0; datumCount < 1000000; datumCount++)
                        {
                            string key = Guid.NewGuid().ToString();
                            int value = r.Next();

                            if (datumCount % 3 == 0)
                            {
                                DateTime expiry = DateTime.UtcNow.AddMinutes(3);
                                sw.Start();
                                foobar.PutWithExpiration(key, value, expiry);
                                sw.Stop();
                            }
                            else
                            {
                                sw.Start();
                                foobar.Put(key, value);
                                sw.Stop();
                            }

                            tb.AddResult(sw.Elapsed);

                            sw.Reset();
                        }

                        endBarrier.SignalAndWait();
                    });
            }

            endBarrier.SignalAndWait();

            AssertTimesAreGood();

        }

        [Test]
        public void MultiThreadsRead()
        {
            List<string> keys = new List<string>();

            Random rinit = new Random();
            for (int i = 0; i < 5000000; i++)
            {
                string key = Guid.NewGuid().ToString();
                int value = rinit.Next();
                keys.Add(key);
                if (i % 3 == 0)
                {
                    DateTime expiry = DateTime.UtcNow.AddMinutes(3);
                    foobar.PutWithExpiration(key, value, expiry);
                }
                else
                {
                    foobar.Put(key, value);
                }
            }
            

            System.Threading.Barrier startBarrier = new System.Threading.Barrier(4);
            System.Threading.Barrier endBarrier = new System.Threading.Barrier(5);

            for (int threadcount = 0; threadcount < 4; threadcount++)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(
                    delegate (object x_)
                    {
                        List<string> keysToRead = new List<string>(keys);
                        keysToRead.Shuffle();
                        startBarrier.SignalAndWait();
                        Random r = new Random();
                        Stopwatch sw = new Stopwatch();
                        foreach (string key in keys)
                        {
                            DateTime expiry = DateTime.UtcNow.AddMinutes(3);
                            sw.Start();
                            int val = foobar.Get(key);
                            sw.Stop();
                            
                            tb.AddResult(sw.Elapsed);
                            
                            sw.Reset();

                        }

                        endBarrier.SignalAndWait();
                    });
            }

            endBarrier.SignalAndWait();

            AssertTimesAreGood();

        }

        private void AssertTimesAreGood()
        {
            Assert.LessOrEqual(1 - (decimal)tb.LessThan1MS / (decimal)tb.Total, .05m);
            Assert.LessOrEqual(1 - (decimal)tb.LessThan5MS / (decimal)tb.Total, .01m);
        }

        

        
    }

    internal static class ListHelpers
    {
        //List shuffling from http://stackoverflow.com/questions/273313/randomize-a-listt
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    internal class TimingBuckets
    {
        private readonly TimeSpan _1msCutoff = new TimeSpan(0, 0, 0, 0, 1);
        private int _lessThan1MS;
        public long LessThan1MS { get { return _lessThan1MS; } }

        private readonly TimeSpan _5msCutoff = new TimeSpan(0, 0, 0, 0, 5);
        private int _lessThan5MS;
        public long LessThan5MS { get { return _lessThan5MS; } }

        public long Total { get; private set; }

        public void AddResult(TimeSpan ts_)
        {
            if(ts_ < _1msCutoff)
            {
                System.Threading.Interlocked.Increment(ref _lessThan1MS);
            }

            if(ts_ < _5msCutoff)
            {
                System.Threading.Interlocked.Increment(ref _lessThan5MS);
            }

            Total++;
        }
    }
}
