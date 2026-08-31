# AGENTS.md

## Scope

These instructions apply to the entire ATMS backend and API repository, including services, contracts, data access, infrastructure, tests, migrations, and API projects.

Treat this repository as a real production application. Follow the existing architecture, naming, and coding conventions before introducing new patterns.

## Core working rules

1. Inspect the relevant code before editing.
2. Find the closest existing implementation and follow its structure, naming, architecture, and test style.
3. Prefer small, focused, maintainable changes over broad rewrites.
4. Do not modify unrelated files.
5. Do not add production dependencies without explicit approval.
6. Do not weaken validation, authorization, typing, tests, security, or error handling merely to make a task pass.
7. Never claim that a build, test, migration, database update, or command passed unless it was actually executed.
8. Before a complex change, summarize:
   - current behavior;
   - intended implementation;
   - assumptions;
   - risks;
   - likely files to change.
9. After implementation, report:
   - changed files;
   - commands executed;
   - build and test results;
   - migration and database update status;
   - anything that could not be verified;
   - remaining risks.
10. Suggest up to three concrete improvements when they would materially improve quality, security, performance, or maintainability. Do not implement unrelated improvements without approval.

---

## Commit messages

Never add trailers or attribution lines to a commit message, a pull request body, or any commit text you draft for a human to use.

Specifically forbidden:

- `Co-Authored-By: Claude ...` or any other `Co-Authored-By` trailer
- `Generated with ...`, `Co-authored by an AI`, or similar attribution
- Any footer naming an AI tool, model, or assistant

A commit message ends with its last substantive line. Nothing follows it.

This applies regardless of who wrote the code — including commits an agent authored end to end.

---

# Code layout and vertical readability

- Optimize code for vertical scanning and readable diffs, not for minimizing the number of lines.
- Keep short, cohesive expressions on one line. Do not split every word, argument, or property mechanically.
- Break code across multiple lines when a single line becomes difficult to scan, requires horizontal scrolling, contains several logical parts, or hides the structure of the code.
- Format long parameter lists, generic types, LINQ and fluent chains, object initializers, boolean expressions, attributes, and method calls vertically at meaningful boundaries.
- Keep one statement or declaration per line; do not compress several operations into a wide one-line block.
- Follow the repository formatter, but improve the source layout when the formatter alone still leaves code unnecessarily wide or difficult to review.

---

# General engineering quality

- Write simple, professional, production-quality C#.
- Follow SOLID and object-oriented design principles pragmatically.
- Do not create abstractions without a real need.
- Follow the existing dependency injection, CQRS/Mediator, validation, mapping, repository, service, result, and exception patterns.
- Keep controllers and endpoints thin.
- Keep business rules in the appropriate application or domain layer.
- Prefer explicit, readable code over clever code.
- Preserve nullable reference type correctness.
- Pass `CancellationToken` through asynchronous operations where supported.
- Do not use blocking calls such as `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` in asynchronous request flows.
- Avoid unnecessary database materialization and N+1 queries.
- Do not expose database entities directly through public API contracts.
- Use request and response contracts to prevent over-posting and accidental data exposure.
- Apply authorization and tenant/workspace boundaries consistently.
- Never log secrets, tokens, passwords, connection strings, or sensitive personal data.
- Do not reveal internal exception details, stack traces, database information, or implementation details to API clients.
- Follow the repository's existing exception and result handling conventions.

---

# Contracts and files

- Keep each significant public class, command, query, model, response, validator, criteria, filter, handler, and service in its own focused file.
- Use descriptive names such as:
  - `CreateProductCommand.cs`;
  - `CreateProductCommandHandler.cs`;
  - `CreateProductCommandValidator.cs`;
  - `GetProductsQuery.cs`;
  - `ProductFilter.cs`;
  - `ProductResponse.cs`.
- Do not group several unrelated contracts into one generic file.
- Reuse existing base classes only when the abstraction is valid and already established.

---

# String properties and nullability

- Do not initialize mandatory string properties with `string.Empty` merely to silence nullable warnings.
- Do not use the null-forgiving operator `!` without a justified reason.
- For required input contracts, prefer `required` members, constructors, or another repository-approved validation pattern.
- For optional values, use nullable types explicitly.
- For EF Core entities, follow the existing entity construction and materialization pattern instead of applying `required` mechanically.

---

# Localization and user-facing messages

