# NapsterAI

A small ASP.NET Core Web API that wraps the [Napster Companion/Agent platform API](https://developers.napster.com/docs/introduction/quickstart).

> **Note on scope:** the original `implementation-plan.md` in this repo describes a *music* API
> (search artists/albums/tracks). As of this rewrite, `developers.napster.com` documents a
> completely different product: an AI conversational agent platform ("Omniagents") — companions,
> agents, live voice/web connections, and session transcripts. This project targets that current,
> real API instead of the (no longer documented) legacy music API.

The app covers eight endpoints confirmed against the live docs (and, in a couple of cases, corrected
against what the live API actually does - see below):

- Browse Napster's stock AI companion personas.
- Create an agent (a named, reusable configuration binding a companion to voice/provider settings).
- Create a live connection (a token a client uses to start a real-time voice/web session with a companion).
- List past conversation sessions.
- List and create knowledge bases (containers of reference documents an agent can be pointed at).
- List and create FAQ collections, and list the question/answer items within one.

## Project structure

```
NapsterAI.slnx
src/NapsterAI.Playground/       React + TypeScript + Vite front-end (see below)
src/NapsterAI.Api/
  Program.cs                     Composition root: DI, HttpClient, middleware pipeline
  appsettings.json                Base configuration (no secrets)
  appsettings.Development.json    Dev-only logging overrides
  Configuration/
    NapsterOptions.cs             Strongly-typed "Napster" config section
  Controllers/
    CompanionsController.cs       GET /api/companions
    AgentsController.cs           POST /api/agents
    ConnectionsController.cs      POST /api/connections
    SessionsController.cs         GET /api/sessions
    KnowledgeBasesController.cs   GET, POST /api/knowledgebases
    FaqsController.cs             GET, POST /api/faqs, GET /api/faqs/{id}/items
  Services/
    INapsterService.cs / NapsterService.cs   HttpClient calls to Napster + error handling
  Models/
    Napster/                      Internal DTOs matching Napster's raw JSON shape
    Dtos/                         Public DTOs returned by our own API
  Exceptions/
    NapsterApiException.cs        InvalidRequestException, NapsterApiException, NapsterResourceNotFoundException, NapsterConflictException
  Middleware/
    ExceptionHandlingMiddleware.cs  Converts exceptions into ProblemDetails (400/404/409/502/500)
tests/NapsterAI.Tests/
  Services/NapsterServiceTests.cs        Service-layer tests against a fake HttpMessageHandler
  Controllers/CompanionsControllerTests.cs  Controller tests with a mocked INapsterService
```

## NuGet packages

| Project | Package | Purpose |
|---|---|---|
| NapsterAI.Api | Microsoft.Extensions.Http.Polly | Retry policy for transient Napster failures (5xx/429) |
| NapsterAI.Api | Swashbuckle.AspNetCore | Swagger UI / OpenAPI docs |
| NapsterAI.Tests | Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio | Test framework |
| NapsterAI.Tests | Moq | Mocking `INapsterService` in controller tests |
| NapsterAI.Tests | coverlet.collector | Code coverage collection |

Everything else (`Microsoft.AspNetCore.*`, `System.Net.Http.Json`, `System.Text.Json`) ships with the
ASP.NET Core / .NET SDK, no extra package needed.

## Configuring the Napster API key

The key is **never** hard-coded or committed. `appsettings.json` only has an empty placeholder:

```json
"Napster": {
  "BaseUrl": "https://companion-api.napster.com/public/",
  "ApiKey": "",
  "ApiVersion": "",
  "TimeoutSeconds": 10
}
```

Get a key from the [Napster developer dashboard](https://companion-api.napster.com/admin) (Keys →
+ Create API key), then supply it one of two ways:

**Option A - .NET user-secrets (recommended for local dev)**

```bash
cd src/NapsterAI.Api
dotnet user-secrets init
dotnet user-secrets set "Napster:ApiKey" "YOUR_NAPSTER_API_KEY"
```

This stores the key outside the repo (in your user profile), and ASP.NET Core's configuration
system picks it up automatically in the Development environment.

**Option B - environment variable**

ASP.NET Core maps environment variables using `__` as the section separator:

```bash
# bash / macOS / Linux
export Napster__ApiKey="YOUR_NAPSTER_API_KEY"

# PowerShell
$env:Napster__ApiKey = "YOUR_NAPSTER_API_KEY"
```

This is the approach to use in production / containers (e.g. set it as a secret in your hosting
platform of choice). The key is sent as the `X-Api-Key` header on every outbound request — never
in a URL or query string, and never usable from client-side code.

## Running locally

```bash
# from the repo root
dotnet restore
dotnet build

# set your API key first (see above), then:
dotnet run --project src/NapsterAI.Api
```

The API listens on `http://localhost:5080` (see `Properties/launchSettings.json`). Swagger UI opens
automatically at `http://localhost:5080/swagger` in Development.

### Running the tests

```bash
dotnet test
```

## Playground (front-end)

`src/NapsterAI.Playground` is a React + TypeScript + Vite app that gives you a testing UI similar to
Napster's own admin "playground" page, but built on top of this project's own API rather than talking
to Napster directly:

- **Companions** — search/browse and pick one from the sidebar.
- **Agent** — configure name, language, voice, instructions (`providerSettings.instructions`), and
  attach a knowledge base / FAQ collections, then `POST /api/agents`.
- **Knowledge bases** / **FAQs** — list and create, so you have something to attach to an agent.
- **Live chat** — mints a connection token via `POST /api/connections` and mounts
  [Napster's Web SDK](https://developers.napster.com/docs/sdks/web-sdk/overview)
  (`@touchcastllc/napster-companion-api`) inline to actually talk/chat with the companion in real
  time (voice + a typed-text side channel), with a live transcript.
- **Sessions** — lists past sessions via `GET /api/sessions`.

The playground never talks to Napster directly or holds the `X-Api-Key` — every request goes through
this repo's own API, which is the only thing that knows the key. The one exception is the live chat
tab: once a connection `token` is minted server-side, the *browser* uses it to open the real-time
WebRTC session directly with Napster (by design — see "Create a live connection" above).

**Run it:**

```bash
# terminal 1 — the API (needs Napster:ApiKey configured, see above)
dotnet run --project src/NapsterAI.Api

# terminal 2 — the playground
cd src/NapsterAI.Playground
npm install
npm run dev
```

Open the URL Vite prints (`http://localhost:5173`). The API's CORS policy in `Program.cs` explicitly
allows `http://localhost:5173`/`:4173` (Vite's dev/preview ports) — update `PlaygroundCorsPolicy` in
`Program.cs` if you serve the playground from somewhere else. The playground's own API base URL is
set via `VITE_API_BASE_URL` in `src/NapsterAI.Playground/.env.development` (defaults to
`http://localhost:5080/api`, matching the API's default port).

## Example requests

**Browse companions**

```
GET /api/companions?search=alex&gender=female&pageIndex=0&pageSize=20
```

```json
{
  "items": [
    {
      "id": "cmp.1a2b3c",
      "firstName": "Alex",
      "lastName": "Rivers",
      "previewUrl": "https://cdn.napster.com/companions/cmp.1a2b3c/preview.jpg",
      "videoLoopUrl": "https://cdn.napster.com/companions/cmp.1a2b3c/loop.mp4",
      "ethnicity": "unspecified",
      "gender": "female",
      "headline": "Friendly and curious",
      "tags": { "style": "casual" },
      "status": "ready"
    }
  ],
  "totalCount": 42,
  "filteredCount": 1,
  "pageIndex": 0,
  "pageSize": 20
}
```

**Create an agent**

```
POST /api/agents
Content-Type: application/json

{
  "companionId": "cmp.1a2b3c",
  "name": "Support Bot",
  "language": "en-US",
  "voiceId": "alloy",
  "providerSettings": { "instructions": "Be concise and friendly." }
}
```

> `voiceId` is documented as optional but Napster's own validation rejects agent creation
> without one (confirmed live: `400 "Voice id is required."`) - this API requires it client-side too.

```json
{
  "id": "agt.9f8e7d",
  "companionId": "cmp.1a2b3c",
  "name": "Support Bot",
  "previewUrl": "https://cdn.napster.com/agents/agt.9f8e7d/preview.jpg",
  "language": "en-US",
  "voiceId": "alloy",
  "functions": [],
  "disableIdleTimeout": false,
  "useWebSearch": false,
  "created": 1735689600
}
```

**Create a live connection**

```
POST /api/connections
Content-Type: application/json

{
  "companionId": "cmp.1a2b3c",
  "providerConfig": { "voiceId": "alloy" }
}
```

> `providerConfig.voiceId` isn't documented for this endpoint at all, but Napster's own
> validation rejects connection creation without it (confirmed live: `400 "Voice id is required."`) -
> this API requires it client-side too. Note the placement differs from Create Agent: here it's
> nested inside `providerConfig`, not a top-level field.

```json
{
  "token": "tok_abcdef123456",
  "connectionId": "conn.44556677"
}
```

The `token` is what a client app (web/voice) uses to open the actual WebRTC/WebSocket session
directly with Napster — this API only brokers it and never proxies the media stream itself.

**List sessions**

```
GET /api/sessions?companionId=cmp.1a2b3c&sessionType=webrtc&pageIndex=0&pageSize=20
```

```json
{
  "items": [
    {
      "id": "ses.112233",
      "companionId": "cmp.1a2b3c",
      "companionFirstName": "Alex",
      "companionLastName": "Rivers",
      "externalClientId": null,
      "sessionType": "webrtc",
      "modality": "voice",
      "status": "closed",
      "agentName": "Support Bot",
      "closeReason": "client_disconnected",
      "cost": 0.42,
      "createdAt": 1735689600,
      "startedAt": 1735689605,
      "closedAt": 1735689900
    }
  ],
  "totalCount": 1,
  "filteredCount": 1,
  "pageIndex": 0,
  "pageSize": 20
}
```

**Create a knowledge base**

```
POST /api/knowledgebases
Content-Type: application/json

{
  "name": "Support Docs",
  "provider": "azureOpenAI"
}
```

```json
{
  "id": "kb.7f8e9d",
  "name": "Support Docs",
  "provider": "azureOpenAI",
  "itemsCount": 0,
  "tags": {},
  "created": 1735689600
}
```

Use the returned `id` as `knowledgeBaseId` on Create Agent to attach it. `GET /api/knowledgebases`
lists existing ones, with optional `provider`/`search` filters.

**Create an FAQ collection**

```
POST /api/faqs
Content-Type: application/json

{
  "name": "Billing FAQs",
  "faqs": [
    { "question": "How do refunds work?", "answer": "Refunds are processed within 30 days." }
  ]
}
```

```json
{
  "id": "faq.4c5d6e",
  "name": "Billing FAQs",
  "itemsCount": 1,
  "created": 1735689600
}
```

`GET /api/faqs` lists existing collections; `GET /api/faqs/{id}/items` lists the question/answer
pairs within one. Use a collection's `id` as an entry in `faqCollections` on Create Agent to attach it.

### Error responses

Every error is an `application/problem+json` body (RFC 7807), produced centrally by
`ExceptionHandlingMiddleware` so controllers stay free of try/catch noise:

| Situation | Status | Example |
|---|---|---|
| Bad input we catch before calling Napster (out-of-range `pageSize`, invalid `gender`/`sessionType`, missing `companionId`/`name`/`providerSettings`/`providerConfig`/`voiceId`/`providerConfig.voiceId`) | 400 | `{"title":"Invalid request","detail":"pageSize must be between 1 and 100."}` |
| Napster itself rejects the request as malformed (HTTP 400) | 400 | `{"title":"Invalid request","detail":"name is required"}` |
| Napster reports a conflict (HTTP 409, e.g. duplicate agent) | 409 | `{"title":"Conflict","detail":"..."}` |
| Companion/agent/session not found upstream (HTTP 404) | 404 | `{"title":"Resource not found","detail":"..."}` |
| Napster unreachable, times out, rejects credentials (401/403), rate-limits (429), or returns malformed JSON | 502 | `{"title":"Napster API error","detail":"The Napster API rejected our credentials. Check that Napster:ApiKey is configured correctly."}` |
| Anything unexpected | 500 | `{"title":"Unexpected error","detail":"An unexpected error occurred while processing the request."}` |
| ASP.NET Core model-binding validation (e.g. malformed JSON body) | 400 | standard ASP.NET Core validation problem details |

## How the important parts work

- **`NapsterOptions`** is bound from the `Napster` config section (`Configuration/NapsterOptions.cs`)
  via the options pattern, so the API key/base URL/timeout are never scattered around as magic strings.
- **Authentication** is a plain header, not OAuth: `Program.cs` sets `X-Api-Key` (and, if configured,
  `X-API-Version`) as default headers on the typed `HttpClient`, so every request carries it automatically.
- **`NapsterService`** is registered as a *typed* `HttpClient` (`AddHttpClient<INapsterService, NapsterService>`)
  with a Polly retry policy for transient failures (network blips, 5xx, 429). It:
  1. Validates its own inputs and throws `InvalidRequestException` for bad input (out-of-range paging, an invalid `gender`/`sessionType` value, a missing required field) — before any network call is made.
  2. Catches network-level failures (`HttpRequestException`, timeouts) and wraps them as `NapsterApiException`.
  3. Inspects non-success responses and extracts a human-readable message regardless of which shape Napster used - in practice this is **not** one consistent shape: a JSON array of `{code, description, type, numericType}` objects for business-rule validation, standard ASP.NET Core validation `ProblemDetails` (`{errors: {Field: [...]}, ...}`) for missing-field checks, or plain `ProblemDetails` (`{title, status, ...}`) for everything else. `ExtractErrorDescriptionAsync` walks the raw JSON defensively rather than binding to one fixed model. The status code then maps to the right exception: 400 → `InvalidRequestException`, 404 → `NapsterResourceNotFoundException`, 409 → `NapsterConflictException`, 401/403 → a credentials `NapsterApiException`, 429 → a rate-limit `NapsterApiException`, anything else → a generic `NapsterApiException` carrying the upstream status code.
  4. Catches JSON parsing failures so a malformed upstream response can't crash the request.
  5. Maps the raw Napster JSON models (`Models/Napster`) onto our own DTOs (`Models/Dtos`) so callers of our API never see Napster's response shape directly. Provider-specific nested objects whose schema isn't pinned down in the public docs (`providerSettings`, `providerConfig`, `mcp`, `tags`) are passed through as raw JSON (`JsonElement`) rather than guessed at field-by-field.
- **`ExceptionHandlingMiddleware`** sits at the top of the pipeline and turns those exception types into
  the correct HTTP status + `ProblemDetails` body, so every controller can just `await` the service and
  return `Ok(...)`/`CreatedAtAction(...)` without repeating try/catch blocks.
- **Controllers** are intentionally thin — they validate nothing themselves beyond what `[ApiController]`
  model binding already gives for free, and delegate all business validation/error handling to the
  service + middleware.

### Undocumented requirements found by testing against the live API

The public docs don't fully match Napster's actual validation. Found by exercising the real API:

- **Create Agent** (`POST /api/agents`): `voiceId` is listed as optional in the docs, but Napster
  rejects agent creation without one (`400 "Voice id is required."`). It's a top-level field.
  Enforced client-side in `NapsterService.CreateAgentAsync`.
- **Create Connection** (`POST /api/connections`): `voiceId` isn't mentioned in the docs for this
  endpoint at all, but Napster rejects connection creation unless `providerConfig` contains a
  `voiceId` string - a *different* placement than Create Agent (nested, not top-level). Enforced
  client-side in `NapsterService.CreateConnectionAsync`.
- **Create Connection** (`POST /api/connections`): `externalClientId`, when supplied, must match
  `^[A-Za-z0-9_-]{1,32}$` (letters, digits, `-`, `_`, max 32 chars) - confirmed live: `400 "Invalid
  external client ID. Allowed characters are letters, digits, hyphens (-), and underscores (_), with
  a maximum of 32 characters."`. Enforced client-side in `NapsterService.CreateConnectionAsync`.
- **`voiceId`'s actual *value* is provider-specific, and neither Napster's REST API nor this one
  validates it.** A companion's live session uses whichever TTS engine its `providerConfig`/avatar
  resolves to (observed: an `"avatar.voices"` entry of `{"id": "external", "provider": "OpenAI"}` in
  the decoded connection token) - passing a voice name from the wrong engine (e.g. an Azure Speech
  name like `en-US-JennyNeural` against an OpenAI-backed companion) returns a `200 OK` with a token
  from `POST /connections`, but the live SDK session then fails at connect time with `"Invalid voice
  provided"`. This API can only check that *some* non-empty `voiceId` was supplied, the same as
  Napster's own REST layer - it cannot validate the value is correct for the resolved provider, since
  there's no documented endpoint to look that up ahead of time. `alloy` (a standard OpenAI TTS voice)
  is used in the examples here since the companions tested against defaulted to OpenAI; if your live
  session still rejects it, decode the `token` from the `POST /connections` response (it's base64 JSON)
  and check `avatar.voices[].provider` to find the actual engine in use.

## What isn't covered

The docs also describe Digital Twins, Functions, MCP Servers, SIP Connections, Recordings, and
Organization quota requests. Only Companions/Agents/Connections/Sessions/Knowledge Bases/FAQs are
implemented here — the others follow the exact same pattern (add a raw model in `Models/Napster`, a
public DTO in `Models/Dtos`, a method on `INapsterService`, and a thin controller) if you need them.
Knowledge Bases and FAQs are also only list+create here, not the full CRUD the docs describe (update/delete,
knowledge-base file upload, individual FAQ item management) - same pattern applies to add those too.

---

## Real Estate module

A second, self-contained vertical layered onto this same API: a property listings + CRM backend, a
public marketing/search site, an agent/admin dashboard, and an AI chat assistant that can search
inventory, answer questions, and book viewings/capture leads on a visitor's behalf via WebMCP. It
reuses this repo's AI-session infrastructure (`POST /api/agents`, `POST /api/connections`) and the
dormant EdgeMCP bridge shipped in Napster's Web SDK — it does not talk to Napster for anything else.
See `RealEstateImplementationPlan.md` for the full design rationale and phase-by-phase build log.

### Project structure

```
src/NapsterAI.Api/
  Configuration/JwtOptions.cs, CorsOptions.cs      Strongly-typed Jwt/Cors config sections
  Data/RealEstateDbContext.cs                      EF Core DbContext (SQL Server), own connection string
  Data/RealEstateDbInitializer.cs                  Dev-only idempotent seed (5 projects, 17 unit types,
                                                    17 inventory rows, 21 amenities, 2 login users)
  Data/Migrations/                                 EF Core migrations (supersede database/RealEstateDB.sql)
  Controllers/RealEstate/*.cs                      /api/realestate/* - Projects, UnitTypes, Inventory,
                                                    Amenities, Locations, Lookups, Organizations, Agents,
                                                    Leads, Inquiries, Viewings, Analytics
  Controllers/AuthController.cs                    /api/auth/login, /api/auth/refresh
  Models/Dtos/RealEstate/*.cs, Models/Entities/RealEstate/*.cs
  Services/RealEstate/*.cs                         One service (+interface) per resource, EF Core-backed
  Exceptions/RealEstateExceptions.cs                RealEstateResourceNotFoundException (404),
                                                    RealEstateConflictException (409)
src/NapsterAI.RealEstate.Web/                       React 19 + TS + Vite 8 + Tailwind v4 + shadcn/ui,
                                                    port 5174 - public site + /admin/* dashboard in one app
  src/routes/public/                               Home, Search, ProjectDetail, UnitDetail, Contact, Landing
  src/routes/admin/                                Dashboard, Projects, Inventory, Leads, Viewings,
                                                    Agents/Organizations (Admin-only)
  src/routes/webmcp-demo/WebSdkDemoPage.tsx         Live document.modelContext / tool-call-log QA page
  src/lib/edge-mcp/registerRealEstateTools.ts       Registers the 7 EdgeMCP tools (see below)
  src/components/chat/                              RealEstateChatWidget.tsx (Napster Web SDK, floating widget)
tests/NapsterAI.Tests/
  Services/RealEstate/*Tests.cs                     EF Core InMemory service tests
  Controllers/RealEstate/*Tests.cs                  Moq-mocked-service controller tests
  Integration/RealEstateApiFactory.cs               WebApplicationFactory<Program> against EF Core InMemory
  Integration/*IntegrationTests.cs                  Real end-to-end HTTP tests (auth pipeline, anonymous
                                                    public writes) through the actual ASP.NET Core pipeline
database/RealEstateDB.sql                           Original hand-written schema - kept as historical
                                                    reference only; the EF Core migrations are authoritative
```

### Running it

```bash
# terminal 1 - the API (see "Connection string & JWT signing key" below first)
dotnet run --project src/NapsterAI.Api

# terminal 2 - the Real Estate frontend
cd src/NapsterAI.RealEstate.Web
npm install
npm run dev
```

Open `http://localhost:5174`. In Development, `Program.cs` runs `dotnet ef` migrations and the dev
seed automatically on startup — no manual DB step needed beyond having a reachable SQL Server instance.

**Dev login accounts** (seeded by `RealEstateDbInitializer`, clearly-fake credentials, never reuse in
a real deployment): `admin@realestate.local` / `Admin123!` (role `Admin` — full access incl.
Organizations/Agents management and listing approvals) and `agent@realestate.local` / `Agent123!`
(role `Agent` — CRUD on listings/leads/viewings, no org/agent management or approvals).

### Connection string & JWT signing key

Neither is committed with a real value, same convention as `Napster:ApiKey` above:

```bash
cd src/NapsterAI.Api
dotnet user-secrets set "ConnectionStrings:RealEstateDb" "Server=localhost;Database=RealEstateAIDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:SigningKey" "<any long random string>"
```

If `Jwt:SigningKey` is left unset, `Program.cs` generates a random ephemeral key for that run only (logs
a `[WARN]`) so the app still starts locally — tokens issued before a restart just stop validating after
one. Set a real, stable key via user-secrets/env vars for anything beyond a single dev session.

### Auth & CORS

Hand-rolled JWT bearer (no ASP.NET Identity) — `POST /api/auth/login` issues a token; `AdminOnly` and
`AgentOrAdmin` authorization policies gate writes (see `Program.cs`). Public catalog `GET`s (Projects,
Inventory, Amenities, Locations, Lookups) stay `[AllowAnonymous]`; `POST /inquiries` and `POST /viewings`
also stay anonymous (they're the public "request info"/"book a viewing" forms and what the AI chat
assistant's `createLead`/`bookViewing` tools call) but are rate-limited (`Microsoft.AspNetCore.RateLimiting`,
10 requests/min per client IP), as is `/api/auth/login` (5/min per IP) to slow credential-stuffing. CORS
origins are config-driven via the `Cors:AllowedOrigins` section, not hardcoded, so a prod origin can be
added without a code change.

### AI chat assistant / EdgeMCP tools

`src/NapsterAI.RealEstate.Web/src/lib/edge-mcp/registerRealEstateTools.ts` installs the
[`@mcp-b/webmcp-polyfill`](https://www.npmjs.com/package/@mcp-b/webmcp-polyfill) (the standard
`document.modelContext` browser API) and registers 7 tools that Napster's `EdgeMcpBridge` (bundled in
`@touchcastllc/napster-companion-api`, auto-invoked by `NapsterCompanionApiSdk.init()`) discovers and
calls automatically once a chat session starts — `searchInventory`, `getProjectDetails`,
`calculatePaymentPlan`, `getAmenities`, `checkAvailability` (all read-only) and `bookViewing`,
`createLead` (destructive — leads sourced this way carry `channel: "AI Assistant"` so they're
distinguishable from the public web form in admin reporting). Each tool calls the same anonymous public
endpoints the site itself uses. Configure `VITE_REAL_ESTATE_COMPANION_ID` in
`src/NapsterAI.RealEstate.Web/.env.development` (see the file's own comment for the exact runbook via
`NapsterAI.Playground`) to make the floating chat widget render; without it the widget just doesn't
appear, rather than erroring for visitors. `http://localhost:5174/webmcp-demo` is a standalone QA page
that shows live tool registration status, a resource push (`re://search-results`) driven by
`searchInventory`, and a running log of every tool call's args/result/latency — useful for verifying the
tool wiring without needing a live voice/chat session.

### Testing

```bash
dotnet test                                        # backend: service + controller + integration tests

cd src/NapsterAI.RealEstate.Web
npm run test                                        # frontend: Vitest + React Testing Library
```

Backend tests follow this repo's existing conventions: `Microsoft.EntityFrameworkCore.InMemory` (a fresh
database per test) for services, Moq-mocked service interfaces for controllers, and
`WebApplicationFactory<Program>` (`tests/NapsterAI.Tests/Integration/RealEstateApiFactory.cs`, running
under a `"Testing"` ASP.NET Core environment that swaps in the InMemory provider) for real end-to-end
HTTP tests of the JWT auth pipeline and the anonymous public write endpoints. Frontend tests prioritize
hooks and the lead/viewing forms over exhaustive component coverage, per this module's own testing
strategy. `postman/NapsterAI.postman_collection.json` has one folder per resource plus a
"Real Estate - EdgeMCP Tools" folder (each tool's underlying HTTP call, for debugging outside the AI
loop) and an extended "Error Handling Examples" folder covering 400/401/403/404/409 across both modules.
