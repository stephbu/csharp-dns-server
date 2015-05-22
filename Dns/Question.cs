// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Question.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.IO;

    public class Question
    {
        public ResourceClass Class;
        public string Name;
        public ResourceType Type;

        public void WriteToStream(Stream stream)
        {
            byte[] name = this.Name.GetResourceBytes();
            stream.Write(name, 0, name.Length);

            // Type
            stream.Write(BitConverter.GetBytes(((ushort) (this.Type)).SwapEndian()), 0, 2);

            // Class
            stream.Write(BitConverter.GetBytes(((ushort) this.Class).SwapEndian()), 0, 2);
        }
    }
}