// //-------------------------------------------------------------------------------------------------
// // <copyright file="RequestProcessingBenchmarks.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace DnsBench
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using BenchmarkDotNet.Attributes;
    using Dns;

    /// <summary>
    /// Benchmarks simulating end-to-end request processing.
    /// Focus: Buffer allocations, MemoryStream usage, dictionary operations.
    /// </summary>
    [MemoryDiagnoser]
    public class RequestProcessingBenchmarks
    {
        // Incoming query bytes
        private byte[] _queryBytes;

        // Pre-parsed message for response building
        private DnsMessage _parsedQuery;

        // Simulated request-response map (like in DnsServer)
        private Dictionary<string, EndPoint> _requestResponseMap;
        private ReaderWriterLockSlim _requestResponseMapLock;

        // Sample endpoint
        private EndPoint _sampleEndpoint;

        [GlobalSetup]
        public void Setup()
        {
            // Query for www.example.com A IN
            _queryBytes = new byte[]
            {
                0xAB, 0xCD, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x03, 0x77, 0x77, 0x77,
                0x07, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65,
                0x03, 0x63, 0x6F, 0x6D,
                0x00, 0x00, 0x01, 0x00, 0x01
            };

            DnsMessage.TryParse(_queryBytes, out _parsedQuery);

            _requestResponseMap = new Dictionary<string, EndPoint>();
            _requestResponseMapLock = new ReaderWriterLockSlim();
            _sampleEndpoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 12345);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _requestResponseMapLock?.Dispose();
        }

        /// <summary>
        /// Simulates key generation for request-response tracking.
        /// This is the current implementation in DnsServer.GetKeyName().
        /// </summary>
        [Benchmark(Description = "GetKeyName: string.Format (current)")]
        public string GetKeyName_StringFormat()
        {
            return string.Format("{0}|{1}|{2}|{3}",
                _parsedQuery.QueryIdentifier,
                _parsedQuery.Questions[0].Class,
                _parsedQuery.Questions[0].Type,
                _parsedQuery.Questions[0].Name);
        }

        /// <summary>
        /// Alternative using string interpolation.
        /// </summary>
        [Benchmark(Description = "GetKeyName: interpolation")]
        public string GetKeyName_Interpolation()
        {
            return $"{_parsedQuery.QueryIdentifier}|{_parsedQuery.Questions[0].Class}|{_parsedQuery.Questions[0].Type}|{_parsedQuery.Questions[0].Name}";
        }

        /// <summary>
        /// Simulates the full parse → build response → serialize cycle.
        /// This is the hot path for authoritative responses.
        /// </summary>
        [Benchmark(Description = "Full cycle: Parse → Build response → Serialize")]
        public byte[] FullCycle_AuthoritativeResponse()
        {
            // Parse incoming
            DnsMessage.TryParse(_queryBytes, out var message);

            // Build response (simulating authoritative answer)
            message.QR = true;
            message.AA = true;
            message.RA = false;
            message.RCode = (byte)RCode.NOERROR;
            message.AnswerCount = 1;

            var rdata = new ANameRData { Address = IPAddress.Parse("192.0.2.1") };
            message.Answers.Add(new ResourceRecord
            {
                Name = message.Questions[0].Name,
                Class = ResourceClass.IN,
                Type = ResourceType.A,
                TTL = 300,
                RData = rdata,
                DataLength = rdata.Length
            });

            // Serialize response
            using var responseStream = new MemoryStream(512);
            message.WriteToStream(responseStream);
            return responseStream.GetBuffer();
        }

        /// <summary>
        /// Simulates buffer copy pattern from UdpListener.
        /// Current code: new byte[bytesRead] + Buffer.BlockCopy
        /// </summary>
        [Benchmark(Description = "Buffer copy: new byte[] + BlockCopy")]
        public byte[] BufferCopy_NewArray()
        {
            int bytesRead = _queryBytes.Length;
            byte[] payload = new byte[bytesRead];
            System.Buffer.BlockCopy(_queryBytes, 0, payload, 0, bytesRead);
            return payload;
        }

        /// <summary>
        /// Simulates the MemoryStream allocation pattern in DnsServer.
        /// Current code: new MemoryStream(512) per response
        /// </summary>
        [Benchmark(Description = "MemoryStream: new per request")]
        public MemoryStream MemoryStream_NewPerRequest()
        {
            var stream = new MemoryStream(512);
            _parsedQuery.WriteToStream(stream);
            return stream;
        }

        /// <summary>
        /// Simulates pooled MemoryStream usage (Phase 2 optimization).
        /// Uses BufferPool.RentMemoryStream() pattern.
        /// </summary>
        [Benchmark(Description = "MemoryStream: pooled (Phase 2)")]
        public PooledMemoryStream MemoryStream_Pooled()
        {
            var stream = BufferPool.RentMemoryStream();
            _parsedQuery.WriteToStream(stream);
            stream.Dispose(); // Returns to pool
            return stream;
        }

        /// <summary>
        /// Simulates SocketAsyncEventArgs allocation pattern.
        /// Current code: new SocketAsyncEventArgs() per send
        /// </summary>
        [Benchmark(Description = "SocketAsyncEventArgs: new per send")]
        public System.Net.Sockets.SocketAsyncEventArgs SocketAsyncEventArgs_New()
        {
            var args = new System.Net.Sockets.SocketAsyncEventArgs();
            args.RemoteEndPoint = _sampleEndpoint;
            args.SetBuffer(_queryBytes, 0, _queryBytes.Length);
            return args;
        }

        /// <summary>
        /// Simulates pooled SocketAsyncEventArgs usage (Phase 2 optimization).
        /// Uses BufferPool.RentSocketAsyncEventArgs() pattern.
        /// </summary>
        [Benchmark(Description = "SocketAsyncEventArgs: pooled (Phase 2)")]
        public System.Net.Sockets.SocketAsyncEventArgs SocketAsyncEventArgs_Pooled()
        {
            var args = BufferPool.RentSocketAsyncEventArgs();
            args.RemoteEndPoint = _sampleEndpoint;
            args.SetBuffer(_queryBytes, 0, _queryBytes.Length);
            BufferPool.ReturnSocketAsyncEventArgs(args);
            return args;
        }

        /// <summary>
        /// Simulates buffer rental from MemoryPool (Phase 2 optimization).
        /// Uses BufferPool.RentBuffer() pattern.
        /// </summary>
        [Benchmark(Description = "Buffer: MemoryPool rental (Phase 2)")]
        public IMemoryOwner<byte> Buffer_MemoryPoolRental()
        {
            var owner = BufferPool.RentBuffer();
            // Simulate copy into rented buffer
            _queryBytes.AsSpan().CopyTo(owner.Memory.Span);
            return owner;
        }

        /// <summary>
        /// Simulates dictionary add with ReaderWriterLockSlim (write path).
        /// </summary>
        [Benchmark(Description = "RequestMap: Add with RWLock")]
        public void RequestMap_AddWithLock()
        {
            string key = $"{_parsedQuery.QueryIdentifier}|test";
            try
            {
                _requestResponseMapLock.EnterWriteLock();
                _requestResponseMap[key] = _sampleEndpoint;
            }
            finally
            {
                _requestResponseMapLock.ExitWriteLock();
            }
            // Cleanup
            try
            {
                _requestResponseMapLock.EnterWriteLock();
                _requestResponseMap.Remove(key);
            }
            finally
            {
                _requestResponseMapLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Simulates dictionary lookup with ReaderWriterLockSlim (read path).
        /// </summary>
        [Benchmark(Description = "RequestMap: TryGet with RWLock")]
        public EndPoint RequestMap_TryGetWithLock()
        {
            string key = $"{_parsedQuery.QueryIdentifier}|test";
            // First add
            try
            {
                _requestResponseMapLock.EnterWriteLock();
                _requestResponseMap[key] = _sampleEndpoint;
            }
            finally
            {
                _requestResponseMapLock.ExitWriteLock();
            }

            EndPoint result = null;
            try
            {
                _requestResponseMapLock.EnterReadLock();
                _requestResponseMap.TryGetValue(key, out result);
            }
            finally
            {
                _requestResponseMapLock.ExitReadLock();
            }

            // Cleanup
            try
            {
                _requestResponseMapLock.EnterWriteLock();
                _requestResponseMap.Remove(key);
            }
            finally
            {
                _requestResponseMapLock.ExitWriteLock();
            }

            return result;
        }
    }
}
