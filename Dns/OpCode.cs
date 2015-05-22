// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="OpCode.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    public enum OpCode
    {
        QUERY = 0,
        IQUERY = 1,
        STATUS = 2,
        NOTIFY = 4,
        UPDATE = 5,
    }
}