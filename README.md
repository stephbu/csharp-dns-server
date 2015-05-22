# csharp-dns-server

Fully functional DNS server written in C#.

## Features
- Pluggable Zone Resolver.  Host a zone locally and run your code to resolve names in that zone.  Enables complex scenarios such as round-robin load-balancing, health-checks, time-based constraints e.g. parental control block Facebook
- Delegates all other DNS lookup to host machines default DNS server(s)
