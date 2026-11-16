// Syncs a roster CSV into the Sandbox, one upsert per row keyed by your own
// id for each person (their External ID). Running it again changes nothing
// and invites nobody twice, so it is safe to run on a schedule.
//
//   TMH_CLIENT_ID=cli_... TMH_CLIENT_SECRET=tmh_cs_... \
//     dotnet run --project examples/SyncRoster -- roster.csv hipaa-2026
//
// roster.csv has a header row and the columns externalId,email,name. This
// reader splits on commas, so it doesn't handle quoted fields; use a CSV
// library for real exports.
using TeachMeHipaa;

if (args.Length != 2)
{
    Console.Error.WriteLine("usage: SyncRoster <roster.csv> <courseId>");
    return 2;
}

var (rosterPath, courseId) = (args[0], args[1]);
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, keyPress) =>
{
    keyPress.Cancel = true;
    cancellation.Cancel();
};

var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
{
    ClientId = RequiredVariable("TMH_CLIENT_ID"),
    ClientSecret = RequiredVariable("TMH_CLIENT_SECRET"),
    Environment = TeachMeHipaaEnvironment.Sandbox,
    AppInfo = new AppInfo("teachmehipaa-dotnet-example-sync-roster"),
});

var rows = File.ReadLines(rosterPath)
    .Skip(1)
    .Where(line => line.Trim().Length > 0)
    .Select(line => line.Split(','))
    .ToList();

var (invited, existing, failed) = (0, 0, 0);
foreach (var row in rows)
{
    var (externalId, email, name) = (row[0].Trim(), row[1].Trim(), row.Length > 2 ? row[2].Trim() : null);
    try
    {
        var learner = await client.Learners.UpsertByExternalIdAsync(
            externalId,
            new LearnerUpsertByExternalIdOptions { Email = email, Name = name, CourseId = courseId },
            cancellationToken: cancellation.Token);
        if (learner.Created)
        {
            invited++;
        }
        else
        {
            existing++;
        }
    }
    catch (TeachMeHipaaException error)
    {
        // One bad row shouldn't stop the sync; report it with the request id support needs.
        failed++;
        Console.Error.WriteLine($"{externalId}: {error.Code} {error.Detail} (request id {error.RequestId})");
    }
}

Console.WriteLine($"{invited} invited, {existing} already enrolled, {failed} failed");
return failed == 0 ? 0 : 1;

static string RequiredVariable(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Set {name} to your Sandbox API client's credentials.");