- User-facing messages that can appear in the UI must be localized.
- This includes validation messages, conflict messages, not-found messages, confirmation-related API messages, and business-rule messages such as “A product with these details already exists.”
- Reuse the most appropriate existing `.resx` resource.
- Create a new focused resource only when no suitable resource exists.
- Provide user-friendly versions in:
  - English;
  - Russian;
  - Azerbaijani.
- Write natural, simple wording for each language.
- Do not use literal machine-like translations or developer jargon.
- Tell the user what happened and, when useful, what they can do next.
- Internal log messages do not need to be localized.
- Do not expose internal identifiers or technical exception names in localized user-facing text.

---

# Validation and security

- Validate all externally supplied data.
- Use the existing validator pattern and add validation tests.
- Enforce authorization server-side even when the UI hides an action.
- Verify ownership, tenant, workspace, and role boundaries before reading or modifying data.
- Prevent mass assignment and over-posting.
- Use parameterized database access through the existing ORM or repository patterns.
- Do not weaken CORS, authentication, authorization, rate limits, antiforgery, or validation without explicit approval.
- Avoid returning more fields than the client needs.
- Consider concurrency and idempotency for commands that can be retried.
- Preserve auditability for important changes according to existing project patterns.

---

# Redis and caching

- Evaluate whether a read path should use the existing Redis/cache pattern.
- Do not cache everything automatically.
- Cache only when appropriate for performance and data consistency.
- Reuse existing cache key builders, serialization, TTL, and abstraction patterns.
- Every cached mutable value must have an invalidation or refresh strategy.
- On create, update, delete, import, role change, tenant change, or other relevant mutation, invalidate all affected cache entries.
- Avoid stale list, details, permission, and dictionary caches.
- Do not cache secrets or sensitive data unless the existing approved architecture explicitly supports it.
- Add or update tests for cache hits, misses, and invalidation when valuable.
- Document any consistency trade-off introduced by caching.

---

# EF Core and database migrations

- When an entity or database schema changes, create the appropriate EF Core migration.
- Review both `Up` and `Down`.
- Verify:
  - names;
  - types;
  - lengths;
  - nullability;
  - defaults;
  - indexes;
  - foreign keys;
  - delete behavior;
  - data compatibility.
- Consider existing production data before making a non-nullable or destructive schema change.
- Run the migration against the local development database when the required local environment is available.
- Never update a shared development, test, staging, or production database without explicit approval and a confirmed connection target.
- Do not generate an empty or unrelated migration.
- After applying locally, run relevant integration tests and confirm the application starts.
- Report the migration name and whether the database update was actually executed.

---

# OpenAPI and XML documentation

- Add or update XML documentation for public API endpoints where the repository currently documents them.
- Add XML documentation to request contracts only when they expose filtering, search, or sorting parameters.
- Do not add XML documentation to commands, response models, or simple request contracts.
- Follow the closest existing endpoint or filtered request documentation pattern.
- Document:
  - purpose;
  - request meaning;
  - response meaning;
  - important validation or authorization behavior;
  - relevant response codes.
- Keep documentation user-oriented and accurate.
- Update OpenAPI annotations, response metadata, examples, or schemas when the public contract changes.
- Do not duplicate misleading boilerplate documentation.

---

# Testing

## Required testing behavior

- Every new behavior and bug fix must have automated test coverage when technically practical.
- Update an existing test when behavior changes.
- Add a regression test for a bug fix.
- During implementation, run the smallest relevant test set for fast feedback.
- Before declaring the task complete, run:
  - all directly affected tests;
  - the affected project test suite;
  - the broader solution test suite when feasible.
- If the full suite cannot run because of time, environment, credentials, external services, or unrelated failures, clearly report:
  - what was run;
  - what passed;
  - what failed;
  - what was not run;
  - why.

## Test organization

- Use one focused test class and file per production subject.
- Do not create one broad `ProductHandlerTests` file covering all handlers.
- Prefer:
  - `CreateProductCommandHandlerTests`;
  - `UpdateProductCommandHandlerTests`;
  - `GetProductQueryHandlerTests`;
  - `GetProductsQueryHandlerTests`;
  - `DeleteProductCommandHandlerTests`;
  - `CreateProductCommandValidatorTests`;
  - `ProductCriteriaTests`;
  - `ProductFilterTests`;
  - focused service tests.
