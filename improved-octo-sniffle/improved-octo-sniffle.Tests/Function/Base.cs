using improved_octo_sniffle.Library;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace improved_octo_sniffle.Tests.Function
{
    [TestFixture]
    public class Base
    {
        SpikeExpiringHashTable<string, int> _ht;

        [SetUp]
        public void SetUp()
        {
            _ht = new SpikeExpiringHashTable<string, int>();
        }

        [Test]
        public void Add()
        {
            string key = "Kate";
            int value = 29;

            _ht.Put(key, value);

            //if we get here, without any exceptions, good.
        }

        [Test]
        public void AddAndGet()
        {
            string key = "Kate";
            int value = 29;

            _ht.Put(key, value);

            Assert.AreEqual(value, _ht.Get(key));
        }

        [Test]
        public void AddWithExpirationAndGet()
        {
            string key = "Kate";
            int value = 29;
            DateTime expiration = DateTime.UtcNow.AddHours(1);

            _ht.PutWithExpiration(key, value, expiration);

            Assert.AreEqual(value, _ht.Get(key));
        }

        [Test]
        public void AddWithExpirationInFutureThenExpire()
        {
            string key = "Kate";
            int value = 29;
            DateTime expiration = DateTime.UtcNow.AddMilliseconds(50);

            _ht.PutWithExpiration(key, value, expiration);

            System.Threading.Thread.Sleep(100);

            Assert.Throws(typeof(KeyNotFoundException), () => _ht.Get(key));
        }

        [Test]
        public void AddWithExpirationInPastThenExpire()
        {
            string key = "Kate";
            int value = 29;
            DateTime expiration = DateTime.UtcNow.AddHours(-1);

            _ht.PutWithExpiration(key, value, expiration);

            Assert.Throws(typeof(KeyNotFoundException), () => _ht.Get(key));
        }

        [Test]
        public void Delete()
        {
            string key = "Kate";
            int value = 29;

            _ht.Put(key, value);

            _ht.Delete(key);

            Assert.Throws(typeof(KeyNotFoundException), () => _ht.Get(key));
        }

        [Test]
        
        public void GetNoPut()
        {
            string key = "Kate";

            Assert.Throws(typeof(KeyNotFoundException), () => _ht.Get(key));
        }
    }
}
