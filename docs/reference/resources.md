<!-- Generated from openapi/sdk-ir.json by tools/Generator. Do not edit. -->

# TeachMeHIPAA resource reference

Each service on `TeachMeHipaaClient` and each of its methods: the C# signature, the HTTP call, the Scopes the Access token needs and how the client retries the call. Generated from the SDK IR for Developer API document release `trimmed-2026-11-15`.

## Conventions

- Every method also takes two optional trailing parameters, which the signatures below leave out: `RequestOptions? requestOptions = null` sets a request id, a timeout, the retry count or your own `Idempotency-Key` (see [Handle errors and retries](../articles/handle-errors.md)), and `CancellationToken cancellationToken = default` cancels the call and its retries.
- Path ids come first, then the options object. Options properties you leave unset aren't sent.
- Models, options classes and enums live in the `TeachMeHipaa` namespace. Every object a method returns exposes the HTTP response as `LastResponse`.
- A path that contains `{orgId}` is Organization-scoped. The client fills the id in from its Organization binding, so no method takes it.

## Services

- [`AuditEvents`](#auditevents): [`ListAsync`](#auditeventslistasync)
- [`Auth`](#auth): [`IntrospectAsync`](#authintrospectasync)
- [`CertificateVerifications`](#certificateverifications): [`GetAsync`](#certificateverificationsgetasync)
- [`Certificates`](#certificates): [`DownloadPdfAsync`](#certificatesdownloadpdfasync), [`ExportAsync`](#certificatesexportasync), [`GetAsync`](#certificatesgetasync), [`ListAsync`](#certificateslistasync), [`RetrieveLinksAsync`](#certificatesretrievelinksasync)
- [`Compliance`](#compliance): [`GetAsync`](#compliancegetasync)
- [`Courses`](#courses): [`GetAsync`](#coursesgetasync), [`ListAsync`](#courseslistasync)
- [`Enrollments`](#enrollments): [`BatchAsync`](#enrollmentsbatchasync), [`BatchByFilterAsync`](#enrollmentsbatchbyfilterasync), [`CancelAsync`](#enrollmentscancelasync), [`CreateAsync`](#enrollmentscreateasync), [`ExportAsync`](#enrollmentsexportasync), [`GetAsync`](#enrollmentsgetasync), [`ImportAsync`](#enrollmentsimportasync), [`ListAsync`](#enrollmentslistasync), [`PreviewImportAsync`](#enrollmentspreviewimportasync), [`RemindAsync`](#enrollmentsremindasync), [`RenewAsync`](#enrollmentsrenewasync), [`ResendInviteAsync`](#enrollmentsresendinviteasync)
- [`Events`](#events): [`ListAsync`](#eventslistasync)
- [`Learners`](#learners): [`BatchUpsertAsync`](#learnersbatchupsertasync), [`CreateAsync`](#learnerscreateasync), [`CreateLaunchLinkAsync`](#learnerscreatelaunchlinkasync), [`ExportAsync`](#learnersexportasync), [`GetAsync`](#learnersgetasync), [`ListAsync`](#learnerslistasync), [`OffboardAsync`](#learnersoffboardasync), [`RemindAsync`](#learnersremindasync), [`UpdateAsync`](#learnersupdateasync), [`UpsertByExternalIdAsync`](#learnersupsertbyexternalidasync)
- [`Members`](#members): [`GetAsync`](#membersgetasync), [`ListAsync`](#memberslistasync)
- [`Operations`](#operations): [`GetAsync`](#operationsgetasync)
- [`Seats`](#seats): [`GetAsync`](#seatsgetasync)
- [`WebhookSubscriptions`](#webhooksubscriptions): [`CreateAsync`](#webhooksubscriptionscreateasync), [`DeleteAsync`](#webhooksubscriptionsdeleteasync), [`GetAsync`](#webhooksubscriptionsgetasync), [`ListAsync`](#webhooksubscriptionslistasync), [`RedriveAsync`](#webhooksubscriptionsredriveasync), [`TestAsync`](#webhooksubscriptionstestasync), [`UpdateAsync`](#webhooksubscriptionsupdateasync)

## AuditEvents

Reach it as `client.AuditEvents`, an instance of `AuditEventService`.

### `AuditEvents.ListAsync`

```csharp
Task<Page<AuditEvent>> ListAsync(AuditEventListOptions? options = null);
IAsyncEnumerable<AuditEvent> ListAutoPagingAsync(AuditEventListOptions? options = null);
```

`GET /organizations/{orgId}/audit-events`

List all audit events.

Returns a list of the organization's audit events, newest first. Filter by `type`, `transition`, `subjectId`, or a `since` and `until` range.

- **Scopes:** `audit:read`
- **Returns:** `Page<AuditEvent>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`AuditEventListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 100. Defaults to 20.
- `Cursor` (`string?`): The `nextCursor` from the previous page.
- `Type` (`ListAuditEventsType?`): Filter to one audit-event family.
- `Transition` (`ListAuditEventsTransition?`): Filter to one lifecycle transition.
- `SubjectId` (`string?`): Filter to one member (canonical subject id).
- `Since` (`DateTimeOffset?`): Inclusive lower bound on `at` (ISO-8601 instant).
- `Until` (`DateTimeOffset?`): Inclusive upper bound on `at` (ISO-8601 instant).

## Auth

Reach it as `client.Auth`, an instance of `AuthService`.

### `Auth.IntrospectAsync`

```csharp
Task<TokenIntrospection> IntrospectAsync();
```

`GET /auth/introspection`

Retrieve the current token.

Describes the access token you call with: whether it belongs to an API client or a person, its organization, its scopes and when it expires. Needs no scope.

- **Scopes:** any Access token
- **Returns:** `TokenIntrospection`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

## CertificateVerifications

Reach it as `client.CertificateVerifications`, an instance of `CertificateVerificationService`.

### `CertificateVerifications.GetAsync`

```csharp
Task<CertificateVerification> GetAsync(string tinyId);
```

`GET /certificate-verifications/{tinyId}`

Verify a certificate.

Verifies the certificate behind a `https://tme.so/{tinyId}` link. Needs no access token. Limited to 60 requests a minute per IP address.

- **Scopes:** none; the method is public and sends no Access token
- **Returns:** `CertificateVerification`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

## Certificates

Reach it as `client.Certificates`, an instance of `CertificateService`.

### `Certificates.DownloadPdfAsync`

```csharp
Task<BinaryResponse> DownloadPdfAsync(string certId);
Task<BinaryStreamResponse> DownloadPdfStreamingAsync(string certId);
```

`GET /organizations/{orgId}/certificates/{certId}/content`

Download a certificate.

Downloads the certificate as a PDF. Revoked and expired certificates are watermarked. Requires the `certificates:read` scope.

- **Scopes:** `certificates:read`
- **Returns:** `BinaryResponse`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Streaming:** `DownloadPdfStreamingAsync` returns once the headers arrive, so you read the file as it downloads.

### `Certificates.ExportAsync`

```csharp
Task<Operation> ExportAsync(CertificateExportOptions options);
```

`POST /organizations/{orgId}/certificates:export`

Export certificates.

Exports the organization's certificate records as CSV. Requires the `certificates:read` scope.

- **Scopes:** `certificates:read`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

Options (`CertificateExportOptions`):

- `Format` (`CertificatesExportRequestFormat`, required): The file format. Only `csv` today.

### `Certificates.GetAsync`

```csharp
Task<Certificate> GetAsync(string certId);
```

`GET /organizations/{orgId}/certificates/{certId}`

Retrieve a certificate.

Retrieves a certificate.

- **Scopes:** `certificates:read`
- **Returns:** `Certificate`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

### `Certificates.ListAsync`

```csharp
Task<Page<Certificate>> ListAsync(CertificateListOptions? options = null);
IAsyncEnumerable<Certificate> ListAutoPagingAsync(CertificateListOptions? options = null);
```

`GET /organizations/{orgId}/certificates`

List all certificates.

Returns a list of the organization's certificates. Pass `subjectId` to list one learner's.

- **Scopes:** `certificates:read`
- **Returns:** `Page<Certificate>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`CertificateListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 100. Defaults to 20.
- `Cursor` (`string?`): The `nextCursor` from the previous page.
- `SubjectId` (`string?`): Filter to one holder (canonical subject id).

### `Certificates.RetrieveLinksAsync`

```csharp
Task<CertificateLinks> RetrieveLinksAsync(string certId);
```

`GET /organizations/{orgId}/certificates/{certId}/credentials`

Retrieve certificate links.

Retrieves the certificate's LinkedIn add-to-profile link and its public `tme.so` verification link. Requires the `certificates:read` scope.

- **Scopes:** `certificates:read`
- **Returns:** `CertificateLinks`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

## Compliance

Reach it as `client.Compliance`, an instance of `ComplianceService`.

### `Compliance.GetAsync`

```csharp
Task<ComplianceSummary> GetAsync(ComplianceGetOptions? options = null);
```

`GET /organizations/{orgId}/compliance`

Retrieve the compliance summary.

Counts the organization's learners in one course by status. Removed learners aren't counted.

- **Scopes:** `compliance:read`
- **Returns:** `ComplianceSummary`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`ComplianceGetOptions`):

- `CourseId` (`string?`): The course. Defaults to the organization's default course.

## Courses

Reach it as `client.Courses`, an instance of `CourseService`.

### `Courses.GetAsync`

```csharp
Task<Course> GetAsync(string courseId);
Task<ConditionalResult<Course>> GetIfModifiedAsync(string courseId, ETag ifNoneMatch);
```

`GET /courses/{courseId}`

Retrieve a course.

Retrieves a course from the public catalog.

- **Scopes:** none; the method is public and sends no Access token
- **Returns:** `Course`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Conditional read:** `GetIfModifiedAsync` sends `If-None-Match`. Its `ConditionalResult` has a `null` `Value` when your copy is current.

### `Courses.ListAsync`

```csharp
Task<Page<Course>> ListAsync(CourseListOptions? options = null);
IAsyncEnumerable<Course> ListAutoPagingAsync(CourseListOptions? options = null);
```

`GET /courses`

List all courses.

Returns a list of the courses in the public catalog. Needs no access token.

- **Scopes:** none; the method is public and sends no Access token
- **Returns:** `Page<Course>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`CourseListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 100. Defaults to 20.
- `Cursor` (`string?`): The `nextCursor` from the previous page.

## Enrollments

Reach it as `client.Enrollments`, an instance of `EnrollmentService`.

### `Enrollments.BatchAsync`

```csharp
Task<EnrollmentsBatchResults> BatchAsync(EnrollmentBatchOptions options);
```

`POST /organizations/{orgId}/enrollments:batch`

Update many enrollments.

Reminds, resends, cancels, renews, archives or unarchives up to 500 enrollments in one request, chosen by id or by filter. Each enrollment succeeds or fails on its own. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `EnrollmentsBatchResults`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`EnrollmentBatchOptions`):

- `Operation` (`EnrollmentsBatchRequestOperation`, required): What to do to each enrollment: `remind`, `resendInvite`, `cancel`, `recertify`, `archive` or `unarchive`.
- `Select` (`EnrollmentsBatchRequestSelectEnrollmentIds`, required): The enrollments to act on: a list of `enrollmentIds` (up to 500), or a `filter`.

### `Enrollments.BatchByFilterAsync`

```csharp
Task<Operation> BatchByFilterAsync(EnrollmentBatchByFilterOptions options);
```

`POST /organizations/{orgId}/enrollments:batch`

Update many enrollments.

Reminds, resends, cancels, renews, archives or unarchives up to 500 enrollments in one request, chosen by id or by filter. Each enrollment succeeds or fails on its own. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

Options (`EnrollmentBatchByFilterOptions`):

- `Operation` (`EnrollmentsBatchRequestOperation`, required): What to do to each enrollment: `remind`, `resendInvite`, `cancel`, `recertify`, `archive` or `unarchive`.
- `Select` (`EnrollmentsBatchRequestSelectFilter`, required): The enrollments to act on: a list of `enrollmentIds` (up to 500), or a `filter`.

### `Enrollments.CancelAsync`

```csharp
Task<CancelEnrollmentResponse> CancelAsync(
    string enrollmentId,
    EnrollmentCancelOptions? options = null);
```

`POST /organizations/{orgId}/enrollments/{enrollmentId}:cancel`

Cancel an enrollment.

Cancels an invite that hasn't been accepted yet and returns its seat to the pool. With `refund: true`, refunds the seat to the card that paid for it instead. The enrollment remains, with status `CANCELLED`. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `CancelEnrollmentResponse`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`EnrollmentCancelOptions`):

- `Refund` (`bool?`): Refund the seat to the card that paid for it instead of returning it to the pool. Defaults to `false`.

### `Enrollments.CreateAsync`

```csharp
Task<Enrollment> CreateAsync(EnrollmentCreateOptions options);
```

`POST /organizations/{orgId}/enrollments`

Create an enrollment.

Invites a person to a course, creating their profile if needed, and uses one seat. The course defaults to `hipaa`. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `Enrollment`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`EnrollmentCreateOptions`):

- `Email` (`string`, required): The learner's email address.
- `Name` (`string?`): The learner's full name.
- `CourseId` (`string?`): The course to invite them to. Defaults to `hipaa`.

### `Enrollments.ExportAsync`

```csharp
Task<Operation> ExportAsync(EnrollmentExportOptions options);
```

`POST /organizations/{orgId}/enrollments:export`

Export enrollments.

Exports the organization's enrollments as CSV. Requires the `enrollments:read` scope.

- **Scopes:** `enrollments:read`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

Options (`EnrollmentExportOptions`):

- `Format` (`EnrollmentsExportRequestFormat`, required): The file format. Only `csv` today.

### `Enrollments.GetAsync`

```csharp
Task<Enrollment> GetAsync(string enrollmentId);
Task<ConditionalResult<Enrollment>> GetIfModifiedAsync(string enrollmentId, ETag ifNoneMatch);
```

`GET /organizations/{orgId}/enrollments/{enrollmentId}`

Retrieve an enrollment.

Retrieves an enrollment. Requires the `enrollments:read` scope.

- **Scopes:** `enrollments:read`
- **Returns:** `Enrollment`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Conditional read:** `GetIfModifiedAsync` sends `If-None-Match`. Its `ConditionalResult` has a `null` `Value` when your copy is current.

### `Enrollments.ImportAsync`

```csharp
Task<Operation> ImportAsync(EnrollmentImportOptions options);
```

`POST /organizations/{orgId}/enrollments:import`

Import learners from CSV.

Invites everyone in a CSV file with an `email` column (and optional `name` and `courseId`). Send `validateOnly: true` first to preview the file without inviting anyone. Up to 500 rows. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **File:** pass it to the options constructor as a `FileUpload`, from a path, a `byte[]` or a `Stream`.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

Options (`EnrollmentImportOptions`):

- `File` (`FileUpload`, constructor argument): UTF-8 CSV (RFC 4180: quoted fields, `""` escapes, CRLF/LF). Header row required.
- `Options` (`ImportOptions?`)

### `Enrollments.ListAsync`

```csharp
Task<Page<OrgEnrollmentListItem>> ListAsync(EnrollmentListOptions? options = null);
IAsyncEnumerable<OrgEnrollmentListItem> ListAutoPagingAsync(EnrollmentListOptions? options = null);
```

`GET /organizations/{orgId}/enrollments`

List all enrollments.

Returns a list of the organization's enrollments. Filter by `status`, `courseId` or `archived`. Requires the `enrollments:read` scope.

- **Scopes:** `enrollments:read`
- **Returns:** `Page<OrgEnrollmentListItem>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`EnrollmentListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 1000. Defaults to 20.
- `Cursor` (`string?`): The `nextCursor` from the previous page.
- `Status` (`ListEnrollmentsStatus?`): Filter on the derived effectiveStatus.
- `CourseId` (`string?`)
- `Archived` (`ListEnrollmentsArchived?`)
- `Include` (`ListEnrollmentsInclude?`): Opt-in enrichment. With `priorCertificate`, an item carries `priorCertificate` (object, or null when the server confirmed there is none). The key is omitted on an item whose answer is unknown, and on every item of the page if the certificate lookup failed; clients must treat a missing key as unknown, never as null.

### `Enrollments.PreviewImportAsync`

```csharp
Task<ImportPreview> PreviewImportAsync(EnrollmentPreviewImportOptions options);
```

`POST /organizations/{orgId}/enrollments:import`

Import learners from CSV.

Invites everyone in a CSV file with an `email` column (and optional `name` and `courseId`). Send `validateOnly: true` first to preview the file without inviting anyone. Up to 500 rows. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `ImportPreview`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **File:** pass it to the options constructor as a `FileUpload`, from a path, a `byte[]` or a `Stream`.

Options (`EnrollmentPreviewImportOptions`):

- `File` (`FileUpload`, constructor argument): UTF-8 CSV (RFC 4180: quoted fields, `""` escapes, CRLF/LF). Header row required.
- `Options` (`ImportOptions?`)

### `Enrollments.RemindAsync`

```csharp
Task<EnrollmentActionAccepted> RemindAsync(string enrollmentId);
```

`POST /organizations/{orgId}/enrollments/{enrollmentId}:remind`

Remind an enrollment.

Emails the learner again: the invitation if they haven't joined, or a reminder linking to the training if they have. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `EnrollmentActionAccepted`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

### `Enrollments.RenewAsync`

```csharp
Task<Enrollment> RenewAsync(string enrollmentId);
```

`POST /organizations/{orgId}/enrollments/{enrollmentId}:renew`

Renew an enrollment.

Starts the next certification cycle for a certified learner and invites them again, using one seat. Retrying never uses a second seat. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `Enrollment`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

### `Enrollments.ResendInviteAsync`

```csharp
Task<EnrollmentActionAccepted> ResendInviteAsync(string enrollmentId);
```

`POST /organizations/{orgId}/enrollments/{enrollmentId}:resendInvite`

Resend an invite.

Sends a pending invite again, within the same cycle. To invite a certified learner again, renew the enrollment. Requires the `enrollments:write` scope.

- **Scopes:** `enrollments:write`
- **Returns:** `EnrollmentActionAccepted`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

## Events

Reach it as `client.Events`, an instance of `EventService`.

The service also has a hand-written `client.Events.TailAsync(EventTailOptions, CancellationToken)`, which reads the Event log from a saved position and can keep polling for new Events. See [Read and follow the Event log](../articles/event-log.md#follow-the-log-from-a-background-service).

### `Events.ListAsync`

```csharp
Task<Page<Event>> ListAsync(EventListOptions? options = null);
IAsyncEnumerable<Event> ListAutoPagingAsync(EventListOptions? options = null);
```

`GET /organizations/{orgId}/events`

List all events.

Returns a list of the organization's events from the last 30 days, oldest first. Pass the last `eventId` you processed as `after` to resume where you stopped, and `type` to list one event type.

- **Scopes:** `webhooks:manage` or `audit:read`
- **Returns:** `Page<Event>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`EventListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 100. Defaults to 20.
- `After` (`string?`): Return events after this event id (exclusive). The previous page's `nextCursor`.
- `Type` (`EventType?`): Filter to one event type.

## Learners

Reach it as `client.Learners`, an instance of `LearnerService`.

### `Learners.BatchUpsertAsync`

```csharp
Task<Operation> BatchUpsertAsync(LearnerBatchUpsertOptions options);
```

`POST /organizations/{orgId}/learners:batchUpsert`

Create many learners.

Creates or retrieves up to 500 learners in one request. Each item works like creating one learner and succeeds or fails on its own.

- **Scopes:** `learners:write`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

Options (`LearnerBatchUpsertOptions`):

- `Items` (`IReadOnlyList<LearnerBatchUpsertRequestItemsItem>`, required): Up to 500 learners, each with the same fields as creating one learner.
- `Delivery` (`LearnerBatchUpsertRequestDelivery?`): Applies to every item. `email` (default) sends invites; `none` sends nothing.
- `SeatPolicy` (`LearnerBatchUpsertRequestSeatPolicy?`): Applies to every item. `require_available` fails an item when no seat is free.

### `Learners.CreateAsync`

```csharp
Task<LearnerWriteResult> CreateAsync(LearnerCreateOptions options);
```

`POST /organizations/{orgId}/learners`

Create a learner.

Adds a person to the organization and invites them to a course in one call, using one seat. If the email or `externalId` already belongs to an active learner in the course, nothing is created. With `delivery: "none"`, no email is sent and the response includes a `joinUrl` for you to deliver.

- **Scopes:** `learners:write`
- **Returns:** `LearnerWriteResult`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Created:** `Created` on the result is `true` when this call created the record.

Options (`LearnerCreateOptions`):

- `Email` (`string`, required): The learner's email address.
- `Name` (`string?`): The learner's full name.
- `ExternalId` (`string?`): Your own id for the learner, unique within the organization.
- `CourseId` (`string?`): The course to invite them to. Defaults to the organization's default course.
- `Delivery` (`LearnerCreateRequestDelivery?`): `email` (default) sends the invite email. `none` sends nothing and returns `joinUrl` for you to deliver.
- `SeatPolicy` (`LearnerCreateRequestSeatPolicy?`): `require_available` fails with `409` when no seat is free.

### `Learners.CreateLaunchLinkAsync`

```csharp
Task<LearnerLaunchLink> CreateLaunchLinkAsync(
    string learnerId,
    LearnerCreateLaunchLinkOptions? options = null);
```

`POST /organizations/{orgId}/learners/{learnerId}:createLaunchLink`

Create a launch link.

Creates a link that opens the course with the learner already signed in, for a "Start training" button in your product. The link works once and expires after 5 minutes: create it when the learner clicks, then redirect them to it. Never email, store or log it. Only for a learner who has accepted their invite.

- **Scopes:** `learners:write`
- **Returns:** `LearnerLaunchLink`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`LearnerCreateLaunchLinkOptions`):

- `CourseId` (`string?`): The course. Defaults to the organization's default course.

### `Learners.ExportAsync`

```csharp
Task<Operation> ExportAsync(LearnerExportOptions options);
```

`POST /organizations/{orgId}/learners:export`

Export learners.

Exports the organization's learners in one course as CSV, optionally only those with one `status`. Requires the `learners:read` scope.

- **Scopes:** `learners:read`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

Options (`LearnerExportOptions`):

- `Format` (`LearnersExportRequestFormat`, required): The file format. Only `csv` today.
- `CourseId` (`string?`): The course to export. Defaults to the organization's default course.
- `Status` (`LearnerStatus?`): Only export learners with this status.

### `Learners.GetAsync`

```csharp
Task<Learner> GetAsync(string learnerId, LearnerGetOptions? options = null);
```

`GET /organizations/{orgId}/learners/{learnerId}`

Retrieve a learner.

Retrieves a learner's status in one course.

- **Scopes:** `learners:read`
- **Returns:** `Learner`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`LearnerGetOptions`):

- `CourseId` (`string?`): The course. Defaults to the organization's default course.

### `Learners.ListAsync`

```csharp
Task<Page<Learner>> ListAsync(LearnerListOptions? options = null);
IAsyncEnumerable<Learner> ListAutoPagingAsync(LearnerListOptions? options = null);
```

`GET /organizations/{orgId}/learners`

List all learners.

Returns a list of the organization's learners in one course, with each learner's status, dates and current certificate. Filter by `status`, or find one learner by `email` or `externalId`.

- **Scopes:** `learners:read`
- **Returns:** `Page<Learner>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`LearnerListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 100. Defaults to 20.
- `Cursor` (`string?`): The `nextCursor` from the previous page.
- `Email` (`string?`)
- `ExternalId` (`string?`): The integration's own id for the learner (unique per organization), or the SCIM `externalId` when none was set.
- `Status` (`LearnerStatus?`)
- `CourseId` (`string?`): The course. Defaults to the organization's default course.

### `Learners.OffboardAsync`

```csharp
Task<Learner> OffboardAsync(string learnerId, LearnerOffboardOptions? options = null);
```

`POST /organizations/{orgId}/learners/{learnerId}:offboard`

Offboard a learner.

Removes a learner from the organization. A learner who hasn't started has their invite cancelled and the seat returned; a learner who has started keeps their training history and certificates. Releases the learner's `externalId`.

- **Scopes:** `learners:write`
- **Returns:** `Learner`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Concurrency:** set `IfMatch` (the ETag string, or the object you read) to send `If-Match`, so the call fails if the object changed.

Options (`LearnerOffboardOptions`):

- `IfMatch` (`ETag`, optional): The ETag you read the object with, or the object itself.
- `CourseId` (`string?`): The course. Defaults to the organization's default course.

### `Learners.RemindAsync`

```csharp
Task<LearnerRemindAccepted> RemindAsync(string learnerId, LearnerRemindOptions? options = null);
```

`POST /organizations/{orgId}/learners/{learnerId}:remind`

Remind a learner.

Emails the learner a reminder: the invite again if they haven't started, or a nudge to finish if they have.

- **Scopes:** `learners:write`
- **Returns:** `LearnerRemindAccepted`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`LearnerRemindOptions`):

- `CourseId` (`string?`): The course. Defaults to the organization's default course.

### `Learners.UpdateAsync`

```csharp
Task<Learner> UpdateAsync(string learnerId, LearnerUpdateOptions options);
```

`PATCH /organizations/{orgId}/learners/{learnerId}`

Update a learner.

Updates a learner's `name` or `externalId`; parameters you don't send are left unchanged. Send the body as `application/merge-patch+json` with the learner's current `ETag` in `If-Match`.

- **Scopes:** `learners:write`
- **Returns:** `Learner`
- **Retries:** none, because replaying the call isn't safe.
- **Concurrency:** the options constructor takes an `ETag` (the ETag string, or the object you read) and sends it as `If-Match`. A stale ETag throws `PreconditionFailedException`.
- **Body:** a JSON merge patch. Unset `Optional<T>` properties aren't sent, and a property set to `null` clears the field.

Options (`LearnerUpdateOptions`):

- `IfMatch` (`ETag`, constructor argument, required): The ETag you read the object with, or the object itself.
- `Name` (`Optional<string>`): The learner's full name.
- `ExternalId` (`Optional<string?>`): Your own id for the learner. Set it to `null` to clear it.
- `CourseId` (`string?`): The course. Defaults to the organization's default course.

### `Learners.UpsertByExternalIdAsync`

```csharp
Task<LearnerWriteResult> UpsertByExternalIdAsync(
    string externalId,
    LearnerUpsertByExternalIdOptions options);
```

`PUT /organizations/{orgId}/learners/by-external-id/{externalId}`

Create or retrieve a learner by your id.

Makes sure a learner exists for your own id. Creates and invites the learner the first time, and returns them on every later call, so a sync job can replay it safely. Never uses a second seat.

- **Scopes:** `learners:write`
- **Returns:** `LearnerWriteResult`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Created:** `Created` on the result is `true` when this call created the record.

Options (`LearnerUpsertByExternalIdOptions`):

- `Email` (`string`, required): The learner's email address.
- `Name` (`string?`): The learner's full name.
- `CourseId` (`string?`): The course to invite them to. Defaults to the organization's default course.
- `Delivery` (`LearnerUpsertRequestDelivery?`): `email` (default) sends the invite email. `none` sends nothing and returns `joinUrl` for you to deliver.
- `SeatPolicy` (`LearnerUpsertRequestSeatPolicy?`): `require_available` fails with `409` when no seat is free.

## Members

Reach it as `client.Members`, an instance of `MemberService`.

### `Members.GetAsync`

```csharp
Task<Member> GetAsync(string subjectId);
```

`GET /organizations/{orgId}/members/{subjectId}`

Retrieve a member.

Retrieves a member of the organization.

- **Scopes:** `members:read`
- **Returns:** `Member`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

### `Members.ListAsync`

```csharp
Task<Page<MemberListItem>> ListAsync(MemberListOptions? options = null);
IAsyncEnumerable<MemberListItem> ListAutoPagingAsync(MemberListOptions? options = null);
```

`GET /organizations/{orgId}/members`

List all members.

Returns a list of the organization's members, its admins and learners. Pass `role=admin` to list only admins.

- **Scopes:** `members:read`
- **Returns:** `Page<MemberListItem>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`MemberListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 1000. Defaults to 20.
- `Cursor` (`string?`): The `nextCursor` from the previous page.
- `Include` (`string?`): Opt-in, comma-separated. With `enrollmentSubject`, a member whose `subjectId` is null carries `enrollmentSubjectId`: the subject their email resolves to (a never-signed-in invitee), or null when none. Use it only to match the member to Enrollments; it is not an address for member routes. With `holdsInvitation`, each learner carries `holdsInvitation`: whether the organization has an invitation for their email or a Seat bound to them. Each key is omitted when its lookup failed.
- `Role` (`ListMembersRole?`): Only admins (paginated).

## Operations

Reach it as `client.Operations`, an instance of `OperationService`.

### `Operations.GetAsync`

```csharp
Task<Operation> GetAsync(string operationId);
```

`GET /organizations/{orgId}/operations/{operationId}`

Retrieve an operation.

Retrieves a bulk operation and its result. Operations are kept for 24 hours.

- **Scopes:** any Access token
- **Returns:** `Operation`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

## Seats

Reach it as `client.Seats`, an instance of `SeatService`.

### `Seats.GetAsync`

```csharp
Task<SeatsInventory> GetAsync();
```

`GET /organizations/{orgId}/seats`

Retrieve the seat inventory.

Retrieves how many seats the organization has, how many are assigned and how many are available for new invites. Requires the `enrollments:read` scope.

- **Scopes:** `enrollments:read`
- **Returns:** `SeatsInventory`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

## WebhookSubscriptions

Reach it as `client.WebhookSubscriptions`, an instance of `WebhookSubscriptionService`.

### `WebhookSubscriptions.CreateAsync`

```csharp
Task<WebhookSubscriptionCreated> CreateAsync(WebhookSubscriptionCreateOptions options);
```

`POST /organizations/{orgId}/webhook-subscriptions`

Create a webhook subscription.

Subscribes an HTTPS endpoint to event types and creates its signing secret. Requires the `webhooks:manage` or `org:admin` scope.

- **Scopes:** `org:admin` or `webhooks:manage`
- **Returns:** `WebhookSubscriptionCreated`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.

Options (`WebhookSubscriptionCreateOptions`):

- `EndpointUrl` (`string`, required): The HTTPS URL to deliver events to.
- `EventTypes` (`IReadOnlyList<EventType>`, required): The event types to deliver.
- `Description` (`string?`): A description for your own reference.
- `SeatsLowThreshold` (`long?`): Send `seats.low` when available seats drop to this number. Defaults to 5.

### `WebhookSubscriptions.DeleteAsync`

```csharp
Task<TeachMeHipaaResponse> DeleteAsync(string subscriptionId);
```

`DELETE /organizations/{orgId}/webhook-subscriptions/{subscriptionId}`

Delete a webhook subscription.

Deletes a webhook subscription and its signing secret. Requires the `webhooks:manage` or `org:admin` scope.

- **Scopes:** `org:admin` or `webhooks:manage`
- **Returns:** `TeachMeHipaaResponse`
- **Retries:** none, because replaying the call isn't safe.

### `WebhookSubscriptions.GetAsync`

```csharp
Task<WebhookSubscription> GetAsync(string subscriptionId);
Task<ConditionalResult<WebhookSubscription>> GetIfModifiedAsync(
    string subscriptionId,
    ETag ifNoneMatch);
```

`GET /organizations/{orgId}/webhook-subscriptions/{subscriptionId}`

Retrieve a webhook subscription.

Retrieves a webhook subscription. Requires the `webhooks:manage` or `org:admin` scope.

- **Scopes:** `org:admin` or `webhooks:manage`
- **Returns:** `WebhookSubscription`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Conditional read:** `GetIfModifiedAsync` sends `If-None-Match`. Its `ConditionalResult` has a `null` `Value` when your copy is current.

### `WebhookSubscriptions.ListAsync`

```csharp
Task<Page<WebhookSubscription>> ListAsync(WebhookSubscriptionListOptions? options = null);
IAsyncEnumerable<WebhookSubscription> ListAutoPagingAsync(
    WebhookSubscriptionListOptions? options = null);
```

`GET /organizations/{orgId}/webhook-subscriptions`

List all webhook subscriptions.

Returns a list of the organization's webhook subscriptions. Signing secrets are never returned. Requires the `webhooks:manage` or `org:admin` scope.

- **Scopes:** `org:admin` or `webhooks:manage`
- **Returns:** `Page<WebhookSubscription>`
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Paging:** `await foreach` over the returned page, or over `ListAutoPagingAsync`, reads every item across pages.

Options (`WebhookSubscriptionListOptions`):

- `Limit` (`long?`): How many objects to return, between 1 and 100. Defaults to 20.
- `Cursor` (`string?`): The `nextCursor` from the previous page.

### `WebhookSubscriptions.RedriveAsync`

```csharp
Task<Operation> RedriveAsync(string subscriptionId);
```

`POST /organizations/{orgId}/webhook-subscriptions/{subscriptionId}:redrive`

Retry failed deliveries.

Delivers the subscription's failed events again. Requires the `webhooks:manage` or `org:admin` scope.

- **Scopes:** `org:admin` or `webhooks:manage`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

### `WebhookSubscriptions.TestAsync`

```csharp
Task<Operation> TestAsync(string subscriptionId);
```

`POST /organizations/{orgId}/webhook-subscriptions/{subscriptionId}:test`

Send a test event.

Sends a signed test delivery to the subscription's endpoint, so you can check your verifier. Requires the `webhooks:manage` or `org:admin` scope.

- **Scopes:** `org:admin` or `webhooks:manage`
- **Returns:** `Operation`
- **Idempotency:** sends a generated `Idempotency-Key` that every retry reuses. Set `RequestOptions.IdempotencyKey` to choose the key yourself.
- **Retries:** on network errors, 409 `conflict`, 429 and 5xx.
- **Long-running:** returns an `Operation`. Call `WaitAsync` to poll until it finishes.

### `WebhookSubscriptions.UpdateAsync`

```csharp
Task<WebhookSubscription> UpdateAsync(
    string subscriptionId,
    WebhookSubscriptionUpdateOptions options);
```

`PATCH /organizations/{orgId}/webhook-subscriptions/{subscriptionId}`

Update a webhook subscription.

Updates a webhook subscription; parameters you don't send are left unchanged. Send its current `ETag` in `If-Match`. The signing secret doesn't change. Requires the `webhooks:manage` or `org:admin` scope.

- **Scopes:** `org:admin` or `webhooks:manage`
- **Returns:** `WebhookSubscription`
- **Retries:** none, because replaying the call isn't safe.
- **Concurrency:** the options constructor takes an `ETag` (the ETag string, or the object you read) and sends it as `If-Match`. A stale ETag throws `PreconditionFailedException`.
- **Body:** a JSON merge patch. Unset `Optional<T>` properties aren't sent, and a property set to `null` clears the field.

Options (`WebhookSubscriptionUpdateOptions`):

- `IfMatch` (`ETag`, constructor argument, required): The ETag you read the object with, or the object itself.
- `EndpointUrl` (`Optional<string>`): The HTTPS URL to deliver events to.
- `EventTypes` (`Optional<IReadOnlyList<EventType>>`): The event types to deliver.
- `Enabled` (`Optional<bool>`): Turn deliveries on or off.
- `Description` (`Optional<string?>`): A description for your own reference.
- `SeatsLowThreshold` (`Optional<long>`): Send `seats.low` when available seats drop to this number.

## Event types

`Events.ListAsync`, `Events.TailAsync` and `WebhookUtility.ConstructEvent` return subclasses of `Event`, one per Event type. Match on the class with a type pattern. An Event type this version of the library doesn't know arrives as `UnknownEvent`.

| Event type | Class | `Data` |
| --- | --- | --- |
| `certificate.revoked` | `EventCertificateRevoked` | `CertificateRevokedEventData` |
| `enrollment.certified` | `EventEnrollmentCertified` | `EnrollmentCertifiedEventData` |
| `invite.bounced` | `EventInviteBounced` | `LearnerEventData` |
| `learner.certified` | `EventLearnerCertified` | `LearnerEventData` |
| `learner.expired` | `EventLearnerExpired` | `LearnerEventData` |
| `learner.expiring` | `EventLearnerExpiring` | `LearnerEventData` |
| `learner.invited` | `EventLearnerInvited` | `LearnerEventData` |
| `learner.removed` | `EventLearnerRemoved` | `LearnerEventData` |
| `learner.started` | `EventLearnerStarted` | `LearnerEventData` |
| `seat.consumed` | `EventSeatConsumed` | `IReadOnlyDictionary<string, JsonElement>` |
| `seat.freed` | `EventSeatFreed` | `IReadOnlyDictionary<string, JsonElement>` |
| `seats.low` | `EventSeatsLow` | `SeatsLowEventData` |
