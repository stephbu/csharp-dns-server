// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="RCode.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    public enum RCode
    {
        NOERROR = 0,
        FORMERR = 1,
        SERVFAIL = 2,
        NXDOMAIN = 3,
        NOTIMP = 4,
        REFUSED = 5,
        YXDOMAIN = 6,
        YXRRSET = 7,
        NXRRSET = 8,
        NOTAUTH = 9,
        NOTZONE = 10,
        BADVERS = 16,
    }
}