- Follow the repository's established singular/plural naming convention if it differs.
- Inherit from the existing `BaseHandlerTest` or other approved test base when appropriate.
- Improve shared test bases only for genuinely shared setup or helpers.
- Do not hide important scenario setup inside a large base class.
- Keep Arrange, Act, and Assert clear.
- Test behavior rather than implementation details.
- Avoid brittle tests that depend on ordering, timestamps, generated IDs, or shared mutable state unless those are part of the contract.
- Controller tests belong in the existing controller/API test project and should follow its current patterns.

## xUnit scenarios

- Use `[Fact]` for one specific behavior.
- Use `[Theory]` with `InlineData`, `MemberData`, or `ClassData` for meaningful scenario matrices.
- Prefer theories for:
  - validation boundaries;
  - null, empty, and invalid values;
  - roles and permissions;
  - filter combinations;
  - status transitions;
  - localization inputs;
  - multiple equivalent business cases.
- Do not force unrelated scenarios into one theory merely to reduce line count.
- Use descriptive test names that state the condition and expected result.
- Cover happy paths, validation failures, not-found cases, conflicts, authorization, cancellation, edge cases, and cache invalidation where applicable.


---

# Explicit ATMS backend rules — mandatory

## Localized user-facing messages

- Every message that is returned to the client and can be displayed to a user must be localized when its wording depends on the selected language.
- Example: a business conflict such as `Product already exists` must not be hard-coded in a handler, controller, validator, or service.
- Reuse the most appropriate existing `.resx` file.
- If no suitable resource exists, create a focused resource in the established localization structure.
- Add natural, user-friendly text in:
  - English;
  - Russian;
  - Azerbaijani.
- Do not translate word-for-word mechanically.
- Avoid developer terminology, internal entity names, exception names, database terms, and technical jargon.
- The user should immediately understand:
  - what happened;
  - why the action could not continue when appropriate;
  - what they can do next when useful.
- Log-only diagnostic messages do not need localization.
- Do not expose internal technical details through localized API responses.

## Redis for GetById and cache invalidation

- For `GetById` queries and handlers, use the project's existing Redis caching pattern unless the value is already cached or caching would violate an established security or consistency rule.
- Use the existing cache abstraction, cache-key builder, serialization format, and configuration.
- Every `GetById` cache entry must have a TTL.
- Reuse the existing configured TTL when one exists; do not hard-code a new arbitrary lifetime.
- On Update:
  - invalidate or refresh the affected `GetById` cache entry after the database change succeeds.
- On Delete:
  - invalidate the affected `GetById` cache entry after the database change succeeds.
- Invalidate related list, details, dictionary, permission, or aggregate cache entries when the mutation makes them stale.
- Never invalidate the cache before a database mutation is confirmed unless the existing consistency strategy explicitly requires it.
- Add or update tests for:
  - cache hit;
  - cache miss;
  - TTL/configuration usage when testable;
  - invalidation after Update;
  - invalidation after Delete.
- If caching is deliberately skipped for a `GetById` flow, explain the concrete technical reason in the final report.

## String property initialization

- Do not initialize string properties with `string.Empty` merely to suppress nullable warnings.
- Do not use `= null!;` or another null-forgiving initialization merely to satisfy the compiler.
- Use the repository's proper model for required and optional values:
  - `required` members for required contracts when appropriate;
  - constructors for required domain/entity state when appropriate;
  - nullable `string?` for optional values;
  - validation for external input.
- Follow existing EF Core materialization patterns for entities.
- Any exception to this rule must have a concrete framework or serialization reason.

## OpenAPI and XML documentation

- Add or update OpenAPI metadata and XML documentation for every new or changed public endpoint.
- Add or update XML documentation for request contracts only when they contain filtering, search, or sorting parameters.
- Do not add XML documentation to commands, response models, or simple request contracts.
- Document controllers, endpoint actions, public API response codes, and eligible filtered request parameters.
- Use the closest existing documented endpoint/query as the template.
- Documentation must explain the real behavior and must not be empty boilerplate.
- Update response types, status codes, authorization notes, validation behavior, and examples when the public contract changes.

## Tests are required in addition to build

- A successful build is not enough.
- For every new feature:
  - add tests for the new behavior;
  - update affected old tests;
  - run all directly affected tests;
  - run the affected test project;
  - run the full available solution test suite before completion when the environment allows it.
