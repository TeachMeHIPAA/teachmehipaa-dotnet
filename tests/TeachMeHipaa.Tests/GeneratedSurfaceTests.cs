using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using TeachMeHipaa.Generator;

namespace TeachMeHipaa.Tests;

/// <summary>
/// Generated surface coverage: every operation in the vendored spec maps to
/// exactly one public method with the expected name, and the committed
/// generated files are what the generator writes from the vendored IR.
/// </summary>
public class GeneratedSurfaceTests
{
    // From api-sdk-surface.md §11 with the grilling's renames (Q12:
    // `retrieveLinks`; the IR: `delete`, not `del`, and `auth.introspect`) and
    // Q25's splits, in Stripe .NET's names: services are the plural resource,
    // `retrieve` is GetAsync, and every method is async.
    private static readonly Dictionary<string, string[]> ExpectedMethods = new()
    {
        ["listAuditEvents"] = ["AuditEvents.ListAsync"],
        ["introspectAccessToken"] = ["Auth.IntrospectAsync"],
        ["getCertificateVerification"] = ["CertificateVerifications.GetAsync"],
        ["downloadCertificatePdf"] = ["Certificates.DownloadPdfAsync"],
        ["exportCertificates"] = ["Certificates.ExportAsync"],
        ["listCertificates"] = ["Certificates.ListAsync"],
        ["getCertificate"] = ["Certificates.GetAsync"],
        ["getCertificateCredentials"] = ["Certificates.RetrieveLinksAsync"],
        ["getComplianceSummary"] = ["Compliance.GetAsync"],
        ["listCourses"] = ["Courses.ListAsync"],
        ["getCourse"] = ["Courses.GetAsync"],
        ["batchEnrollments"] = ["Enrollments.BatchAsync", "Enrollments.BatchByFilterAsync"],
        ["cancelEnrollment"] = ["Enrollments.CancelAsync"],
        ["createEnrollment"] = ["Enrollments.CreateAsync"],
        ["exportEnrollments"] = ["Enrollments.ExportAsync"],
        ["importEnrollments"] = ["Enrollments.ImportAsync", "Enrollments.PreviewImportAsync"],
        ["listEnrollments"] = ["Enrollments.ListAsync"],
        ["remindEnrollment"] = ["Enrollments.RemindAsync"],
        ["renewEnrollment"] = ["Enrollments.RenewAsync"],
        ["resendEnrollmentInvite"] = ["Enrollments.ResendInviteAsync"],
        ["getEnrollment"] = ["Enrollments.GetAsync"],
        ["listOrgEvents"] = ["Events.ListAsync"],
        ["batchUpsertLearners"] = ["Learners.BatchUpsertAsync"],
        ["createLearner"] = ["Learners.CreateAsync"],
        ["createLearnerLaunchLink"] = ["Learners.CreateLaunchLinkAsync"],
        ["exportLearners"] = ["Learners.ExportAsync"],
        ["listLearners"] = ["Learners.ListAsync"],
        ["offboardLearner"] = ["Learners.OffboardAsync"],
        ["remindLearner"] = ["Learners.RemindAsync"],
        ["getLearner"] = ["Learners.GetAsync"],
        ["updateLearner"] = ["Learners.UpdateAsync"],
        ["upsertLearnerByExternalId"] = ["Learners.UpsertByExternalIdAsync"],
        ["listMembers"] = ["Members.ListAsync"],
        ["getMember"] = ["Members.GetAsync"],
        ["getOperation"] = ["Operations.GetAsync"],
        ["getPartner"] = ["Partners.GetAsync"],
        ["createPartnerOrganization"] = ["Partners.Organizations.CreateAsync"],
        ["listPartnerOrganizations"] = ["Partners.Organizations.ListAsync"],
        ["confirmPaymentMethod"] = ["PaymentMethod.ConfirmAsync"],
        ["removePaymentMethod"] = ["PaymentMethod.DeleteAsync"],
        ["preparePaymentMethod"] = ["PaymentMethod.PrepareAsync"],
        ["getPaymentMethod"] = ["PaymentMethod.GetAsync"],
        ["setupPaymentMethod"] = ["PaymentMethod.SetupAsync"],
        ["createSeatOrder"] = ["SeatOrders.CreateAsync"],
        ["listSeatOrders"] = ["SeatOrders.ListAsync"],
        ["quoteSeatOrder"] = ["SeatOrders.QuoteAsync"],
        ["getSeatOrder"] = ["SeatOrders.GetAsync"],
        ["getSeatInventory"] = ["Seats.GetAsync"],
        ["createWebhookSubscription"] = ["WebhookSubscriptions.CreateAsync"],
        ["deleteWebhookSubscription"] = ["WebhookSubscriptions.DeleteAsync"],
        ["listWebhookSubscriptions"] = ["WebhookSubscriptions.ListAsync"],
        ["redriveWebhookSubscription"] = ["WebhookSubscriptions.RedriveAsync"],
        ["getWebhookSubscription"] = ["WebhookSubscriptions.GetAsync"],
        ["testWebhookSubscription"] = ["WebhookSubscriptions.TestAsync"],
        ["updateWebhookSubscription"] = ["WebhookSubscriptions.UpdateAsync"],
    };

