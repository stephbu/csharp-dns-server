// // //-------------------------------------------------------------------------------------------------
// // // <copyright file="DnsQueryClient.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace DnsTest.Integration
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Dns;

    internal sealed class DnsQueryClient
    {
        private readonly IPEndPoint _endpoint;
        private readonly TimeSpan _timeout;
        private int _messageId;

        public DnsQueryClient(IPEndPoint endpoint, TimeSpan? timeout = null)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _timeout = timeout ?? TimeSpan.FromSeconds(5);
            _messageId = Environment.TickCount;
        }

        public async Task<DnsMessage> QueryAsync(string hostName, ResourceType resourceType = ResourceType.A, bool recursionDesired = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentException("A host name is required.", nameof(hostName));
            }

            var queryMessage = CreateQuery(hostName, resourceType, recursionDesired);
            byte[] payload = SerializeMessage(queryMessage);

            using UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork);
            await udpClient.SendAsync(payload, payload.Length, _endpoint).ConfigureAwait(false);

            Task<UdpReceiveResult> receiveTask = udpClient.ReceiveAsync();
            Task timeoutTask = Task.Delay(_timeout, cancellationToken);

            Task completed = await Task.WhenAny(receiveTask, timeoutTask).ConfigureAwait(false);
            if (completed != receiveTask)
            {
                if (timeoutTask.IsCanceled)
                {
                    throw new OperationCanceledException("DNS query was cancelled.", cancellationToken);
                }

                throw new TimeoutException($"Timed out waiting for DNS response for {hostName}.");
            }

            UdpReceiveResult receiveResult = await receiveTask.ConfigureAwait(false);
            if (!DnsMessage.TryParse(receiveResult.Buffer, out DnsMessage response))
            {
                throw new InvalidDataException("Unable to parse DNS response.");
            }

            if (response.QueryIdentifier != queryMessage.QueryIdentifier)
            {
                throw new InvalidOperationException("Received DNS response with mismatched identifier.");
            }

            return response;
        }

        private DnsMessage CreateQuery(string hostName, ResourceType resourceType, bool recursionDesired)
        {
            var message = new DnsMessage();
            message.QueryIdentifier = (ushort)Interlocked.Increment(ref _messageId);
            message.QuestionCount = 1;
            message.RD = recursionDesired;
            message.Questions.Add(new Question
            {
                Name = hostName,
                Class = ResourceClass.IN,
                Type = resourceType
            });
            return message;
        }

        private static byte[] SerializeMessage(DnsMessage message)
        {
            using MemoryStream stream = new MemoryStream();
            message.WriteToStream(stream);
            return stream.ToArray();
        }
    }
}
