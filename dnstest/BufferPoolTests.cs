// // //-------------------------------------------------------------------------------------------------
// // // <copyright file="BufferPoolTests.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Dns;
using Xunit;

namespace DnsTest
{
    /// <summary>
    /// Regression tests for BufferPool to ensure proper buffer lifecycle management.
    /// These tests verify that rented resources are properly returned to pools,
    /// preventing memory leaks and resource exhaustion under load.
    /// </summary>
    public class BufferPoolTests
    {
        #region MemoryPool Buffer Tests

        [Fact]
        public void RentBuffer_ReturnsBufferOfRequestedMinimumSize()
        {
            using var buffer = BufferPool.RentBuffer(512);

            Assert.NotNull(buffer);
            Assert.True(buffer.Memory.Length >= 512);
        }

        [Fact]
        public void RentBuffer_DefaultSize_ReturnsEDNSCompatibleBuffer()
        {
            using var buffer = BufferPool.RentBuffer();

            Assert.NotNull(buffer);
            Assert.True(buffer.Memory.Length >= BufferPool.DefaultBufferSize);
            Assert.True(buffer.Memory.Length >= 4096, "Buffer should support EDNS(0) 4096-byte messages");
        }

        [Fact]
        public void RentBuffer_DisposedBuffer_CanBeRentedAgain()
        {
            // Rent and return multiple buffers to verify pool reuse
            var buffers = new List<IMemoryOwner<byte>>();

            // Rent several buffers
            for (int i = 0; i < 10; i++)
            {
                buffers.Add(BufferPool.RentBuffer());
            }

            // Return all buffers
            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }

            buffers.Clear();

            // Rent again - should succeed without allocating new memory (pool reuse)
            for (int i = 0; i < 10; i++)
            {
                buffers.Add(BufferPool.RentBuffer());
            }

            // Verify all rentals succeeded
            Assert.Equal(10, buffers.Count);
            foreach (var buffer in buffers)
            {
                Assert.NotNull(buffer);
                Assert.True(buffer.Memory.Length >= BufferPool.DefaultBufferSize);
                buffer.Dispose();
            }
        }

