// //-------------------------------------------------------------------------------------------------
// // <copyright file="DnsLookupKey.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;

    /// <summary>
    /// Value-type key for DNS request/response tracking.
    /// Eliminates string allocation (~80 bytes) per lookup by using struct with precomputed hash.
    /// </summary>
    /// <remarks>
    /// Phase 4 optimization: replaces string.Format("{id}|{class}|{type}|{name}") with zero-allocation struct.
    /// Used in DnsServer._requestResponseMap to correlate forwarded queries with their originators.
    /// </remarks>
    public readonly struct DnsRequestKey : IEquatable<DnsRequestKey>
    {
        public readonly ushort QueryId;
        public readonly ResourceClass Class;
        public readonly ResourceType Type;
        public readonly string Name;
        private readonly int _hashCode;

        public DnsRequestKey(ushort queryId, ResourceClass resClass, ResourceType resType, string name)
        {
            QueryId = queryId;
            Class = resClass;
            Type = resType;
            Name = name ?? string.Empty;
            // Precompute hash for fast dictionary lookups
            _hashCode = HashCode.Combine(QueryId, Class, Type, StringComparer.OrdinalIgnoreCase.GetHashCode(Name));
        }

        public DnsRequestKey(DnsMessage message)
            : this(
                message.QueryIdentifier,
                message.QuestionCount > 0 ? message.Questions[0].Class : ResourceClass.IN,
                message.QuestionCount > 0 ? message.Questions[0].Type : ResourceType.A,
                message.QuestionCount > 0 ? message.Questions[0].Name : string.Empty)
        {
        }

        public bool Equals(DnsRequestKey other)
        {
            return QueryId == other.QueryId
                && Class == other.Class
                && Type == other.Type
                && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => obj is DnsRequestKey other && Equals(other);

        public override int GetHashCode() => _hashCode;

        public static bool operator ==(DnsRequestKey left, DnsRequestKey right) => left.Equals(right);
        public static bool operator !=(DnsRequestKey left, DnsRequestKey right) => !left.Equals(right);

        public override string ToString() => $"{QueryId}|{Class}|{Type}|{Name}";
    }

    /// <summary>
    /// Value-type key for DNS zone lookups.
    /// Eliminates string allocation for zone map key generation.
    /// </summary>
    /// <remarks>
    /// Phase 4 optimization: replaces string.Format("{host}|{class}|{type}") with zero-allocation struct.
    /// Used in SmartZoneResolver._zoneMap for hostname resolution.
    /// </remarks>
    public readonly struct DnsZoneLookupKey : IEquatable<DnsZoneLookupKey>
    {
        public readonly ResourceClass Class;
        public readonly ResourceType Type;
        public readonly string Host;
        private readonly int _hashCode;

        public DnsZoneLookupKey(string host, ResourceClass resClass, ResourceType resType)
        {
            Host = host ?? string.Empty;
            Class = resClass;
            Type = resType;
            // Precompute hash for fast dictionary lookups (case-insensitive for DNS)
            _hashCode = HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(Host),
                Class,
                Type);
        }

        public bool Equals(DnsZoneLookupKey other)
        {
            return Class == other.Class
                && Type == other.Type
                && string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => obj is DnsZoneLookupKey other && Equals(other);

        public override int GetHashCode() => _hashCode;

        public static bool operator ==(DnsZoneLookupKey left, DnsZoneLookupKey right) => left.Equals(right);
        public static bool operator !=(DnsZoneLookupKey left, DnsZoneLookupKey right) => !left.Equals(right);

        public override string ToString() => $"{Host}|{Class}|{Type}";
    }
}
