namespace Dns.Config
{
   public class AppConfig
   {
      public ServerOptions Server { get; set; } 
   }

   public class ServerOptions
   {
        public ZoneOptions Zone { get; set; }
        public DnsListenerOptions DnsListener { get; set;}
        public WebServerOptions WebServer { get; set; }
   }

   public class ZoneOptions
   {
      public string Name { get; set; }
      public string Provider { get; set; }
   }

   public class DnsListenerOptions
    {
      public ushort Port { get; set; }
   }

   public class WebServerOptions 
   {
      public bool Enabled { get; set; }
      public int Port { get; set; }
   }

}