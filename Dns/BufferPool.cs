// //-------------------------------------------------------------------------------------------------
// // <copyright file="BufferPool.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net.Sockets;

    /// <summary>
    /// Provides pooled buffers and objects for high-performance DNS request processing.
    /// Designed to minimize allocations in the hot path.
    /// </summary>
    public static class BufferPool
    {
        /// <summary>
        /// Default buffer size for DNS messages. Standard DNS is 512 bytes,
        /// but EDNS(0) extends this to 4096 bytes. We use 4096 to be future-proof.
        /// </summary>
        public const int DefaultBufferSize = 4096;

        /// <summary>
        /// Maximum number of SocketAsyncEventArgs to keep pooled.
        /// </summary>
        private const int MaxPooledSocketArgs = 64;

        /// <summary>
        /// Shared memory pool for byte buffers.
        /// Uses ArrayPool under the hood with efficient bucket sizes.
        /// </summary>
        private static readonly MemoryPool<byte> s_memoryPool = MemoryPool<byte>.Shared;

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs for send operations.
        /// </summary>
        private static readonly ConcurrentBag<SocketAsyncEventArgs> s_socketArgsPool = new ConcurrentBag<SocketAsyncEventArgs>();

        /// <summary>
        /// Pool of reusable MemoryStream objects for response serialization.
        /// </summary>
        private static readonly ConcurrentBag<PooledMemoryStream> s_streamPool = new ConcurrentBag<PooledMemoryStream>();

        /// <summary>
        /// Rents a buffer from the pool. The buffer may be larger than requested.
        /// Caller must dispose the returned IMemoryOwner when done.
        /// </summary>
        /// <param name="minBufferSize">Minimum buffer size needed.</param>
        /// <returns>A memory owner that must be disposed to return the buffer.</returns>
        public static IMemoryOwner<byte> RentBuffer(int minBufferSize = DefaultBufferSize)
        {
            return s_memoryPool.Rent(minBufferSize);
        }

        /// <summary>
        /// Gets a SocketAsyncEventArgs from the pool or creates a new one.
        /// </summary>
        /// <returns>A SocketAsyncEventArgs ready for use.</returns>
        public static SocketAsyncEventArgs RentSocketAsyncEventArgs()
        {
            if (s_socketArgsPool.TryTake(out var args))
            {
                return args;
            }
            return new SocketAsyncEventArgs();
        }

        /// <summary>
        /// Returns a SocketAsyncEventArgs to the pool for reuse.
        /// </summary>
        /// <param name="args">The args to return.</param>
        public static void ReturnSocketAsyncEventArgs(SocketAsyncEventArgs args)
        {
            if (args == null) return;

            // Clear state for reuse
            args.SetBuffer(null, 0, 0);
            args.RemoteEndPoint = null;
            args.UserToken = null;

            // Only pool if we haven't exceeded our limit
            if (s_socketArgsPool.Count < MaxPooledSocketArgs)
            {
                s_socketArgsPool.Add(args);
            }
            else
            {
                args.Dispose();
            }
        }

        /// <summary>
        /// Gets a MemoryStream from the pool or creates a new one.
        /// The stream is reset to position 0 and ready for writing.
        /// </summary>
        /// <param name="capacity">Initial capacity hint.</param>
        /// <returns>A pooled memory stream that should be disposed when done.</returns>
        public static PooledMemoryStream RentMemoryStream(int capacity = DefaultBufferSize)
        {
            if (s_streamPool.TryTake(out var stream))
            {
                stream.SetLength(0);
                stream.Position = 0;
                return stream;
            }
            return new PooledMemoryStream(capacity);
        }

        /// <summary>
        /// Returns a PooledMemoryStream to the pool.
        /// Called automatically by PooledMemoryStream.Dispose().
        /// </summary>
        internal static void ReturnMemoryStream(PooledMemoryStream stream)
        {
            if (stream == null) return;

            // Reset and return to pool
            stream.SetLength(0);
            stream.Position = 0;
            s_streamPool.Add(stream);
        }
    }

    /// <summary>
    /// A MemoryStream that returns itself to the pool when disposed.
    /// </summary>
    public class PooledMemoryStream : MemoryStream
    {
        private bool _disposed;

        public PooledMemoryStream(int capacity) : base(capacity)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = false; // Reset so it can be reused
                BufferPool.ReturnMemoryStream(this);
                return; // Don't call base.Dispose - we want to reuse this
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the underlying buffer without creating a copy.
        /// Only valid bytes are from 0 to Position.
        /// </summary>
        public ArraySegment<byte> GetBufferSegment()
        {
            return new ArraySegment<byte>(GetBuffer(), 0, (int)Position);
        }
    }
}
