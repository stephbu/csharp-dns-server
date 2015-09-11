// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Resource.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.IO;

    public class ResourceRecord
    {
        public string Name { get; set; }
        public uint TTL { get; set; }
        public ResourceClass Class { get; set; }
        public ResourceType Type { get; set; }
        public RData RData { get; set;}
        public ushort DataLength { get; set; }

        /// <summary>Serialize resource to stream according to RFC1034 format</summary>
        /// <param name="stream"></param>
        public void WriteToStream(Stream stream)
        {
            this.Name.WriteToStream(stream);
            ((ushort)(this.Type)).SwapEndian().WriteToStream(stream);
            ((ushort)(this.Class)).SwapEndian().WriteToStream(stream);
            this.TTL.SwapEndian().WriteToStream(stream);

            if(this.RData != null)
            {
                this.RData.Length.SwapEndian().WriteToStream(stream);
                this.RData.WriteToStream(stream);
            }
            else
            {
                // no RDATA write (ushort) DataLength=0
                stream.WriteByte(0x00);
                stream.WriteByte(0x00);
            }
        }

        public void Dump()
        {
            Console.WriteLine("ResourceName:   {0}", this.Name);
            Console.WriteLine("ResourceType:   {0}", this.Type);
            Console.WriteLine("ResourceClass:  {0}", this.Class);
            Console.WriteLine("TimeToLive:     {0}", this.TTL);
            Console.WriteLine("DataLength:     {0}", this.DataLength);

            if (this.RData != null)
            {
                this.RData.Dump();
            }
        }
    }

}