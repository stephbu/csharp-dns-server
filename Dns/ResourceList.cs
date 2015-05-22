// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ResourceList.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ResourceList : List<Resource>
    {
        public int LoadFrom(byte[] bytes, int offset, ushort count)
        {
            int currentOffset = offset;

            for (int index = 0; index < count; index++)
            {
                // TODO: move this code into the Resource object

                Resource resource = new Resource();
                //// extract the domain, question type, question class and Ttl

                resource.Name = DnsProtocol.ReadString(bytes, ref currentOffset);

                resource.Type = (ResourceType) (BitConverter.ToUInt16(bytes, currentOffset).SwapEndian());
                currentOffset += sizeof (ushort);

                resource.Class = (ResourceClass) (BitConverter.ToUInt16(bytes, currentOffset).SwapEndian());
                currentOffset += sizeof (ushort);

                resource.TTL = BitConverter.ToUInt32(bytes, currentOffset).SwapEndian();
                currentOffset += sizeof (uint);

                resource.DataLength = BitConverter.ToUInt16(bytes, currentOffset).SwapEndian();
                currentOffset += sizeof (ushort);

                if (resource.Class == ResourceClass.IN && resource.Type == ResourceType.A)
                {
                    resource.RData = ANameRData.Parse(bytes, currentOffset, resource.DataLength);
                }
                else if (resource.Type == ResourceType.CNAME)
                {
                    resource.RData = CNameRData.Parse(bytes, currentOffset, resource.DataLength);
                }

                // move past resource data record
                currentOffset = currentOffset + resource.DataLength;

                this.Add(resource);
            }

            int bytesRead = currentOffset - offset;
            return bytesRead;
        }

        public void WriteToStream(Stream stream)
        {
            foreach (var resource in this)
            {
                resource.WriteToStream(stream);
            }
        }
    }
}