# csharp-dns-server

Fully functional DNS server written in C#.

## Features
- Pluggable Zone Resolver.  Host a zone locally and run your code to resolve names in that zone.  Enables many complex scenarios such as 
  - round-robin load-balancing.  Distribute load and provide failover with a datacentre without expensive hardware.
  - health-checks.  Pull machines from DNS when they fail health response
  - time-based constraints. Parental controls blockage of Facebook.
- Delegates all other DNS lookup to host machines default DNS server(s)