    // The token endpoint is the hand-written auth layer, not a service method.
    private static readonly string[] HandWritten = ["createAccessToken"];

    private static readonly string Root = RepoRoot.Find();

    private static string[] ExpectedNames =>
        [.. ExpectedMethods.Values.SelectMany(names => names).Order(StringComparer.Ordinal)];

    [Fact]
    public void TheVendoredIrWasBuiltFromTheVendoredSpec()
    {
        using var ir = Load("sdk-ir.json");
        var source = ir.RootElement.GetProperty("source");
        var spec = File.ReadAllBytes(Path.Combine(Root, "openapi", "openapi.developer.json"));

        Assert.Equal(source.GetProperty("sha256").GetString(), Convert.ToHexString(SHA256.HashData(spec)).ToLowerInvariant());
        Assert.Equal(source.GetProperty("release").GetString(), File.ReadAllText(Path.Combine(Root, "OPENAPI_VERSION")).Trim());
    }

    [Fact]
    public void EverySpecOperationExceptTheTokenEndpointIsExpected()
    {
        using var spec = Load("openapi.developer.json");
        var operationIds = spec.RootElement.GetProperty("paths").EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject())
            .Where(operation => operation.Value.ValueKind == JsonValueKind.Object && operation.Value.TryGetProperty("operationId", out _))
            .Select(operation => operation.Value.GetProperty("operationId").GetString()!)
            .ToHashSet();

