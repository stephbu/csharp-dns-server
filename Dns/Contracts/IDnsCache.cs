// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="IDnsCache.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns.Contracts
{
    public interface IDnsCache
    {
        byte[] Get(string key);

        void Set(string key, byte[] bytes, int ttlSeconds);
    }
}