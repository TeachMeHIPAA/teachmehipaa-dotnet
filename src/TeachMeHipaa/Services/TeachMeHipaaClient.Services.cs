// Generated from openapi/sdk-ir.json by tools/Generator. Do not edit.
// Regenerate with `dotnet run --project tools/Generator`.

namespace TeachMeHipaa;

public sealed partial class TeachMeHipaaClient
{
    private TeachMeHipaaClient(ApiRequestor requestor)
    {
        AuditEvents = new AuditEventService(requestor);
        Auth = new AuthService(requestor);
        CertificateVerifications = new CertificateVerificationService(requestor);
        Certificates = new CertificateService(requestor);
        Compliance = new ComplianceService(requestor);
        Courses = new CourseService(requestor);
        Enrollments = new EnrollmentService(requestor);
        Events = new EventService(requestor);
        Learners = new LearnerService(requestor);
        Members = new MemberService(requestor);
        Operations = new OperationService(requestor);
        Partners = new PartnerService(requestor);
        PaymentMethod = new PaymentMethodService(requestor);
        SeatOrders = new SeatOrderService(requestor);
        Seats = new SeatService(requestor);
        WebhookSubscriptions = new WebhookSubscriptionService(requestor);
    }

    /// <summary>The Developer API's Audit events endpoints.</summary>
    public AuditEventService AuditEvents { get; }

    /// <summary>The Developer API's Auth endpoints.</summary>
    public AuthService Auth { get; }

    /// <summary>The Developer API's Certificate verifications endpoints.</summary>
    public CertificateVerificationService CertificateVerifications { get; }

    /// <summary>The Developer API's Certificates endpoints.</summary>
    public CertificateService Certificates { get; }

    /// <summary>The Developer API's Compliance endpoints.</summary>
    public ComplianceService Compliance { get; }

    /// <summary>The Developer API's Courses endpoints.</summary>
    public CourseService Courses { get; }

    /// <summary>The Developer API's Enrollments endpoints.</summary>
    public EnrollmentService Enrollments { get; }

    /// <summary>The Developer API's Events endpoints.</summary>
    public EventService Events { get; }

    /// <summary>The Developer API's Learners endpoints.</summary>
    public LearnerService Learners { get; }

    /// <summary>The Developer API's Members endpoints.</summary>
    public MemberService Members { get; }

    /// <summary>The Developer API's Operations endpoints.</summary>
    public OperationService Operations { get; }

    /// <summary>The Developer API's Partners endpoints.</summary>
    public PartnerService Partners { get; }

    /// <summary>The Developer API's Payment method endpoints.</summary>
    public PaymentMethodService PaymentMethod { get; }

    /// <summary>The Developer API's Seat orders endpoints.</summary>
    public SeatOrderService SeatOrders { get; }

    /// <summary>The Developer API's Seats endpoints.</summary>
    public SeatService Seats { get; }

    /// <summary>The Developer API's Webhook subscriptions endpoints.</summary>
    public WebhookSubscriptionService WebhookSubscriptions { get; }
}
