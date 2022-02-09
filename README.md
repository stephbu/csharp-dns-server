# csharp-dns-server

Fully functional DNS server written in C#.

The project was conceived while working to reduce the cost of datacentre "stamps" while providing robust services within a datacentre, specifically to remove the need for an expensive load-balancer device by providing round-robin DNS services, and retrying connectivity instead.

## Licence
This software is licenced under MIT terms that permits reuse within proprietary software provided all copies of the licensed software include a copy of the MIT License terms and the copyright notice.  See [licence.txt](./licence.txt)

## Getting Started

```
// clone the repo
>> cd $repo-root
>> git clone https://github.com/stephbu/csharp-dns-server

// check you can build the project
>> cd $repo-root/csharp-dns-server
>> dotnet build

// check that the tests run
>> dotnet test

```

## Gotchas
- if you're running on Windows 10 with Docker Tools installed, Docker uses the ICS SharedAccess service to provide DNS resolution for Docker containers - this listens on UDP:53, and will conflict with the DNS project.  Either turn off the the service (```net stop SharedAccess```), or change the UDP port.

## Features

As written, the server has the following features:

 - Pluggable Zone Resolver.  Host one or more zones locally, and run your code to resolve names in that zone.  Enables many complex scenarios such as:
 - round-robin load-balancing.  Distribute load and provide failover with a datacentre without expensive hardware.
 - health-checks.  While maintaining a list of machines in round-robin for a name, the code performs periodic healthchecks against the machines, if necessary removing machines that fail the health checks from rotation.
 - Delegates all other DNS lookup to host machines default DNS server(s)

The DNS server has a built-in Web Server providing operational insight into the current server behaviour.
- healthcheck for server status
- counters
- zone information

## Interesting Possible Uses
Time-based constraints such as parental controls to block a site, e.g. Facebook.
Logging of site usage e.g. company notifications

## Challenges

### Testing

Two phases of testing was completed.

1) Verification that the bit-packing classes correctly added and removed bits in correct Endian order, complicated by network bitpacking in reverse order to Windows big-endian packing.

2) Protocol verification - that well known messages were correctly decoded and re-encoded using the bit-packing system.

Much time was spent using Netmon to capture real DNS challenges and verify that the C# DNS server responded appropriately.

### DNS-Sec
No effort made to handle or respond to DNS-Sec challenges.

## Contribution Guide
Pull Requests, Bug Reports, and Feature Requests are most welcome.  

### Contribution Workflow
Suggested workflow for PRs is

1. Make a fork of csharp-dns-server/master in your own repository.
2. Create a branch in your own repo to entirely encapsulate all your proposed changes
3. Make your changes, add documentation if you need it, markdown text preferred.
4. Squash your commits into a single change [(Find out how to squash here)](http://stackoverflow.com/questions/616556/how-do-you-squash-commits-into-one-patch-with-git-format-patch)
5. Submit a PR, and put in comments anything that you think I'll need to help merge and evaluate the changes

### Licence Reminder
All contributions must be licenced under the same MIT terms, do include a header file to that effect.

