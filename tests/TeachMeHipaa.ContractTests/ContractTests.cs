using System.Net;

namespace TeachMeHipaa.ContractTests;

/// <summary>
/// The public client against a Prism mock of the vendored developer spec: one
/// representative call per resource family. A request that breaks the spec
/// fails the call (Prism answers 4xx) or the test (a violation in
/// <c>sl-violations</c>, checked after every test).
/// </summary>
[Collection(PrismSuite.Name)]
public sealed class ContractTests : IDisposable
{
    // Values that satisfy the spec's path-parameter patterns.
    private const string OrgId = "org_acme";
    private const string LearnerId = "sub_01JD80AB2C3D4E5F6G7H8J9K0M";
    private const string EnrollmentId = "sub_01JD80AB2C3D4E5F6G7H8J9K0M__hipaa-2026";
    private const string CertificateId = "cert_01JD8F";
    private const string CourseId = "hipaa-2026";
    private const string SubscriptionId = "whs_01JD8D";
    private const string OperationId = "op-01JD8H";
    private const string PartnerId = "ptn_01JD8E2C3D4E5F6G7H8J9K0M1N";
    private const string TinyId = "cAbCdEfGhIjKl";

    private readonly ViolationRecorder _violations = new();
    private readonly TeachMeHipaaClient _client;

    public ContractTests(PrismMock prism)
    {
        // Prism can't mint tokens from the form-encoded /auth/token, so the
        // client sends a static one and skips Organization discovery.
        _client = new TeachMeHipaaClient(new TeachMeHipaaClientOptions
        {
            TokenProvider = new StaticTokenProvider("contract-test-token"),
            OrganizationId = OrgId,
            BaseUrl = prism.BaseUrl,
            MaxRetries = 0,
            HttpMessageHandler = _violations,
        });
    }

    public void Dispose()
    {
        try
        {
            Assert.True(_violations.Found.Count == 0, $"Prism reported request violations:\n{string.Join('\n', _violations.Found)}");
        }
        finally
        {
            _violations.Dispose();
        }
    }

    [Fact]
    public async Task TheHarnessCatchesARequestThatBreaksTheSpec()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(() => _client.SeatOrders.QuoteAsync(new SeatOrderQuoteOptions { Quantity = 0 }));

