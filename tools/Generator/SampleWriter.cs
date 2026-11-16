using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TeachMeHipaa.Generator;

/// <summary>
/// Writes <c>docs/reference/code-samples.json</c>: one complete C# program per
/// IR operation, for the C# tab of the API reference. The client
/// construction comes from the method's security; the call and the line that
/// uses its result come from <see cref="Calls"/>. The README test project
/// compiles every program against the library.
/// </summary>
internal sealed class SampleWriter(IrDocument ir, ServiceWriter services)
{
    public const string Path = "docs/reference/code-samples.json";

    /// <summary>
    /// Ids and values every SDK's samples share, so the six languages line up
    /// in the reference's language picker. A path parameter with an entry here
    /// must appear in its sample, quoted.
    /// </summary>
    internal static readonly Dictionary<string, string> PathValues = new(StringComparer.Ordinal)
    {
        ["learnerId"] = "sub_01JD80AB2C3D4E5F6G7H8J9K0M",
        ["subjectId"] = "sub_01JD80AB2C3D4E5F6G7H8J9K0M",
        ["enrollmentId"] = "enr_01JD8K2M4N6P8Q0R2S4T6V8W0X",
        ["certId"] = "cert_01JD8F3G5H7J9K1M3N5P7Q9R1S",
        ["courseId"] = "hipaa-2026",
        ["externalId"] = "hr-1042",
        ["operationId"] = "op_01JD8T4V6W8X0Y2Z4A6B8C0D2E",
        ["subscriptionId"] = "whs_01JD8Q5R7S9T1V3W5X7Y9Z1A3B",
        ["tinyId"] = "a7Kq2Z",
    };

    private const string CredentialsClient = """
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            ClientId = Environment.GetEnvironmentVariable("TMH_CLIENT_ID")!,
            ClientSecret = Environment.GetEnvironmentVariable("TMH_CLIENT_SECRET")!,
            Environment = TeachMeHipaaEnvironment.Sandbox,
        });
        """;

    // Public methods send no Access token, so their client needs no credentials.
    private const string PublicClient = """
        var client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            Environment = TeachMeHipaaEnvironment.Sandbox,
        });
        """;