        [Fact]
        public async Task RentBuffer_ConcurrentRentReturn_NoExceptions()
        {
            // Stress test concurrent rent/return operations
            const int iterations = 1000;
            var tasks = new List<Task>();

            for (int i = 0; i < iterations; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    using var buffer = BufferPool.RentBuffer();
                    // Simulate some work
                    buffer.Memory.Span[0] = 0xFF;
                }));
            }

            await Task.WhenAll(tasks);
            // If we get here without exceptions, concurrent access is safe
        }

        [Fact]
        public void RentBuffer_BufferContentsAreWritable()
        {
            using var buffer = BufferPool.RentBuffer(512);

            var span = buffer.Memory.Span;

            // Write test pattern
            for (int i = 0; i < 512; i++)
            {
                span[i] = (byte)(i % 256);
            }

            // Verify contents
            for (int i = 0; i < 512; i++)
            {
                Assert.Equal((byte)(i % 256), span[i]);
            }
        }

        [Fact]
        public async Task RentBuffer_HighThroughput_NoMemoryLeak()
        {
            // Simulate high-throughput DNS server usage pattern
            const int totalRequests = 10000;
            long initialMemory = GC.GetTotalMemory(true);

            var tasks = new List<Task>();
            for (int i = 0; i < totalRequests; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    using var buffer = BufferPool.RentBuffer();
                    // Simulate DNS packet processing
                    var span = buffer.Memory.Span;
                    span[0] = 0x00; // Query ID high byte
                    span[1] = 0x01; // Query ID low byte
                }));
            }

            await Task.WhenAll(tasks);

            // Force GC to clean up any unreturned buffers
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long finalMemory = GC.GetTotalMemory(true);

            // Memory growth should be minimal (pools retain some buffers)
            // Allow for reasonable pool retention overhead
            long memoryGrowth = finalMemory - initialMemory;
            Assert.True(memoryGrowth < 10 * 1024 * 1024, // Less than 10MB growth
                $"Memory grew by {memoryGrowth / 1024}KB after {totalRequests} requests - possible leak");
        }

        #endregion

        #region SocketAsyncEventArgs Pool Tests

        [Fact]
        public void RentSocketAsyncEventArgs_ReturnsValidInstance()
        {
            var args = BufferPool.RentSocketAsyncEventArgs();

            Assert.NotNull(args);

            BufferPool.ReturnSocketAsyncEventArgs(args);
        }

        [Fact]
        public void ReturnSocketAsyncEventArgs_ClearsState()
        {
            var args = BufferPool.RentSocketAsyncEventArgs();

            // Set some state
            args.SetBuffer(new byte[100], 0, 100);
            args.RemoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 53);
            args.UserToken = "test";

            // Return to pool
            BufferPool.ReturnSocketAsyncEventArgs(args);

            // Rent again (should get same instance from pool on first return)
            var args2 = BufferPool.RentSocketAsyncEventArgs();

            // State should be cleared
            Assert.Null(args2.Buffer);
            Assert.Null(args2.RemoteEndPoint);
            Assert.Null(args2.UserToken);

            BufferPool.ReturnSocketAsyncEventArgs(args2);
        }

        [Fact]
        public void ReturnSocketAsyncEventArgs_NullArgument_NoException()
        {
            // Should handle null gracefully
            BufferPool.ReturnSocketAsyncEventArgs(null);
        }

        [Fact]
        public void RentSocketAsyncEventArgs_PoolReuse_SameInstanceReturned()
        {
            var args1 = BufferPool.RentSocketAsyncEventArgs();
            BufferPool.ReturnSocketAsyncEventArgs(args1);

            var args2 = BufferPool.RentSocketAsyncEventArgs();

            // After return and re-rent, we should get the same pooled instance
            // (assuming single-threaded and pool wasn't empty)
            Assert.Same(args1, args2);

            BufferPool.ReturnSocketAsyncEventArgs(args2);
        }

        [Fact]
        public async Task SocketAsyncEventArgs_ConcurrentRentReturn_NoExceptions()
        {
            const int iterations = 100;
            var tasks = new List<Task>();

            for (int i = 0; i < iterations; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var args = BufferPool.RentSocketAsyncEventArgs();
                    // Simulate some work
                    args.SetBuffer(new byte[10], 0, 10);
                    BufferPool.ReturnSocketAsyncEventArgs(args);
                }));
            }

            await Task.WhenAll(tasks);
        }

        #endregion

        #region PooledMemoryStream Tests

        [Fact]
        public void RentMemoryStream_ReturnsValidInstance()
        {
            using var stream = BufferPool.RentMemoryStream();

            Assert.NotNull(stream);
            Assert.Equal(0, stream.Position);
            Assert.Equal(0, stream.Length);
        }

        [Fact]
        public void RentMemoryStream_WithCapacity_ReturnsUsableStream()
        {
            // Note: When getting from pool, capacity may be larger than requested
            // (from previously expanded stream). This test verifies the stream is usable.
            using var stream = BufferPool.RentMemoryStream(8192);

            Assert.NotNull(stream);
            Assert.Equal(0, stream.Position);
            Assert.Equal(0, stream.Length);

            // Verify we can write the requested capacity without issue
            var data = new byte[8192];
            stream.Write(data, 0, data.Length);
            Assert.Equal(8192, stream.Length);
        }

        [Fact]
        public void PooledMemoryStream_Dispose_ReturnsToPool()
        {
            PooledMemoryStream stream1;
            using (stream1 = BufferPool.RentMemoryStream())
            {
                stream1.Write(new byte[] { 1, 2, 3 }, 0, 3);
            }

            // After dispose, rent again - should get same instance
            using var stream2 = BufferPool.RentMemoryStream();

            // Stream should be reset
            Assert.Equal(0, stream2.Position);
            Assert.Equal(0, stream2.Length);

            // Should be the same pooled instance
            Assert.Same(stream1, stream2);
        }

        [Fact]
        public void PooledMemoryStream_Dispose_ClearsContents()
        {
            byte[] originalData;

            using (var stream = BufferPool.RentMemoryStream())
            {
                stream.Write(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, 0, 4);
                originalData = stream.ToArray();
            }

            using var stream2 = BufferPool.RentMemoryStream();

            // Position and Length should be reset
            Assert.Equal(0, stream2.Position);
            Assert.Equal(0, stream2.Length);
        }

        [Fact]
        public void PooledMemoryStream_GetBufferSegment_ReturnsValidSegment()
        {
            using var stream = BufferPool.RentMemoryStream();

            stream.Write(new byte[] { 1, 2, 3, 4, 5 }, 0, 5);

            var segment = stream.GetBufferSegment();

            Assert.Equal(0, segment.Offset);
            Assert.Equal(5, segment.Count);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, segment.ToArray());
        }

        [Fact]
        public async Task PooledMemoryStream_ConcurrentRentReturn_NoExceptions()
        {
            const int iterations = 100;
            var tasks = new List<Task>();

            for (int i = 0; i < iterations; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    using var stream = BufferPool.RentMemoryStream();
                    stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
                    stream.Position = 0;
                    stream.ReadByte();
                }));
            }

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task PooledMemoryStream_HighThroughput_PoolReuse()
        {
            // Track unique stream instances to verify pool reuse
            var seenInstances = new HashSet<int>();
            var lockObj = new object();

            const int iterations = 1000;
            var tasks = new List<Task>();

            for (int i = 0; i < iterations; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    using var stream = BufferPool.RentMemoryStream();

                    lock (lockObj)
                    {
                        seenInstances.Add(stream.GetHashCode());
                    }

                    stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
                }));
            }

            await Task.WhenAll(tasks);

            // With pooling, we should see fewer unique instances than iterations
            // The exact number depends on concurrency, but should be significantly less
            Assert.True(seenInstances.Count < iterations,
                $"Expected pool reuse but got {seenInstances.Count} unique instances for {iterations} iterations");
        }

        #endregion

        #region Integration Tests - Simulated DNS Request Pattern

        [Fact]
        public void SimulatedDnsRequest_BufferLifecycle_ProperCleanup()
        {
            // Simulate the buffer usage pattern in DnsServer
            const int requestCount = 100;

            for (int i = 0; i < requestCount; i++)
            {
                // Simulate receiving a DNS request (UdpListener pattern)
                using var receiveBuffer = BufferPool.RentBuffer();

                // Write simulated DNS query
                var memory = receiveBuffer.Memory;
                memory.Span[0] = (byte)(i >> 8); // Query ID high
                memory.Span[1] = (byte)(i & 0xFF); // Query ID low

                // Simulate building response (DnsServer pattern)
                using var responseStream = BufferPool.RentMemoryStream();
                responseStream.Write(memory.Span.Slice(0, 12).ToArray(), 0, 12);

                // Simulate sending response (DnsServer pattern)
                var args = BufferPool.RentSocketAsyncEventArgs();
                try
                {
                    var segment = responseStream.GetBufferSegment();
                    args.SetBuffer(segment.Array, segment.Offset, segment.Count);
                    // In real code, would call SendToAsync here
                }
                finally
                {
                    BufferPool.ReturnSocketAsyncEventArgs(args);
                }
            }

            // Force cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // If we get here without exceptions, lifecycle is correct
        }

        [Fact]
        public void BufferPool_DefaultBufferSize_MatchesEDNSRequirement()
        {
            // Regression test: EDNS(0) requires 4096-byte buffer support
            Assert.Equal(4096, BufferPool.DefaultBufferSize);
        }

        [Fact]
        public async Task BufferPool_UnderLoad_MaintainsStability()
        {
            // Stress test all pool components simultaneously
            const int duration = 2; // seconds
            var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(duration));
            var exceptions = new List<Exception>();
            var lockObj = new object();

            var tasks = new List<Task>();

            // Memory buffer workers
            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            using var buffer = BufferPool.RentBuffer();
                            buffer.Memory.Span[0] = 0xFF;
                            await Task.Yield();
                        }
                        catch (Exception ex)
                        {
                            lock (lockObj) exceptions.Add(ex);
                        }
                    }
                }));
            }

            // SocketAsyncEventArgs workers
            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var args = BufferPool.RentSocketAsyncEventArgs();
                            args.SetBuffer(new byte[10], 0, 10);
                            BufferPool.ReturnSocketAsyncEventArgs(args);
                            await Task.Yield();
                        }
                        catch (Exception ex)
                        {
                            lock (lockObj) exceptions.Add(ex);
                        }
                    }
                }));
            }

            // MemoryStream workers
            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            using var stream = BufferPool.RentMemoryStream();
                            stream.WriteByte(0xFF);
                            await Task.Yield();
                        }
                        catch (Exception ex)
                        {
                            lock (lockObj) exceptions.Add(ex);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            Assert.Empty(exceptions);
        }

        #endregion
    }
}