        var violation = Assert.Single(_violations.Found);
        Assert.Contains("quantity", violation, StringComparison.Ordinal);
        _violations.Clear();
    }

    [Fact]
    public async Task ListsCoursesAsAPage()
    {
        var page = await _client.Courses.ListAsync(new CourseListOptions { Limit = 5 });

        Assert.Equal(HttpStatusCode.OK, page.LastResponse!.StatusCode);
        Assert.All(page.Items, course => Assert.False(string.IsNullOrEmpty(course.CourseId)));
    }

    [Fact]
    public async Task RetrievesACourse()
    {
        var course = await _client.Courses.GetAsync(CourseId);

        Assert.False(string.IsNullOrEmpty(course.Title));
    }

    [Fact]
    public async Task RetrievesACertificateVerification()
    {
        var verification = await _client.CertificateVerifications.GetAsync(TinyId);

        Assert.False(string.IsNullOrEmpty(verification.LearnerName));
    }

    [Fact]
    public async Task CreatesALearnerAndReportsWhetherItWasCreated()
    {
        var result = await _client.Learners.CreateAsync(new LearnerCreateOptions
        {
            Email = "ana@example.com",
            Name = "Ana Example",
            ExternalId = "hr-123",
            CourseId = CourseId,
            Delivery = LearnerCreateRequestDelivery.None,
        });

        Assert.False(string.IsNullOrEmpty(result.LearnerId));
        Assert.Equal(result.LastResponse!.StatusCode == HttpStatusCode.Created, result.Created);
    }

    [Fact]
    public async Task ListsLearnersWithFilters()
    {
        var page = await _client.Learners.ListAsync(new LearnerListOptions
        {
            Limit = 10,
            Status = LearnerStatus.InProgress,
            Email = "ana@example.com",
        });

        Assert.All(page.Items, learner => Assert.False(string.IsNullOrEmpty(learner.Email)));
    }

    [Fact]
    public async Task RetrievesALearner()
    {
        var learner = await _client.Learners.GetAsync(LearnerId, new LearnerGetOptions { CourseId = CourseId });

        Assert.False(string.IsNullOrEmpty(learner.LearnerId));
        Assert.False(string.IsNullOrEmpty(learner.Course.CourseId));
    }

    [Fact]
    public async Task UpdatesALearnerAsAMergePatchWithIfMatch()
    {
        var learner = await _client.Learners.UpdateAsync(LearnerId, new LearnerUpdateOptions("\"7\"")
        {
            Name = "Ana Q. Example",
            ExternalId = null,
        });

        Assert.False(string.IsNullOrEmpty(learner.LearnerId));
    }

    [Fact]
    public async Task CancelsAnEnrollment()
    {
        var result = await _client.Enrollments.CancelAsync(EnrollmentId, new EnrollmentCancelOptions { Refund = false });

        Assert.Equal(HttpStatusCode.OK, result.LastResponse!.StatusCode);
    }

    [Fact]
    public async Task RetrievesACertificate()
    {
        var certificate = await _client.Certificates.GetAsync(CertificateId);

        Assert.False(string.IsNullOrEmpty(certificate.CertificateId));
    }

    [Fact]
    public async Task DownloadsACertificatePdf()
    {
        var pdf = await _client.Certificates.DownloadPdfAsync(CertificateId);

        Assert.Equal("application/pdf", pdf.ContentType);
        Assert.NotNull(pdf.Data);
    }

    [Fact]
    public async Task StreamsACertificatePdf()
    {
        await using var pdf = await _client.Certificates.DownloadPdfStreamingAsync(CertificateId);
        using var copy = new MemoryStream();
        await pdf.Content.CopyToAsync(copy);

        Assert.Equal("application/pdf", pdf.ContentType);
        Assert.Equal(HttpStatusCode.OK, pdf.LastResponse.StatusCode);
    }

    [Fact]
    public async Task QuotesASeatOrder()
    {
        var quote = await _client.SeatOrders.QuoteAsync(new SeatOrderQuoteOptions { Quantity = 10, Currency = "usd" });

        Assert.Equal(HttpStatusCode.OK, quote.LastResponse!.StatusCode);
    }

    [Fact]
    public async Task CreatesAWebhookSubscriptionWithItsSigningSecret()
    {
        var subscription = await _client.WebhookSubscriptions.CreateAsync(new WebhookSubscriptionCreateOptions
        {
            EndpointUrl = "https://hr.example.com/hooks/tmh",
            EventTypes = [EventType.LearnerCertified, EventType.SeatsLow],
            Description = "HR sync",
            SeatsLowThreshold = 5,
        });

        Assert.False(string.IsNullOrEmpty(subscription.SubscriptionId));
        Assert.False(string.IsNullOrEmpty(subscription.Secret));
    }

    [Fact]
    public async Task DeletesAWebhookSubscription()
    {
        var response = await _client.WebhookSubscriptions.DeleteAsync(SubscriptionId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ListsTheEventLog()
    {
        var page = await _client.Events.ListAsync(new EventListOptions { Limit = 25 });

        Assert.All(page.Items, @event => Assert.False(string.IsNullOrEmpty(@event.EventId)));
    }

    [Fact]
    public async Task RetrievesAnOperation()
    {
        var operation = await _client.Operations.GetAsync(OperationId);

        Assert.False(string.IsNullOrEmpty(operation.Name));
    }

    [Fact]
    public async Task IntrospectsTheAccessToken()
    {
        var introspection = await _client.Auth.IntrospectAsync();

        Assert.Equal(HttpStatusCode.OK, introspection.LastResponse!.StatusCode);
    }

    [Fact]
    public async Task ListsAPartnersCustomerOrganizations()
    {
        var page = await _client.Partners.Organizations.ListAsync(PartnerId, new PartnerOrganizationListOptions { Limit = 10 });

        Assert.All(page.Items, organization => Assert.False(string.IsNullOrEmpty(organization.OrgId)));
    }
}