        Assert.Equal(56, operationIds.Count);
        Assert.Equal(
            ExpectedMethods.Keys.Order(StringComparer.Ordinal),
            operationIds.Except(HandWritten).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheClientExposesExactlyTheExpectedMethods()
    {
        Assert.Equal(57, ExpectedNames.Length);
        Assert.Equal(ExpectedNames, ServiceMethods(typeof(TeachMeHipaaClient), "").Select(entry => entry.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void EveryIrMethodHasItsExpectedName()
    {
        using var ir = Load("sdk-ir.json");
        var methods = ir.RootElement.GetProperty("resources").EnumerateArray()
            .SelectMany(resource => resource.GetProperty("methods").EnumerateArray().Select(method => (
                OperationId: method.GetProperty("operationId").GetString()!,
                Name: DotNetName(resource.GetProperty("path").EnumerateArray().Select(segment => segment.GetString()!), method.GetProperty("name").GetString()!))))
            .ToList();

        Assert.Equal(57, methods.Select(method => method.Name).Distinct().Count());
        Assert.All(methods, method => Assert.Contains(method.Name, ExpectedMethods[method.OperationId]));
    }

    [Fact]
    public void MethodsAreAsyncAndEndWithRequestOptionsAndACancellationToken()
    {
        Assert.All(ServiceMethods(typeof(TeachMeHipaaClient), ""), entry =>
        {
            var parameters = entry.Method.GetParameters();
            Assert.Equal(typeof(Task<>), entry.Method.ReturnType.GetGenericTypeDefinition());
            Assert.Equal(typeof(RequestOptions), parameters[^2].ParameterType);
            Assert.Equal(typeof(CancellationToken), parameters[^1].ParameterType);
            Assert.True(parameters[^1].IsOptional, entry.Name);
        });
    }

    [Fact]
    public void OrgScopedMethodsTakeNoOrgIdAndPartnerMethodsTakeAPartnerId()
    {
        Assert.All(ServiceMethods(typeof(TeachMeHipaaClient), ""), entry =>
        {
            var names = entry.Method.GetParameters().Select(parameter => parameter.Name).ToList();
            Assert.DoesNotContain("orgId", names);
            Assert.DoesNotContain("organizationId", names);
            if (entry.Name.StartsWith("Partners.", StringComparison.Ordinal))
            {
                Assert.Equal("partnerId", names[0]);
            }
        });
    }

    [Fact]
    public void OptionsClassesAreNamedForTheResourceAndMethod()
    {
        var learners = typeof(LearnerService);

        Assert.Equal(
            ["learnerId", "options", "requestOptions", "cancellationToken"],
            learners.GetMethod(nameof(LearnerService.UpdateAsync))!.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(typeof(LearnerUpdateOptions), learners.GetMethod(nameof(LearnerService.UpdateAsync))!.GetParameters()[1].ParameterType);
        Assert.Equal(typeof(EnrollmentListOptions), typeof(EnrollmentService).GetMethod(nameof(EnrollmentService.ListAsync))!.GetParameters()[0].ParameterType);
        Assert.Equal(
            typeof(PartnerOrganizationListOptions),
            typeof(PartnerOrganizationService).GetMethod(nameof(PartnerOrganizationService.ListAsync))!.GetParameters()[1].ParameterType);
    }

    [Fact]
    public void TheCommittedFilesAreWhatTheGeneratorWrites()
    {
        var fresh = SurfaceGenerator.Generate(
            File.ReadAllText(Path.Combine(Root, "openapi", "sdk-ir.json")),
            File.ReadAllText(Path.Combine(Root, "openapi", "sdk-ir.schema.json")));
        var committed = RepoRoot.GeneratedFiles(Root);

        Assert.Equal(fresh.Keys, committed.Keys);
        Assert.All(fresh, file => Assert.Equal(file.Value, committed[file.Key]));
    }

    [Fact]
    public void ListsReturnAPageWithItemsAndANextCursor()
    {
        var list = typeof(LearnerService).GetMethod(nameof(LearnerService.ListAsync))!;

        Assert.Equal(typeof(Task<Page<Learner>>), list.ReturnType);
    }

    private static string DotNetName(IEnumerable<string> resourcePath, string method)
    {
        var verb = method == "retrieve" ? "Get" : char.ToUpperInvariant(method[0]) + method[1..];
        return string.Join('.', resourcePath.Select(segment => char.ToUpperInvariant(segment[0]) + segment[1..]).Append(verb + "Async"));
    }

    /// <summary>Every public method reachable from a service property, dotted.</summary>
    private static IEnumerable<(string Name, MethodInfo Method)> ServiceMethods(Type type, string prefix)
    {
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.PropertyType.Name.EndsWith("Service", StringComparison.Ordinal)))
        {
            var service = property.PropertyType;
            foreach (var method in service.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName))
            {
                yield return ($"{prefix}{property.Name}.{method.Name}", method);
            }

            foreach (var nested in ServiceMethods(service, $"{prefix}{property.Name}."))
            {
                yield return nested;
            }
        }
    }

    private static JsonDocument Load(string name) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "openapi", name)));
}
