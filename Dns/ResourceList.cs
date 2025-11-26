// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ResourceList.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.IO;

    public class ResourceList : List<ResourceRecord>
    {
        public int LoadFrom(byte[] bytes, int offset, ushort count)
        {
            int currentOffset = offset;

            for (int index = 0; index < count; index++)
            {
                // TODO: move this code into the Resource object

                ResourceRecord resourceRecord = new ResourceRecord();
                //// extract the domain, question type, question class and Ttl

                resourceRecord.Name = DnsProtocol.ReadString(bytes, ref currentOffset);

                // Phase 5: Use BinaryPrimitives for zero-allocation reads
                var span = bytes.AsSpan(currentOffset);
                resourceRecord.Type = (ResourceType)BinaryPrimitives.ReadUInt16BigEndian(span);
                currentOffset += sizeof(ushort);

                resourceRecord.Class = (ResourceClass)BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2));
                currentOffset += sizeof(ushort);

                resourceRecord.TTL = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(4));
                currentOffset += sizeof(uint);

                resourceRecord.DataLength = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(8));
                currentOffset += sizeof(ushort);

                if (resourceRecord.Class == ResourceClass.IN && resourceRecord.Type == ResourceType.A)
                {
                    resourceRecord.RData = ANameRData.Parse(bytes, currentOffset, resourceRecord.DataLength);
                }
                else if (resourceRecord.Type == ResourceType.CNAME)
                {
                    resourceRecord.RData = CNameRData.Parse(bytes, currentOffset, resourceRecord.DataLength);
                }
                else if (resourceRecord.Type == ResourceType.SOA)
                {
                    resourceRecord.RData = StatementOfAuthorityRData.Parse(bytes, currentOffset, resourceRecord.DataLength);
                }

                // move past resource data record
                currentOffset = currentOffset + resourceRecord.DataLength;

                this.Add(resourceRecord);
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
