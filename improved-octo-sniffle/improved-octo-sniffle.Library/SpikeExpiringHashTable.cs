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
                    //we remove it, and let the key not found exception be thrown later;
                    ValueExpiryManager dummy;
                    _base.TryRemove(key_, out dummy);

                }
            }
            
            throw new KeyNotFoundException(string.Format("Not found: " + Convert.ToString(key_)));
        }

        public void Put(TKey key_, TValue val_)
        {
            _base.TryAdd(key_, new ValueExpiryManager(val_, DateTime.MinValue));
        }

        public void PutWithExpiration(TKey key_, TValue val_, DateTime expiration_)
        {
            _base.TryAdd(key_, new ValueExpiryManager(val_, expiration_));
        }

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
