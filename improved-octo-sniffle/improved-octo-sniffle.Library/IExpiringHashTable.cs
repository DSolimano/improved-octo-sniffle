using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace improved_octo_sniffle.Library
{
    public interface IExpiringHashTable<TKey, TValue>
    {
        void Put(TKey key_, TValue val_);

        void PutWithExpiration(TKey key_, TValue val_, DateTime expiration_);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key_"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if there's no such key present</exception>
        TValue Get(TKey key_);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key_"></param>
        /// <exception cref="KeyNotFoundException">Thrown if there's no such key present</exception>
        void Delete(TKey key_);

    }
}
