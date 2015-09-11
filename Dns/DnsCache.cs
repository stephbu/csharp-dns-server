// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="DnsCache.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Runtime.Caching;
    using Dns.Contracts;

    internal class DnsCache : IDnsCache
    {
        private readonly MemoryCache _cache = new MemoryCache("DnsCache");

        byte[] IDnsCache.Get(string key)
        {
            return _cache[key] as byte[];
        }

        void IDnsCache.Set(string key, byte[] bytes, int ttlSeconds)
        {
            CacheItem item = new CacheItem(key, bytes);
            CacheItemPolicy policy = new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now + TimeSpan.FromSeconds(ttlSeconds)};

            _cache.Add(item, policy);
        }
    }
}