    /// <summary>
    /// Per operationId: the call, with required arguments only plus at most
    /// one telling optional, and one line that uses the result. Where two
    /// methods share an operationId, the sample shows the first.
    /// </summary>
    internal static readonly Dictionary<string, string> Calls = new(StringComparer.Ordinal)
    {
        ["listAuditEvents"] = """
            await foreach (var auditEvent in client.AuditEvents.ListAutoPagingAsync())
            {
                if (auditEvent is AuditEventEnrollmentTransition transition)
                {
                    Console.WriteLine($"{transition.SubjectId}: {transition.StatusFrom} -> {transition.StatusTo}");
                }
            }
            """,
        ["introspectAccessToken"] = """
            TokenIntrospection token = await client.Auth.IntrospectAsync();
            Console.WriteLine($"{token.Mode} token for {token.OrgId}, expires {token.ExpiresAt:u}");
            """,
        ["getCertificateVerification"] = """
            CertificateVerification verification = await client.CertificateVerifications.GetAsync("a7Kq2Z");
            Console.WriteLine($"{verification.Status}: {verification.LearnerName}, {verification.CourseTitle}");
            """,
        ["downloadCertificatePdf"] = """
            BinaryResponse pdf = await client.Certificates.DownloadPdfAsync("cert_01JD8F3G5H7J9K1M3N5P7Q9R1S");
            await File.WriteAllBytesAsync(pdf.FileName ?? "certificate.pdf", pdf.Data);
            """,
        ["exportCertificates"] = """
            Operation operation = await client.Certificates.ExportAsync(
                new CertificateExportOptions { Format = CertificatesExportRequestFormat.Csv });
            ExportResult export = (await operation.WaitAsync()).GetExportResult();
            await File.WriteAllTextAsync("certificates.csv", export.Csv);
            """,
        ["listCertificates"] = """
            await foreach (var certificate in client.Certificates.ListAutoPagingAsync())
            {
                Console.WriteLine($"{certificate.CertificateId} {certificate.Status}, expires {certificate.ExpiresAt}");
            }
            """,
        ["getCertificate"] = """
            Certificate certificate = await client.Certificates.GetAsync("cert_01JD8F3G5H7J9K1M3N5P7Q9R1S");
            Console.WriteLine($"{certificate.Status}, expires {certificate.ExpiresAt}");
            """,
        ["getCertificateCredentials"] = """
            CertificateLinks links = await client.Certificates.RetrieveLinksAsync("cert_01JD8F3G5H7J9K1M3N5P7Q9R1S");
            Console.WriteLine(links.ShortLink);
            """,
        ["getComplianceSummary"] = """
            ComplianceSummary summary = await client.Compliance.GetAsync(new() { CourseId = "hipaa-2026" });
            Console.WriteLine($"{summary.CompliantRate:P0} of {summary.Totals.Learners} Learners are compliant");
            """,
        ["listCourses"] = """
            await foreach (var course in client.Courses.ListAutoPagingAsync())
            {
                Console.WriteLine($"{course.CourseId}: {course.Title}");
            }
            """,
        ["getCourse"] = """
            Course course = await client.Courses.GetAsync("hipaa-2026");
            Console.WriteLine($"{course.Title}, passing score {course.PassingThreshold:P0}");
            """,
        ["batchEnrollments"] = """
            EnrollmentsBatchResults batch = await client.Enrollments.BatchAsync(new EnrollmentBatchOptions
            {
                Operation = EnrollmentsBatchRequestOperation.Remind,
                Select = new() { EnrollmentIds = ["enr_01JD8K2M4N6P8Q0R2S4T6V8W0X"] },
            });
            Console.WriteLine(string.Join(", ", batch.Results.Select(result => $"{result.EnrollmentId}: {result.Outcome}")));
            """,
        ["cancelEnrollment"] = """
            CancelEnrollmentResponse cancelled = await client.Enrollments.CancelAsync("enr_01JD8K2M4N6P8Q0R2S4T6V8W0X");
            Console.WriteLine($"Cancelled {cancelled.EnrollmentId}");
            """,
        ["createEnrollment"] = """
            Enrollment enrollment = await client.Enrollments.CreateAsync(new EnrollmentCreateOptions
            {
                Email = "dana.whitfield@example.com",
                CourseId = "hipaa-2026",
            });
            Console.WriteLine($"{enrollment.EnrollmentId} {enrollment.Status}");
            """,
        ["exportEnrollments"] = """
            Operation operation = await client.Enrollments.ExportAsync(
                new EnrollmentExportOptions { Format = EnrollmentsExportRequestFormat.Csv });
            ExportResult export = (await operation.WaitAsync()).GetExportResult();
            await File.WriteAllTextAsync("enrollments.csv", export.Csv);
            """,
        ["importEnrollments"] = """
            Operation operation = await client.Enrollments.ImportAsync(new EnrollmentImportOptions("roster.csv"));
            Operation done = await operation.WaitAsync();
            Console.WriteLine($"Imported roster.csv: {done.Name}");
            """,
        ["listEnrollments"] = """
            var expiring = new EnrollmentListOptions { Status = ListEnrollmentsStatus.Expiring };
            await foreach (var enrollment in client.Enrollments.ListAutoPagingAsync(expiring))
            {
                Console.WriteLine($"{enrollment.EnrollmentId} expires {enrollment.ExpiresAt}");
            }
            """,
        ["getEnrollment"] = """
            Enrollment enrollment = await client.Enrollments.GetAsync("enr_01JD8K2M4N6P8Q0R2S4T6V8W0X");
            Console.WriteLine($"{enrollment.EffectiveStatus}, certified {enrollment.CertifiedAt}");
            """,
        ["remindEnrollment"] = """
            EnrollmentActionAccepted accepted = await client.Enrollments.RemindAsync("enr_01JD8K2M4N6P8Q0R2S4T6V8W0X");
            Console.WriteLine($"{accepted.Action} accepted for {accepted.EnrollmentId}");
            """,
        ["renewEnrollment"] = """
            Enrollment renewed = await client.Enrollments.RenewAsync("enr_01JD8K2M4N6P8Q0R2S4T6V8W0X");
            Console.WriteLine($"Cycle {renewed.Cycle}: {renewed.Status}");
            """,
        ["resendEnrollmentInvite"] = """
            EnrollmentActionAccepted accepted = await client.Enrollments.ResendInviteAsync("enr_01JD8K2M4N6P8Q0R2S4T6V8W0X");
            Console.WriteLine($"{accepted.Action} accepted for {accepted.EnrollmentId}");
            """,
        ["listOrgEvents"] = """
            await foreach (var @event in client.Events.ListAutoPagingAsync(new() { Type = EventType.LearnerCertified }))
            {
                Console.WriteLine($"{@event.EventId} at {@event.OccurredAt:u}");
            }
            """,
        ["batchUpsertLearners"] = """
            Operation operation = await client.Learners.BatchUpsertAsync(new LearnerBatchUpsertOptions
            {
                Items = [new() { Email = "dana.whitfield@example.com", ExternalId = "hr-1042" }],
            });
            Operation done = await operation.WaitAsync();
            Console.WriteLine($"Batch finished: {done.Name}");
            """,
        ["createLearner"] = """
            LearnerWriteResult learner = await client.Learners.CreateAsync(new LearnerCreateOptions
            {
                Email = "dana.whitfield@example.com",
                Name = "Dana Whitfield",
            });
            Console.WriteLine($"{learner.LearnerId} created: {learner.Created}");
            """,
        ["createLearnerLaunchLink"] = """
            LearnerLaunchLink link = await client.Learners.CreateLaunchLinkAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
            Console.WriteLine($"{link.Url} (expires {link.ExpiresAt:u})");
            """,
        ["exportLearners"] = """
            Operation operation = await client.Learners.ExportAsync(
                new LearnerExportOptions { Format = LearnersExportRequestFormat.Csv });
            ExportResult export = (await operation.WaitAsync()).GetExportResult();
            await File.WriteAllTextAsync("learners.csv", export.Csv);
            """,
        ["listLearners"] = """
            await foreach (var learner in client.Learners.ListAutoPagingAsync(new() { Status = LearnerStatus.InProgress }))
            {
                Console.WriteLine($"{learner.Name} <{learner.Email}>");
            }
            """,
        ["offboardLearner"] = """
            Learner learner = await client.Learners.OffboardAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
            Console.WriteLine($"{learner.Email} is now {learner.Status}");
            """,
        ["remindLearner"] = """
            LearnerRemindAccepted reminder = await client.Learners.RemindAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
            Console.WriteLine($"{reminder.Action} accepted for {reminder.LearnerId}");
            """,
        ["getLearner"] = """
            Learner learner = await client.Learners.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
            Console.WriteLine($"{learner.Name} {learner.Status}");
            """,
        ["updateLearner"] = """
            Learner learner = await client.Learners.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
            Learner updated = await client.Learners.UpdateAsync(
                learner.LearnerId,
                new LearnerUpdateOptions(learner) { Name = "Dana Whitfield" });
            Console.WriteLine(updated.Name);
            """,
        ["upsertLearnerByExternalId"] = """
            LearnerWriteResult learner = await client.Learners.UpsertByExternalIdAsync(
                "hr-1042",
                new LearnerUpsertByExternalIdOptions { Email = "dana.whitfield@example.com" });
            Console.WriteLine($"{learner.LearnerId} created: {learner.Created}");
            """,
        ["listMembers"] = """
            await foreach (var member in client.Members.ListAutoPagingAsync())
            {
                Console.WriteLine($"{member.Email} {member.Role}");
            }
            """,
        ["getMember"] = """
            Member member = await client.Members.GetAsync("sub_01JD80AB2C3D4E5F6G7H8J9K0M");
            Console.WriteLine($"{member.Name} {member.Role}");
            """,
        ["getOperation"] = """
            Operation operation = await client.Operations.GetAsync("op_01JD8T4V6W8X0Y2Z4A6B8C0D2E");
            Console.WriteLine(operation.Done ? "Done" : "Still running");
            """,
        ["getSeatInventory"] = """
            SeatsInventory seats = await client.Seats.GetAsync();
            Console.WriteLine($"{seats.Available} of {seats.Total} Seats available");
            """,
        ["createWebhookSubscription"] = """
            var subscription = await client.WebhookSubscriptions.CreateAsync(new WebhookSubscriptionCreateOptions
            {
                EndpointUrl = "https://example.com/webhooks/teachmehipaa",
                EventTypes = [EventType.LearnerCertified],
            });
            Console.WriteLine($"{subscription.SubscriptionId}: store Secret now, it is returned only once");
            """,
        ["deleteWebhookSubscription"] = """
            TeachMeHipaaResponse response = await client.WebhookSubscriptions.DeleteAsync("whs_01JD8Q5R7S9T1V3W5X7Y9Z1A3B");
            Console.WriteLine($"Deleted: {response.StatusCode}");
            """,
        ["listWebhookSubscriptions"] = """
            await foreach (var subscription in client.WebhookSubscriptions.ListAutoPagingAsync())
            {
                Console.WriteLine($"{subscription.SubscriptionId} {subscription.EndpointUrl} enabled={subscription.Enabled}");
            }
            """,
        ["redriveWebhookSubscription"] = """
            Operation operation = await client.WebhookSubscriptions.RedriveAsync("whs_01JD8Q5R7S9T1V3W5X7Y9Z1A3B");
            WebhookDeliveryResult result = (await operation.WaitAsync()).GetDeliveryResult();
            Console.WriteLine(result);
            """,
        ["getWebhookSubscription"] = """
            WebhookSubscription subscription = await client.WebhookSubscriptions.GetAsync("whs_01JD8Q5R7S9T1V3W5X7Y9Z1A3B");
            Console.WriteLine($"{subscription.EndpointUrl}: {string.Join(", ", subscription.EventTypes)}");
            """,
        ["testWebhookSubscription"] = """
            Operation operation = await client.WebhookSubscriptions.TestAsync("whs_01JD8Q5R7S9T1V3W5X7Y9Z1A3B");
            WebhookDeliveryResult result = (await operation.WaitAsync()).GetDeliveryResult();
            Console.WriteLine(result is WebhookDeliverySent sent ? $"Endpoint answered {sent.ResponseStatus}" : $"{result}");
            """,
        ["updateWebhookSubscription"] = """
            WebhookSubscription current = await client.WebhookSubscriptions.GetAsync("whs_01JD8Q5R7S9T1V3W5X7Y9Z1A3B");
            WebhookSubscription updated = await client.WebhookSubscriptions.UpdateAsync(
                current.SubscriptionId,
                new WebhookSubscriptionUpdateOptions(current) { Enabled = false });
            Console.WriteLine($"Enabled: {updated.Enabled}");
            """,
    };

