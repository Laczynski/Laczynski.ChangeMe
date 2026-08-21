# Cross-module feature workflow

> Type: development
> Scope: system
> Status: implemented
> Canonical for: changes that cross frontend, backend, persistence, and system operations
>
> Before starting: read the target `FR-*` (L4). For UI work, read [`product-standards.md`](../../requirements/_shared/conventions/product-standards.md) (L2).

## Add a backend endpoint

1. Create or update the endpoint in `src/ChangeMe.Backend.Web/Endpoints/<Feature>/` (use `<Feature>/<ChildResource>/` for nested routes — see **Feature and sub-slice layout** in [backend development](../../modules/backend/development.md)).
2. Pick the endpoint base type (see **Endpoint conventions** in [backend development](../../modules/backend/development.md)):
   - `BaseEndpoint<TRequest, TResponse>` when FastEndpoints should bind body, query, or route into `TRequest`
   - `BaseEndpointWithoutRequest<TRequest, TResponse>` when there is no HTTP payload (use a parameterless `record` for `TRequest`)
   - a custom `Endpoint` / `EndpointWithoutRequest` only for multipart upload, binary download, or other non-standard responses
3. Add or update the validator in the same file when the request DTO needs validation.
4. Add the request contract and handler in the matching `src/ChangeMe.Backend.UseCases/<Feature>/` folder (sub-slice when nested).
5. Reuse domain behavior or add it in `Domain` if new invariants are introduced.
6. Add integration tests under `tests/ChangeMe.Backend.IntegrationTests/Endpoints/<Feature>/`.

## Add a persisted field

1. Update the domain model if the field is part of business state.
2. Update the EF configuration in `Infrastructure/Persistence/Config`.
3. Add a migration in `Infrastructure/Persistence/Migrations`.
4. Update use case DTOs and frontend models if the field crosses the API.
5. Add unit and integration coverage for the new behavior.

## Add a frontend screen backed by an existing endpoint

1. Add or extend the model in `features/<feature>/models`.
2. Add or extend the service in `features/<feature>/services`.
3. Create a standalone component under `features/<feature>/components/<name>/`.
4. Register the route in `src/app/app.routes.ts` if it is navigable directly.
5. Run lint and add frontend unit tests when client logic changes ([testing strategy](testing-strategy.md)); colocate `*.spec.ts` next to the component or service.

## Add or migrate a list screen (DataGrid)

Use the nearest existing **list screen** under `features/*/components/*-list/` as the template.

### Backend

1. List DTO in `UseCases/<Feature>/Dtos/` — `[GridSearchable]` on searchable text fields; `[GridIgnore]` on computed or collection fields not meant for server sort/filter.
2. Query: `public GridQuery Grid { get; set; } = new();` returning `GridResult<TDto>`.
3. Handler: project to `IQueryable<TDto>`, then `await projected.ToGridResultAsync(query.Grid, cancellationToken)`.
4. Endpoint: `BaseEndpoint<GetAllXQuery, GridResult<TDto>>` with `GET` route; FastEndpoints binds `?grid=` automatically.

### Frontend

1. Service method: `getAllX(grid: GridQuery): Observable<GridResult<TDto>>` via `apiService.get(..., { grid })`.
2. Component: inject `GridResourceFactory`, create `grid` in constructor with `load`, `defaultSort`, `defaultTake`, optional `persistState`.
3. Template: `<dg-prime-data-grid [grid]="grid">` with `dgColumn` templates per column; `dgEmpty` for empty state.
4. Add component unit tests with a mocked `GridResourceFactory`.

## Change auth-sensitive behavior

1. Check backend endpoint auth defaults in `BaseEndpoint` / `BaseEndpointWithoutRequest`.
2. Check route guards in the auth feature slice.
3. Check token and session handling in the auth feature service.
4. Add or update integration coverage for authenticated and anonymous flows.
5. Add or update guard/component unit tests; extend E2E smoke only when the user journey changes ([testing strategy](testing-strategy.md)).

## Add file upload and download

Follow the shared attachment infrastructure under `Domain/Common/Attachments/`, `Infrastructure/FileStorage/`, and the **`attachments`** table (TPH). Copy the nearest existing attachment sub-slice in `Web/Endpoints/` and `UseCases/` before inventing a parallel upload flow.

### Backend

1. Add a value to **`AttachmentType`** (`Domain/Common/Attachments/`), a derived entity (for example `<Feature>Attachment : Attachment`), and aggregate methods for owner-specific rules.
2. Configure persistence: shared TPH mapping in **`Infrastructure/Persistence/Config/Attachments/AttachmentConfiguration.cs`** (add `.HasValue<…>(AttachmentType.…)`); owner relationship in **`Config/<Feature>/`** for the owning aggregate.
3. Reuse `Infrastructure/FileStorage/`:
   - `IFileContentValidator` + Mime-Detective content inspection
   - `IFileStorageService` + `LocalFileStorageService` (`container` + `ownerId` paths)
4. Use **file-first upload with a single DB commit**:
   - validate content, then create attachment metadata and side effects in memory
   - write file to storage using the generated opaque key
   - `SaveChanges` once (metadata + history/notifications when applicable)
   - on failure before `SaveChanges`, delete the stored file only; if `SaveChanges` fails after a successful storage write, EF rolls back metadata and **`AttachmentStorageCleanupJob`** removes the orphaned file on the next run
5. Reuse `AttachmentStorageCleanupJob` (Hangfire): orphaned stored files with no matching metadata row for all attachment types.
6. Add list/upload/download/delete use cases in `UseCases/<Feature>/<ChildResource>/` (or at the feature root when not nested). Colocate sub-slice DTOs and utils under `<ChildResource>/Dtos/` and `<ChildResource>/Utils/` when they are not shared with the parent feature.
7. Add endpoints in `Web/Endpoints/<Feature>/<ChildResource>/`:
   - JSON list/delete via `BaseEndpoint` (route/query/body binding as needed)
   - parameterless JSON actions via `BaseEndpointWithoutRequest` (e.g. logout, mark-all-read)
   - multipart upload via `Endpoint` + `AllowFileUploads()`; send the handler `Result<T>` with `HttpContext.SendResultAsync`
   - binary download via `EndpointWithoutRequest`; set `Content-Disposition: attachment` and `X-Content-Type-Options: nosniff`, stream bytes to `Response.Body`; use `HttpContext.SendResultAsync` only for error `Result<T>` responses
8. Cascade-delete stored files when the owning aggregate is removed.
9. Add integration tests for happy path, validation failure, auth, and delete authorization.
10. Update [file storage operations](../../modules/backend/operations/file-storage.md) when volume, backup, or retention changes.

### Frontend

1. Extend feature models and `features/<feature>/utils` with size/extension limits.
2. Add `ApiService.postFormData()` and `getBlob()` helpers; keep endpoint strings in the feature service.
3. Add a tab or panel component with upload control, paginated list, download, and uploader-only delete.
4. Reuse PrimeNG `p-fileupload` (basic mode) or equivalent; show inline validation errors and toasts for mutations.
