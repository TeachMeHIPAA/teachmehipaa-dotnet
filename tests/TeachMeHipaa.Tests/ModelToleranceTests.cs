using TeachMeHipaa.Tests.Support;
using static TeachMeHipaa.Tests.Support.Fixtures;

namespace TeachMeHipaa.Tests;

public class ModelToleranceTests
{
    [Fact]
    public async Task KeepsFieldsTheLibraryDoesNotKnowYet()
    {
        var json = LearnerJson.Replace("\"certificate\": null", "\"certificate\": null, \"department\": \"Radiology\"", StringComparison.Ordinal);
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, new ScriptedReply { Json = json });
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal("Radiology", learner.ExtensionData["department"].GetString());
    }

    [Fact]
    public async Task AcceptsALearnerStatusTheLibraryDoesNotKnowYet()
    {
        var json = LearnerJson.Replace("\"in_progress\"", "\"on_leave\"", StringComparison.Ordinal);
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, new ScriptedReply { Json = json });
        var client = NewClient(handler);

        var learner = await client.Learners.GetAsync(LearnerId);

        Assert.Equal("on_leave", learner.Status.Value);
        Assert.Equal(new LearnerStatus("on_leave"), learner.Status);
        Assert.NotEqual(LearnerStatus.InProgress, learner.Status);
    }

    [Fact]
    public async Task ReadsTheLearnersCertificate()
    {
        var json = LearnerJson.Replace(
            "\"certificate\": null",
            """
            "certificate": {
              "certificateId": "cert_01JD8",
              "issuedAt": "2026-11-03T08:00:00.000Z",
              "expiresAt": "2027-11-03T08:00:00.000Z",
              "verifyUrl": "https://verify.teachmehipaa.com/cert_01JD8",
              "pdfUrl": "/v1/organizations/org_acme/certificates/cert_01JD8/pdf"
            }
            """,
            StringComparison.Ordinal);
        var handler = new FakeHttpMessageHandler()
            .Script(TokenRoute, TokenReply())
            .Script(LearnerRoute, new ScriptedReply { Json = json });
        var client = NewClient(handler);

        var certificate = Assert.IsType<LearnerCertificate>((await client.Learners.GetAsync(LearnerId)).Certificate);

        Assert.Equal("cert_01JD8", certificate.CertificateId);
        Assert.Equal(new DateTimeOffset(2026, 11, 3, 8, 0, 0, TimeSpan.Zero), certificate.IssuedAt);
        Assert.Equal(new DateTimeOffset(2027, 11, 3, 8, 0, 0, TimeSpan.Zero), certificate.ExpiresAt);
        Assert.Equal("https://verify.teachmehipaa.com/cert_01JD8", certificate.VerifyUrl);
        Assert.Equal("/v1/organizations/org_acme/certificates/cert_01JD8/pdf", certificate.PdfUrl);
    }
}
