// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="RData.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.IO;
    using System.Net;

    public abstract class RData
    {
        public abstract void Dump();
        public abstract void WriteToStream(Stream stream);

        public abstract ushort Length { get; }

    }

    public class ANameRData : RData
    {
        public IPAddress Address
        {
            get; 
            set;
        }

        public static ANameRData Parse(byte[] bytes, int offset, int size)
        {
            ANameRData aname = new ANameRData();
            uint addressBytes = BitConverter.ToUInt32(bytes, offset);
            aname.Address = new IPAddress(addressBytes);
            return aname;
        }

        public override void WriteToStream(Stream stream)
        {
            byte[] bytes = this.Address.GetAddressBytes();
            stream.Write(bytes, 0, bytes.Length);
        }

        public override ushort Length
        {
            get { return 4; }
        }

        public override void Dump()
        {
            Console.WriteLine("Address:   {0}", this.Address.ToString());
        }
    }

    public class CNameRData : RData
    {
        public string Name { get; set; }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort) (Name.Length + 2); }
        }

        public static CNameRData Parse(byte[] bytes, int offset, int size)
        {
            CNameRData cname = new CNameRData();
            cname.Name = DnsProtocol.ReadString(bytes, ref offset);
            return cname;
        }

        public override void WriteToStream(Stream stream)
        {
            Name.WriteToStream(stream);
        }

        public override void Dump()
        {
            Console.WriteLine("CName:   {0}", this.Name);
        }
    }

    public class DomainNamePointRData : RData
    {
        public string Name { get; set; }

        public static DomainNamePointRData Parse(byte[] bytes, int offset, int size)
        {
            DomainNamePointRData domainName = new DomainNamePointRData();
            domainName.Name = DnsProtocol.ReadString(bytes, ref offset);
            return domainName;
        }

        public override void WriteToStream(Stream stream)
        {
            Name.WriteToStream(stream);
        }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort)(Name.Length + 2); }
        }

        public override void Dump()
        {
            Console.WriteLine("DName:   {0}", this.Name);
        }
    }

    public class NameServerRData : RData
    {
        public string Name { get; set; }

        public static NameServerRData Parse(byte[] bytes, int offset, int size)
        {
            NameServerRData nsRdata = new NameServerRData();
            nsRdata.Name = DnsProtocol.ReadString(bytes, ref offset);
            return nsRdata;
        }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort)(Name.Length + 2); }
        }

        public override void WriteToStream(Stream stream)
        {
            this.Name.WriteToStream(stream);
        }


        public override void Dump()
        {
            Console.WriteLine("NameServer:   {0}", this.Name);
        }
    }

    public class StatementOfAuthorityRData : RData
    {

        public string PrimaryNameServer { get; set; }
        public string ResponsibleAuthoritativeMailbox { get; set; }
        public uint Serial { get; set; }
        public uint RefreshInterval { get; set; }
        public uint RetryInterval { get; set; }
        public uint ExpirationLimit { get; set; }
        public uint MinimumTTL { get; set; }

        public static StatementOfAuthorityRData Parse(byte[] bytes, int offset, int size)
        {
            StatementOfAuthorityRData soaRdata = new StatementOfAuthorityRData();
            soaRdata.PrimaryNameServer = DnsProtocol.ReadString(bytes, ref offset);
            soaRdata.ResponsibleAuthoritativeMailbox = DnsProtocol.ReadString(bytes, ref offset);
            soaRdata.Serial = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.RefreshInterval = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.RetryInterval = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.ExpirationLimit = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.MinimumTTL = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            return soaRdata;
        }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort) (PrimaryNameServer.Length + 2 + ResponsibleAuthoritativeMailbox.Length + 2 + 20); }
        }

        public override void WriteToStream(Stream stream)
        {
            this.PrimaryNameServer.WriteToStream(stream);
            this.ResponsibleAuthoritativeMailbox.WriteToStream(stream);
            this.Serial.SwapEndian().WriteToStream(stream);
            this.RefreshInterval.SwapEndian().WriteToStream(stream);
            this.RetryInterval.SwapEndian().WriteToStream(stream);
            this.ExpirationLimit.SwapEndian().WriteToStream(stream);
            this.MinimumTTL.SwapEndian().WriteToStream(stream);
        }

        public override void Dump()
        {
            Console.WriteLine("PrimaryNameServer:               {0}", this.PrimaryNameServer);
            Console.WriteLine("ResponsibleAuthoritativeMailbox: {0}", this.ResponsibleAuthoritativeMailbox);
            Console.WriteLine("Serial:                          {0}", this.Serial);
            Console.WriteLine("RefreshInterval:                 {0}", this.RefreshInterval);
            Console.WriteLine("RetryInterval:                   {0}", this.RetryInterval);
            Console.WriteLine("ExpirationLimit:                 {0}", this.ExpirationLimit);
            Console.WriteLine("MinimumTTL:                      {0}", this.MinimumTTL);
        }
    }

}