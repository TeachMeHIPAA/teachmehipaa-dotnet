// Prints the Organization's Compliance summary and the first page of
// Learners from the Sandbox.
//
//   TMH_CLIENT_ID=cli_... TMH_CLIENT_SECRET=tmh_cs_... dotnet run --project examples/Quickstart
using TeachMeHipaa;

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = RequiredVariable("TMH_CLIENT_ID"),
    ClientSecret = RequiredVariable("TMH_CLIENT_SECRET"),
    Environment = TeachMeHipaaEnvironment.Sandbox,
});

var compliance = await client.Compliance.GetAsync();
Console.WriteLine($"{compliance.CompliantRate:P0} of {compliance.Totals.Learners} Learners are compliant");

var page = await client.Learners.ListAsync(new() { Limit = 10 });
foreach (var learner in page.Items)
{
    Console.WriteLine($"{learner.Email,-40} {learner.Status}");
}

if (page.HasNextPage)
{
    Console.WriteLine("…and more. ListAutoPagingAsync iterates every page.");
}

static string RequiredVariable(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Set {name} to your Sandbox API client's credentials.");
