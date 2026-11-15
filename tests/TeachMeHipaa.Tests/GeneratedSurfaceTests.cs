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
    // `retrieve` is GetAsync, and every method is async. Each list also has
    // Stripe .NET's `ListAutoPagingAsync` sibling, iterating every page.
    private static readonly Dictionary<string, string[]> ExpectedMethods = new()
    {
        ["listAuditEvents"] = ["AuditEvents.ListAsync", "AuditEvents.ListAutoPagingAsync"],
        ["introspectAccessToken"] = ["Auth.IntrospectAsync"],
        ["getCertificateVerification"] = ["CertificateVerifications.GetAsync"],
        ["downloadCertificatePdf"] = ["Certificates.DownloadPdfAsync"],
        ["exportCertificates"] = ["Certificates.ExportAsync"],
        ["listCertificates"] = ["Certificates.ListAsync", "Certificates.ListAutoPagingAsync"],
        ["getCertificate"] = ["Certificates.GetAsync"],
        ["getCertificateCredentials"] = ["Certificates.RetrieveLinksAsync"],
        ["getComplianceSummary"] = ["Compliance.GetAsync"],
        ["listCourses"] = ["Courses.ListAsync", "Courses.ListAutoPagingAsync"],
        ["getCourse"] = ["Courses.GetAsync"],
        ["batchEnrollments"] = ["Enrollments.BatchAsync", "Enrollments.BatchByFilterAsync"],
        ["cancelEnrollment"] = ["Enrollments.CancelAsync"],
        ["createEnrollment"] = ["Enrollments.CreateAsync"],
        ["exportEnrollments"] = ["Enrollments.ExportAsync"],
        ["importEnrollments"] = ["Enrollments.ImportAsync", "Enrollments.PreviewImportAsync"],
        ["listEnrollments"] = ["Enrollments.ListAsync", "Enrollments.ListAutoPagingAsync"],
        ["remindEnrollment"] = ["Enrollments.RemindAsync"],
        ["renewEnrollment"] = ["Enrollments.RenewAsync"],
        ["resendEnrollmentInvite"] = ["Enrollments.ResendInviteAsync"],
        ["getEnrollment"] = ["Enrollments.GetAsync"],
        ["listOrgEvents"] = ["Events.ListAsync", "Events.ListAutoPagingAsync"],
        ["batchUpsertLearners"] = ["Learners.BatchUpsertAsync"],
        ["createLearner"] = ["Learners.CreateAsync"],
        ["createLearnerLaunchLink"] = ["Learners.CreateLaunchLinkAsync"],
        ["exportLearners"] = ["Learners.ExportAsync"],
        ["listLearners"] = ["Learners.ListAsync", "Learners.ListAutoPagingAsync"],
        ["offboardLearner"] = ["Learners.OffboardAsync"],
        ["remindLearner"] = ["Learners.RemindAsync"],
        ["getLearner"] = ["Learners.GetAsync"],
        ["updateLearner"] = ["Learners.UpdateAsync"],
        ["upsertLearnerByExternalId"] = ["Learners.UpsertByExternalIdAsync"],
        ["listMembers"] = ["Members.ListAsync", "Members.ListAutoPagingAsync"],
        ["getMember"] = ["Members.GetAsync"],
        ["getOperation"] = ["Operations.GetAsync"],
        ["getPartner"] = ["Partners.GetAsync"],
        ["createPartnerOrganization"] = ["Partners.Organizations.CreateAsync"],
        ["listPartnerOrganizations"] = ["Partners.Organizations.ListAsync", "Partners.Organizations.ListAutoPagingAsync"],
        ["confirmPaymentMethod"] = ["PaymentMethod.ConfirmAsync"],
        ["removePaymentMethod"] = ["PaymentMethod.DeleteAsync"],
        ["preparePaymentMethod"] = ["PaymentMethod.PrepareAsync"],
        ["getPaymentMethod"] = ["PaymentMethod.GetAsync"],
        ["setupPaymentMethod"] = ["PaymentMethod.SetupAsync"],
        ["createSeatOrder"] = ["SeatOrders.CreateAsync"],
        ["listSeatOrders"] = ["SeatOrders.ListAsync", "SeatOrders.ListAutoPagingAsync"],
        ["quoteSeatOrder"] = ["SeatOrders.QuoteAsync"],
        ["getSeatOrder"] = ["SeatOrders.GetAsync"],
        ["getSeatInventory"] = ["Seats.GetAsync"],
        ["createWebhookSubscription"] = ["WebhookSubscriptions.CreateAsync"],
        ["deleteWebhookSubscription"] = ["WebhookSubscriptions.DeleteAsync"],
        ["listWebhookSubscriptions"] = ["WebhookSubscriptions.ListAsync", "WebhookSubscriptions.ListAutoPagingAsync"],
        ["redriveWebhookSubscription"] = ["WebhookSubscriptions.RedriveAsync"],
        ["getWebhookSubscription"] = ["WebhookSubscriptions.GetAsync"],
        ["testWebhookSubscription"] = ["WebhookSubscriptions.TestAsync"],
        ["updateWebhookSubscription"] = ["WebhookSubscriptions.UpdateAsync"],
    };

    // The token endpoint is the hand-written auth layer, not a service method.
    private static readonly string[] HandWritten = ["createAccessToken"];

    // Hand-written methods on generated services (src/TeachMeHipaa/Extensions),
    // as Node's and Python's extensions add them.
    private static readonly string[] Extensions = ["Events.TailAsync"];

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
        Assert.Equal(67, ExpectedNames.Length);
        Assert.Equal(
            ExpectedNames.Concat(Extensions).Order(StringComparer.Ordinal),
            ServiceMethods(typeof(TeachMeHipaaClient), "").Select(entry => entry.Name).Order(StringComparer.Ordinal));
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
            var returns = entry.Name switch
            {
                _ when entry.Name.EndsWith("AutoPagingAsync", StringComparison.Ordinal) => typeof(IAsyncEnumerable<>),
                "Events.TailAsync" => typeof(EventTail),
                _ => typeof(Task<>),
            };
            Assert.Equal(returns, entry.Method.ReturnType.IsGenericType ? entry.Method.ReturnType.GetGenericTypeDefinition() : entry.Method.ReturnType);
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

    [Fact]
    public void EveryListHasAnAutoPagingSiblingWithTheSameParameters()
    {
        var methods = ServiceMethods(typeof(TeachMeHipaaClient), "").ToDictionary(entry => entry.Name, entry => entry.Method);
        var lists = methods.Where(entry => entry.Value.ReturnType.IsGenericType
            && entry.Value.ReturnType.GetGenericArguments()[0] is { IsGenericType: true } page
            && page.GetGenericTypeDefinition() == typeof(Page<>)).ToList();

        Assert.Equal(10, lists.Count);
        Assert.All(lists, list =>
        {
            var sibling = methods[list.Key.Replace("ListAsync", "ListAutoPagingAsync", StringComparison.Ordinal)];
            var item = list.Value.ReturnType.GetGenericArguments()[0].GetGenericArguments()[0];
            Assert.Equal(typeof(IAsyncEnumerable<>).MakeGenericType(item), sibling.ReturnType);
            Assert.Equal(
                list.Value.GetParameters().Select(parameter => (parameter.Name, parameter.ParameterType, parameter.IsOptional)),
                sibling.GetParameters().Select(parameter => (parameter.Name, parameter.ParameterType, parameter.IsOptional)));
        });
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
