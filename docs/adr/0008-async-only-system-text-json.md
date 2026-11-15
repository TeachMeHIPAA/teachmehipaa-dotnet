# Async-only, System.Text.Json source generation, DelegatingHandler pipeline

`teachmehipaa-dotnet` exposes only `async` methods that take a
`CancellationToken`. There are no sync twins, because sync-over-async wrappers
deadlock under some synchronization contexts. Serialization is
System.Text.Json with a source-generated context, so it's trim- and AOT-safe;
there's no Newtonsoft. Auth and retries are `DelegatingHandler`s, so the
client works with `IHttpClientFactory` through `AddTeachMeHipaa()`. Targets
are `net8.0` and `netstandard2.0`.
