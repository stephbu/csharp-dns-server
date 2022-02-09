// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="UdpListener.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate void OnRequestHandler(byte[] buffer, EndPoint remoteEndPoint);

    public class UdpListener
    {
        public OnRequestHandler OnRequest;
        private Socket _listener;

        public void Initialize(ushort port = 53)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            _listener.Bind(ep);
        }

        public async void Start()
        {
            while (true)
            {
                try
                {
                    // Reusable SocketAsyncEventArgs and awaitable wrapper 
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.SetBuffer(new byte[0x1000], 0, 0x1000);
                    args.RemoteEndPoint = _listener.LocalEndPoint;
                    SocketAwaitable awaitable = new SocketAwaitable(args);

                    // Do processing, continually receiving from the socket 
                    while (true)
                    {
                        await _listener.ReceiveFromAsync(awaitable);
                        int bytesRead = args.BytesTransferred;
                        if (bytesRead <= 0)
                        {
                            break;
                        }

                        if (OnRequest != null)
                        {
                            var buffer = new byte[bytesRead];
                            Buffer.BlockCopy(args.Buffer, 0, buffer, 0, buffer.Length);
                            var process = Task.Run(() => OnRequest(buffer, args.RemoteEndPoint));
                        }
                        else
                        {
                            // defaults to console dump if no listener is bound
                            var dump = Task.Run(() => ProcessReceiveFrom(args));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                // listener restarts if an exception occurs
            }
        }

        public void Stop()
        {
            _listener.Close();
        }

        public async void SendToAsync(SocketAsyncEventArgs args)
        {
            SocketAwaitable awaitable = new SocketAwaitable(args);
            await _listener.SendToAsync(awaitable);
        }

        public void ProcessReceiveFrom(SocketAsyncEventArgs args)
        {
            Console.WriteLine(args.RemoteEndPoint.ToString());
            Console.WriteLine(args.BytesTransferred);
        }
    }

    /// <summary>IO completion based socket await object</summary>
    public sealed class SocketAwaitable : INotifyCompletion
    {
        private static readonly Action SENTINEL = () => { };

        internal Action m_continuation;
        internal SocketAsyncEventArgs m_eventArgs;
        internal bool m_wasCompleted;

        public SocketAwaitable(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }
            m_eventArgs = eventArgs;
            eventArgs.Completed += delegate
                                   {
                                       Action prev = m_continuation ?? Interlocked.CompareExchange(ref m_continuation, SENTINEL, null);
                                       if (prev != null)
                                       {
                                           prev();
                                       }
                                   };
        }

        public bool IsCompleted
        {
            get { return m_wasCompleted; }
        }

        public void OnCompleted(Action continuation)
        {
            if (m_continuation == SENTINEL || Interlocked.CompareExchange(ref m_continuation, continuation, null) == SENTINEL)
            {
                Task.Run(continuation);
            }
        }

        internal void Reset()
        {
            m_wasCompleted = false;
            m_continuation = null;
        }

        public SocketAwaitable GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
            if (m_eventArgs.SocketError != SocketError.Success)
            {
                throw new SocketException((int) m_eventArgs.SocketError);
            }
        }
    }
}