    public string Write()
    {
        var methods = services.Plan.Roots.Values
            .SelectMany(root => root.Walk())
            .SelectMany(node => node.Methods.Select(method => (Node: node, Method: method)))
            .ToList();
        var operations = ir.Resources.SelectMany(resource => resource.Methods).Select(method => method.OperationId).Distinct().ToList();
        if (Calls.Keys.FirstOrDefault(operation => !operations.Contains(operation)) is { } extra)
        {
            throw new GeneratorException($"SampleWriter.Calls has {extra}, which the IR no longer has");
        }

        var samples = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var operation in operations)
        {
            // Two methods can share an operationId (batch and batchByFilter);
            // the IR's first is the sample.
            var (node, plan) = methods.First(entry => entry.Method.Method.OperationId == operation);
            var call = Calls.GetValueOrDefault(operation)
                ?? throw new GeneratorException($"{operation} has no sample in SampleWriter.Calls");
            var target = $"client.{string.Join(".", node.Path.Select(Naming.Pascal))}.{plan.Name[..^"Async".Length]}";
            if (!call.Contains(target, StringComparison.Ordinal))
            {
                throw new GeneratorException($"the {operation} sample doesn't call {target}");
            }

            foreach (var parameter in plan.Method.PathParams)
            {
                var value = PathValues.GetValueOrDefault(parameter.Name)
                    ?? throw new GeneratorException($"{operation}: no shared sample value for {parameter.Name}");
                if (!call.Contains($"\"{value}\"", StringComparison.Ordinal))
                {
                    throw new GeneratorException($"the {operation} sample doesn't pass \"{value}\" for {parameter.Name}");
                }
            }

            var client = plan.Method.Security.Kind == IrSecurityKind.None ? PublicClient : CredentialsClient;
            samples.Add(operation, $"using TeachMeHipaa;\n\n{client}\n\n{call}\n");
        }

        return Json(samples);
    }

    private string Json(SortedDictionary<string, string> samples)
    {
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            json.WriteStartObject();
            json.WriteString("language", "csharp");
            json.WriteString("release", ir.Source.Release);
            json.WriteStartObject("samples");
            foreach (var (operation, source) in samples)
            {
                json.WriteString(operation, source);
            }

            json.WriteEndObject();
            json.WriteEndObject();
        }

        // Utf8JsonWriter ends lines with Environment.NewLine; strings escape
        // theirs, so this only touches the indentation.
        return Encoding.UTF8.GetString(buffer.ToArray()).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    }
}
