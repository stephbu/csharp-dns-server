# Task 6 Plan – Migrate to Microsoft.Extensions.DependencyInjection

## Goal
Replace the ad-hoc Ninject usage across the solution with the built-in `Microsoft.Extensions.DependencyInjection` (MS.DI) stack so the DNS server, CLI host, and tests share a consistent, modern dependency injection pipeline compatible with .NET 8.

## Scope
- **Projects**: `Dns`, `dns-cli`, and `dnstest` (any code instantiating `Program.Run` or depending on the service provider).
- **Files**: `Dns/Program.cs`, `dns-cli/Program.cs` (if they configure DI directly), any zone providers or resolvers that currently rely on `IKernel`.
- **Out of Scope**: Introducing new services or refactoring unrelated runtime logic; focus strictly on the container swap while keeping behavior identical.

## Steps
1. **Inventory current bindings**  
   - Examine `Dns/Program.cs` to list every type registered via `container.Bind<>().To(...)`.  
   - Identify transient vs singleton semantics and how configuration sections are passed into zone providers.

2. **Design MS.DI composition root**  
   - Decide where to build `IHost`/`ServiceProvider` (likely in `Dns/Program.Run`).  
   - Map Ninject lifetimes to MS.DI lifetimes (`Singleton`, `Scoped`, `Transient`).  
   - Determine how to register configuration (`IConfiguration`, options classes) so zone providers receive required settings.

3. **Implement the container swap**  
   - Remove Ninject references/packages from the solution and add `Microsoft.Extensions.DependencyInjection` (and possibly `Microsoft.Extensions.Hosting`).  
   - Introduce a `ServiceCollection` setup in `Program.Run`, registering zone providers via reflection (mirroring `ByName`) or using configuration-driven type lookup.  
   - Replace `_zoneProvider = container.Get<...>()` with `provider.GetRequiredService<...>()`.

4. **Update entry points and consumers**  
   - Ensure `dns-cli` and any tests constructing `Program.Run` use the new DI pipeline (e.g., pass an optional `IServiceProvider` or factory if needed).  
   - Confirm `SmartZoneResolver`, `DnsServer`, and `HttpServer` dependencies are resolved through the new provider instead of manual `new`.

5. **Clean up configuration wiring**  
   - Register strongly typed options via `services.Configure<AppConfig>(configuration)` or equivalent so components can consume options via `IOptions<T>`.  
   - Remove remaining Ninject-specific code paths and update `using` statements.

6. **Validation**  
   - Run `dotnet build` and `dotnet test csharp-dns-server.sln`.  
   - Exercise `dns-cli` locally with `dotnet run -- ./appsettings.json` to ensure the server still starts, loads zones, and answers queries.

7. **Documentation**  
   - Update `README.md`/`AGENTS.md` build notes to mention MS.DI usage and remove references to Ninject.  
   - Mark T06 done in `docs/task_list.md` when merged.

## Acceptance Criteria
- All projects build without Ninject dependencies and instead rely on MS.DI.  
- Runtime behavior (zone loading, DNS/HTTP serving) matches the pre-migration behavior.  
- Tests and CLI runs succeed without manual container wiring.  
- Documentation reflects the new dependency injection approach.
