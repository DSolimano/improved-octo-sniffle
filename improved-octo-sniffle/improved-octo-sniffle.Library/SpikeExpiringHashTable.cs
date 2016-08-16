using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace improved_octo_sniffle.Library
{
    public class SpikeExpiringHashTable<TKey, TValue> : IExpiringHashTable<TKey, TValue>
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<TKey, ValueExpiryManager>
            _base = new System.Collections.Concurrent.ConcurrentDictionary<TKey, ValueExpiryManager>(20, 10000000);

        public void Delete(TKey key_)
        {
            ValueExpiryManager val;
            _base.TryRemove(key_, out val);
        }

        public TValue Get(TKey key_)
        {
            ValueExpiryManager val;
            if(_base.TryGetValue(key_, out val))
            {
                if (!val.IsExpired())
                {
                    return val.Val;
                }
                else
                {
                    //we do nothing here if the key is expired.  The reason that we don't do a remove is
                    // another thread may be replacing our value with one that is not expired.  If we remove
                    // by key, we may accidentally remove the new value in that case.  This way is wasteful of space
                    // but correct.

                }
            }
            
            throw new KeyNotFoundException(string.Format("Not found: " + Convert.ToString(key_)));
        }

        public void Put(TKey key_, TValue val_)
        {
            PutWithExpiration(key_, val_, DateTime.MinValue);
        }

        public void PutWithExpiration(TKey key_, TValue val_, DateTime expiration_)
        {
            _base[key_] = new ValueExpiryManager(val_, expiration_);
        }

        /// <summary>
        /// This class exists to wrap the values for our hash table and let us know when, if ever, they are expired.
        /// </summary>
        private struct ValueExpiryManager
        {
            public TValue Val { get; }
            public DateTime Expiry { get; }

            public ValueExpiryManager(TValue val_, DateTime expiry_)
            {
                Val = val_;
                Expiry = expiry_;
            }

            public bool IsExpired()
            {
                return Expiry == DateTime.MinValue ? false : DateTime.UtcNow > Expiry;
            }
        }
    }
}
