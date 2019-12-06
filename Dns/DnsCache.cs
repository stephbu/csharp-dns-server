// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="DnsCache.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using Microsoft.Extensions.Caching.Memory;
    using Dns.Contracts;

    public class DnsCache : IDnsCache
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        byte[] IDnsCache.Get(string key)
        {
            byte[] entry;
            if (_cache.TryGetValue(key, out entry)) {
                return entry;
            }

            return null;
        }

        void IDnsCache.Set(string key, byte[] bytes, int ttlSeconds)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.Now + TimeSpan.FromSeconds(ttlSeconds));
            _cache.Set(key, bytes, cacheEntryOptions);
        }
    }
}