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
    public class PutLootsOfData
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
        public void OneThreadNoExpiration()
        {
            Random r = new Random();
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 5000000; i++)
            {
                string key = Guid.NewGuid().ToString();
                int value = r.Next();

                if (i % 3 == 0)
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

            AssertTimesAreGood();

        }

        [Test]
        public void MultiThreads()
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

        private void AssertTimesAreGood()
        {
            Assert.LessOrEqual(1 - (decimal)tb.LessThan1MS / (decimal)tb.Total, .05m);
            Assert.LessOrEqual(1 - (decimal)tb.LessThan5MS / (decimal)tb.Total, .01m);
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
