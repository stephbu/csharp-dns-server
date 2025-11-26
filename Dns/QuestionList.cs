// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="QuestionList.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.IO;

    public class QuestionList : List<Question>
    {
        public int LoadFrom(byte[] bytes, int offset, ushort count)
        {
            int currentOffset = offset;

            for (int index = 0; index < count; index++)
            {
                // TODO: move this code into the Question object

                Question question = new Question();

                question.Name = DnsProtocol.ReadString(bytes, ref currentOffset);

                // Phase 5: Use BinaryPrimitives for zero-allocation reads
                var span = bytes.AsSpan(currentOffset);
                question.Type = (ResourceType)BinaryPrimitives.ReadUInt16BigEndian(span);
                currentOffset += 2;

                question.Class = (ResourceClass)BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2));
                currentOffset += 2;

                this.Add(question);
            }

            int bytesRead = currentOffset - offset;
            return bytesRead;
        }

        public long WriteToStream(Stream stream)
        {
            long start = stream.Length;
            foreach (Question question in this)
            {
                question.WriteToStream(stream);
            }
            long end = stream.Length;
            return end - start;
        }
    }
}