- For every bug fix:
  - add or update a regression test that fails before the fix and passes after the fix when technically practical.
- If tests already exist but do not cover the changed scenario, extend them.
- If no suitable test exists, create a new focused test.
- Never report completion after only running `dotnet build`.

## One focused test class per production subject

- Do not create one broad test file such as `ProductHandlerTests` for several handlers.
- Create a separate test class and file for every handler, validator, criteria, filter, service, and other independently testable production subject.
- Example handler tests:
  - `CreateProductHandlerTests`;
  - `UpdateProductHandlerTests`;
  - `GetProductHandlerTests`;
  - `GetProductsHandlerTests`;
  - `DeleteProductHandlerTests`.
- If the production naming includes `CommandHandler` or `QueryHandler`, use the matching precise test name, for example:
  - `CreateProductCommandHandlerTests`;
  - `GetProductQueryHandlerTests`.
- Apply the same separation to:
  - validators;
  - criteria;
  - filters;
  - services;
  - repositories when unit-tested;
  - cache behavior;
  - controllers in the appropriate API/controller test project.
- Use the existing `BaseHandlerTest` for handler tests where appropriate.
- Extend `BaseHandlerTest` only with genuinely reusable setup, mocks, builders, or assertions.
- A product-specific base such as `BaseProductHandlerTest` may be created only when several Product handler test classes share meaningful Product-specific setup.
- Do not put unrelated scenarios into a base class merely to reduce duplication.

## xUnit Theory and scenario coverage

- Use `[Fact]` for one fixed scenario.
- Use `[Theory]` with `InlineData`, `MemberData`, or `ClassData` when several input combinations should produce the same category of behavior.
- Use theories wherever appropriate for:
  - valid and invalid boundaries;
  - null, empty, and whitespace values;
  - string lengths;
  - enums and statuses;
  - permissions and roles;
  - filter combinations;
  - localization cultures;
  - equivalent success or failure cases.
- Cover nearly all meaningful scenarios, not only the happy path.
- Include, where applicable:
  - success;
  - validation failures;
  - not found;
  - already exists/conflict;
  - unauthorized/forbidden;
  - cancellation;
  - repository or dependency failure behavior;
  - cache hit;
  - cache miss;
  - cache invalidation;
  - edge and boundary values.
- Do not force unrelated scenarios into one theory merely for fewer lines.
- Use descriptive test names that clearly state the condition and expected result.

---

# Aspire and runtime diagnostics

- When the local Aspire AppHost is available, use the Aspire dashboard as the primary runtime diagnostic surface for backend issues.
- Inspect the state and logs of every project involved in the request flow, not only the project where the symptom appeared. At minimum, check Gateway, Admin API, and Project API when the flow crosses those boundaries.
- For asynchronous messaging issues, trace the complete delivery path:
  - producer and database transaction;
  - outbox record and dispatcher;
  - RabbitMQ exchange, routing key, main queue, retry queue, and dead-letter queue;
  - consumer logs and inbox record;
  - target database state.
- Check externally managed local infrastructure such as PostgreSQL, RabbitMQ, and Redis when it is not represented as an Aspire resource.
- Correlate errors by timestamp, message ID, request/trace ID, and entity ID where available. Never log or report passwords, tokens, connection strings, or unnecessary personal data.
- Distinguish the root cause from unrelated warnings and report both clearly when the warnings are materially relevant.
- After a fix, reproduce the same flow, recheck Aspire logs for all affected projects, verify queue/dead-letter state, and confirm the final database state before reporting success.
- If Aspire is unavailable, state that explicitly and use the closest available logs and infrastructure diagnostics instead.

---

# Definition of done

A backend task is complete only when all applicable items are satisfied:

- Existing patterns were inspected and followed.
- The implementation is focused and production-safe.
- Types, validation, authorization, nullability, and error handling are correct.
- User-facing messages are localized in English, Russian, and Azerbaijani when applicable.
- Redis caching and invalidation were considered where relevant.
- A database migration was created for schema changes and reviewed.
- OpenAPI and XML documentation were updated when public behavior changed.
- Relevant automated tests were added or updated.
- Aspire resource states and logs for every affected project were checked when the local AppHost was available.
- Builds and tests were actually run where the environment allowed.
- Local database update was performed only when the target was confirmed.
- The final diff contains no unrelated changes.
- Any unverified item is explicitly reported.
