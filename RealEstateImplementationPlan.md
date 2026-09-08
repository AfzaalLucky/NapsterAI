# Real Estate Module — Implementation Plan

## Context

`Prompt.md` asks for a comprehensive implementation plan (no code yet) for a Real Estate module. Research shows this repo is **not** a blank real-estate app: `NapsterAI.Api` + `NapsterAI.Playground` are an existing AI-companion/conversational-agent platform (voice/chat "companions", agents, sessions, knowledge bases, FAQs — see `README.md`). `RealEstate.md` describes the intended shape: Customer → Web SDK/AI Assistant → WebMCP/EdgeMCP (business-action tools) → Property/CRM APIs → Listings/Leads. So Real Estate is a **new vertical layered on the existing platform**, reusing its AI-session infrastructure (`/api/agents`, `/api/connections`) and its dormant EdgeMCP bridge, not a rewrite.

`database\RealEstateDB.sql` (SQL Server) already defines 4 tables (`Projects`, `UnitTypes`, `Inventory`, `Amenities`) but is completely unwired — no app code touches it, no FK constraints, all-nullable columns, no indexes beyond PKs, and no Users/Agents/Organizations/Leads/Media tables at all. The backend has zero data-access layer today (all "data" is proxied live from the upstream Napster API) and zero authentication. The frontend (`NapsterAI.Playground`) is a hand-styled internal test harness (no router, no Tailwind/shadcn, no real state library) — a separate app for search/admin/public UX is warranted rather than stretching it.

This plan is organized as a blueprint another developer can execute phase by phase. **No code, migrations, or files are created by this task** — planning only.

---

## 1. Review summary — reuse vs. net-new vs. gaps

**Reuse as-is:** `Middleware\ExceptionHandlingMiddleware.cs` (extend with new catch clauses), `Models\Dtos\PagedResultDto.cs`, the validate-then-`InvalidRequestException` idiom in `Services\NapsterService.cs` (static allow-list arrays for enum-like fields), the existing `POST /api/agents` / `POST /api/connections` (AI session infra — Real Estate doesn't need its own), the dormant `EdgeMcpBridge` shipped inside `@touchcastllc/napster-companion-api` (Real Estate only needs to register tools against `document.modelContext`, not build a bridge), `postman\NapsterAI.postman_collection.json` folder structure, xUnit+Moq test conventions in `tests\NapsterAI.Tests`.

**Net-new:** any data-access layer (no EF Core/Dapper/DbContext exists), any auth (`UseAuthorization()` is called but nothing populates the principal — no `[Authorize]` anywhere), CRM concepts (`Users`, `Organizations`, `SalesAgents`, `Customers`, `Leads`, `Inquiries`, `Viewings`, `Media` — none exist), a Tailwind/shadcn design system, a router + data-fetching library on the frontend.

**Gaps/inconsistencies to fix:** `RealEstateDB.sql` has zero FKs, no unique constraints on natural keys (`ProjectCode`, `UnitNumber`), no indexes beyond PKs, free-text enum fields (`ProjectType`, `Status`×2, `Category`×2, `FurnishingStatus`) with no lookup table; `/api/agents`/`AgentDto` already mean "AI conversation agent" — real-estate sales-agent types/routes must be disambiguated; `appsettings.json` currently commits the live Napster API key in plaintext — don't repeat that for the new JWT signing key / SQL connection string (user-secrets/env vars only).

---

## 2. Key architectural decisions (recommended, with justification)

| Decision | Recommendation | Why |
|---|---|---|
| New project vs. extend `NapsterAI.Api` | **Extend in place**, feature-folder convention (`Controllers/RealEstate/`, `Services/RealEstate/`, etc.) | Keeps EdgeMCP/AI-session endpoints in-process; route prefix `/api/realestate/*` + class prefixes solve naming collisions without a second deployable/CORS surface. |
| Data access | **EF Core, Code-First + migrations** (SQL Server) | Matches .NET idioms, gives LINQ for search/filter/sort/paging, DI-friendly like `INapsterService`. `RealEstateDB.sql` becomes the reference for the first migration, not a script the app runs. |
| Auth | **Hand-rolled JWT bearer**, not full ASP.NET Identity | No auth infra exists; a minimal issuer against a new `Users` table matches the codebase's already-lightweight, hand-rolled style. Roles: `Admin`, `Agent`, anonymous for public reads. |
| FK constraints | **Add real FKs** in the new EF Core schema | This is now genuine local persistence (not a live external proxy), so integrity matters; EF navigation properties need real FKs. |
| DB scope | **Keep `RealEstateDB` standalone** (own connection string/DbContext) | Nothing on the companion side persists locally — nothing to merge with. |
| Frontend | **New sibling project `src\NapsterAI.RealEstate.Web`** (React 19 + TS + Vite, port 5174), Playground untouched | Playground is intentionally a minimal AI-platform test harness; stretching it into public site + admin dashboard destabilizes it. No monorepo tooling exists today, so this stays a second standalone npm project, same convention as Playground. |
| Router | **React Router v7** | Best fit for nested public vs. `/admin/*` guarded layouts. |
| Data fetching | **TanStack Query** | Matches the `PagedResultDto<T>` list+mutation shape used everywhere. |
| Global state | **React Context/Zustand for auth only** — don't carry Redux Toolkit forward | RTK is an unused SDK peer-dep in Playground today; don't repeat that pattern. |
| App split | **Single Vite app**, `/admin/*` as a guarded route subtree | Trivially answers "share the design system between admin and public" — literally the same `components/ui`. |

---

## 3. Database schema evolution

Keep `database\RealEstateDB.sql` as historical reference (untouched); the first EF Core migration supersedes it.

**Extend existing tables:**
- `Projects`: add `ApprovalStatus` (Draft/PendingReview/Approved/Rejected), `IsDeleted`; make `ProjectCode`, `ProjectName`, `ProjectType`, `Status`, `City`, `Country`, `Currency`, `IsActive`, `IsFeatured` NOT NULL; unique constraint on `ProjectCode`.
- `UnitTypes`: real FK `ProjectID → Projects` (cascade delete).
- `Inventory`: real FKs `ProjectID → Projects`, `UnitTypeID → UnitTypes` (restrict delete); add nullable `SalesAgentID → SalesAgents` (keep legacy `AgentName`/`AgentContact` as migration bridge); add `ApprovalStatus`; NOT NULL on `UnitNumber`/`Status`; unique on `(ProjectID, UnitNumber)`.
- `Amenities`: real FK `ProjectID → Projects` (cascade delete).

**New tables:** `Locations` (Country/City/District/Lat/Long lookup), `Media` (EntityType/EntityID/MediaType/Url/DisplayOrder/IsPrimary — replaces single-scalar image/video/brochure/floorplan URL columns, legacy columns kept for compatibility), `Organizations` (agencies), `SalesAgents` (OrganizationID FK, UserID FK nullable), `Users` (auth: Email/PasswordHash/Role/SalesAgentID), `Customers`, `Inquiries` (raw public/AI contact capture), `Leads` (Status: New/Contacted/Qualified/ViewingScheduled/Negotiation/Won/Lost), `LeadActivities` (CRM timeline), `Viewings` (backs `bookViewing` tool), `PaymentPlanMilestones` (structured replacement for free-text `Projects.PaymentPlan`, needed so `calculatePaymentPlan` can compute), `Lookups` (generic reference-data table for admin dropdowns — enforcement stays in the service layer via static allow-lists, same idiom as `NapsterService`, not DB CHECK constraints).

**Indexes (none exist beyond PKs today):** `Projects`: unique(`ProjectCode`), index on `City`/`IsActive`/`IsFeatured`/`ProjectType`/`Status`. `Inventory`: unique(`ProjectID`,`UnitNumber`), index on `ProjectID`/`UnitTypeID`/`Status`/`ListPrice`/`Bedrooms`/`SalesAgentID`. `UnitTypes`/`Amenities`: index on `ProjectID`. `Leads`: index on `Status`/`SalesAgentID`/`ProjectID`/`CreatedDate`. `Media`: composite index on `(EntityType, EntityID)`.

---

## 4. Backend architecture (`src\NapsterAI.Api`)

**New folders/files:**
```
Configuration/JwtOptions.cs, CorsOptions.cs
Data/RealEstateDbContext.cs, RealEstateDbInitializer.cs (dev seed)
Data/Configurations/*.cs  (one EF config class per entity)
Data/Migrations/<ts>_InitialRealEstateSchema.cs, <ts>_AddCrmTables.cs
Controllers/RealEstate/RealEstateProjectsController.cs        /api/realestate/projects
                        RealEstateUnitTypesController.cs       /api/realestate/unit-types
                        RealEstateInventoryController.cs       /api/realestate/inventory
                        RealEstateAmenitiesController.cs       /api/realestate/amenities
                        RealEstateMediaController.cs           /api/realestate/media
                        RealEstateAgentsController.cs          /api/realestate/agents
                        RealEstateOrganizationsController.cs   /api/realestate/organizations
                        RealEstateLocationsController.cs       /api/realestate/locations
                        RealEstateLookupsController.cs         /api/realestate/lookups
                        RealEstateLeadsController.cs           /api/realestate/leads
                        RealEstateInquiriesController.cs       /api/realestate/inquiries
                        RealEstateViewingsController.cs        /api/realestate/viewings
                        RealEstateAnalyticsController.cs       /api/realestate/analytics
                        AuthController.cs                      /api/auth/login, /api/auth/refresh
Models/Dtos/RealEstate/*.cs   (ProjectDtos, UnitTypeDtos, InventoryDtos, AmenityDtos, MediaDtos,
                                RealEstateAgentDtos, OrganizationDtos, LocationDtos, LookupDtos,
                                LeadDtos, InquiryDtos, ViewingDtos, PaymentPlanDtos, AnalyticsDtos, AuthDtos)
Services/RealEstate/I*Service.cs / *Service.cs  (Project, Inventory, Agent, Lead, Viewing, Auth)
Exceptions/RealEstateExceptions.cs  (RealEstateResourceNotFoundException, RealEstateConflictException)
```
Naming convention: every route under `/api/realestate/*`; every class prefixed `RealEstate*`; DTOs that would collide with existing names get the prefix too (e.g. `RealEstateAgentDto` vs. existing `AgentDto`).

**Endpoints (representative):** CRUD + filters on Projects (city/type/status/featured/price), Inventory (bedrooms/price/area/status/viewType, sort by price/area/date), Agents, Organizations; `GET /locations`, `GET /lookups?type=`; Leads CRUD + `PATCH .../status` + `POST .../activities` + `POST .../assign`; `POST /inquiries` (anonymous, rate-limited); `POST /viewings` (anonymous, rate-limited, transactionally upserts Customer+Lead+Viewing); `GET /analytics/leads-summary`, `/inventory-summary`; `POST /auth/login`, `/auth/refresh`.

**Auth:** add `Microsoft.AspNetCore.Authentication.JwtBearer`; wire `AddAuthentication().AddJwtBearer()` + `AddAuthorization` policies (`AdminOnly`, `AgentOrAdmin`) in `Program.cs`; insert `app.UseAuthentication()` before the existing `app.UseAuthorization()` (currently a no-op). Public `GET` catalog endpoints stay `[AllowAnonymous]`; writes require `AgentOrAdmin`; approvals/user-management require `AdminOnly`; `inquiries`/`viewings` POST stay anonymous but rate-limited.

**Validation/errors:** reuse `InvalidRequestException` with static allow-list arrays per enum-like field; add `RealEstateResourceNotFoundException`→404, `RealEstateConflictException`→409 as new catch clauses in `ExceptionHandlingMiddleware.cs`.

**Performance/security:** reuse existing paging-bounds validation pattern (max page size 100); `IMemoryCache` for Lookups/Locations/Amenities; `Microsoft.AspNetCore.RateLimiting` on `inquiries`/`viewings`/`auth/login`; replace hardcoded CORS origin array with config-driven `Cors:AllowedOrigins` (new `CorsOptions`, bound like `NapsterOptions`) covering Playground (5173/4173) + new frontend (5174) + prod placeholders.

**Modified:** `Program.cs` (DbContext DI, JWT auth, config-driven CORS, rate limiting), `NapsterAI.Api.csproj` (add `Microsoft.EntityFrameworkCore.SqlServer`/`.Design`/`.Tools`, `Microsoft.AspNetCore.Authentication.JwtBearer`), `appsettings*.json` (new `ConnectionStrings:RealEstateDb`, `Jwt`, `Cors` sections), `ExceptionHandlingMiddleware.cs`, `postman\NapsterAI.postman_collection.json`.

**Untouched:** every existing controller/DTO/service/exception, `database\RealEstateDB.sql` (kept as reference).

---

## 5. Frontend — new project `src\NapsterAI.RealEstate.Web`

Scaffold via `create-vite react-ts`, dev port 5174.

```
src/routes/public/  HomePage, SearchPage, ProjectDetailPage, UnitDetailPage, ContactPage, LandingPage
src/routes/admin/   AdminLayout, DashboardPage, ProjectsAdminPage, ProjectFormPage, InventoryAdminPage,
                     AgentsAdminPage, OrganizationsAdminPage, LeadsAdminPage, LeadDetailPage, ViewingsAdminPage
src/routes/auth/    LoginPage
src/routes/webmcp-demo/ WebSdkDemoPage
src/components/ui/       (shadcn-generated primitives)
src/components/layout/   PublicHeader, PublicFooter, AdminSidebar, AdminTopbar, RequireAuth
src/components/property/ PropertyCard, PropertyGrid, PropertyFilters, PriceRangeSlider
src/components/admin/    DataTable, KpiCard, LeadStatusBadge, BulkActionsToolbar
src/components/chat/     RealEstateChatWidget.tsx  (adapted from Playground's LiveChatPanel.tsx)
src/features/{projects,inventory,leads,auth}/use*.ts   (TanStack Query hooks)
src/api/client.ts, types.ts   (same fetch-wrapper + hand-mirrored-DTO convention as Playground, + Authorization header)
src/lib/utils.ts, queryClient.ts, auth-store.ts, edge-mcp/registerRealEstateTools.ts
.env.development, .env.example, .env.production
```

**shadcn/ui setup:** Tailwind + `npx shadcn@latest init` (generates `components.json`, `lib/utils.ts`, CSS-variable tokens light/dark); add components: button, card, dialog, dropdown-menu, table, form, input, select, badge, tabs, command, sheet, sonner, avatar, skeleton, pagination, separator, label, textarea, checkbox, switch, calendar, popover, navigation-menu, breadcrumb. Plus `react-hook-form`+`zod`, `@tanstack/react-table`, `recharts`, `lucide-react`. Shared by construction (one app, one `components/ui`) between admin and public.

**Public site:** homepage (hero search, featured carousel, CTA to AI assistant), URL-synced filterable search/grid, project/unit detail pages (gallery from `Media`, amenities, unit availability, inquiry/viewing forms), slug-driven landing pages, mobile filter drawer (shadcn `sheet`), skeleton loading states, empty-state components, toast-driven error handling, `react-helmet-async` for basic SEO (note: true SEO needs SSR — flagged as future work for this client-only SPA), Radix-driven accessibility.

**Admin dashboard:** KPI row (projects/units/leads/conversion/pipeline value/viewings this week) from `/analytics/*`; `DataTable` (TanStack Table) for Projects/Inventory with bulk actions; Agents/Orgs CRUD; Leads/Inquiries table + detail sheet with activity timeline + assign/convert/schedule actions; approval workflow UI (Agent-created listings default `PendingReview`, Admin approve/reject gates `IsActive`); Recharts analytics; RBAC-aware nav (Admin: full CRUD+approvals+user mgmt; Agent: own leads/viewings, read-only catalog) mirrored by backend `[Authorize]` policies (defense in depth).

---

## 6. WebSDK integration

`RealEstateChatWidget.tsx` directly adapts `LiveChatPanel.tsx`: same `NapsterCompanionApiSdk.init()` + `api.createConnection()` flow against the **existing, unmodified** `POST /api/connections` — no new session endpoint needed. Mounted as a floating widget in the public site's root layout. Requires a Napster companion configured as the "Real Estate AI Sales Agent" persona via the existing `POST /api/agents`. `/webmcp-demo` route shows a live tool-call log (name/args/result/latency) for QA. Example flow: customer asks "2BR under 2M AED in Dubai Marina" in chat → `searchInventory` tool call → chat summary **and** a live `re://search-results` resource push updates the visible search grid.

## 7. EdgeMCP integration

`src\lib\edge-mcp\registerRealEstateTools.ts` (called once from `App.tsx`) registers tools on `document.modelContext`; the existing `EdgeMcpBridge` inside `@touchcastllc/napster-companion-api` (already invoked by `NapsterCompanionApiSdk.init()`) auto-discovers them — no bridge code to write.

Tools: `searchInventory` (readOnly) → `GET /inventory`; `getProjectDetails` (readOnly) → `GET /projects/{id}`; `calculatePaymentPlan` (readOnly, idempotent) → `POST /inventory/{id}/payment-plan`; `bookViewing` (destructive) → `POST /viewings`; `createLead` (destructive) → `POST /inquiries`; `getAmenities` (readOnly) → `GET /projects/{id}/amenities`; `checkAvailability` (readOnly, idempotent) → `GET /inventory/{id}`. All run with anonymous-visitor privileges only — no admin tools exposed publicly. Test surface: `/webmcp-demo` page + a new Postman folder hitting the same endpoints directly (bypassing the AI loop) for backend-only verification.

---

## 8. Implementation phases

1. **DB & data layer** — EF Core packages, `RealEstateDbContext`, entity configs, initial migration (extends `RealEstateDB.sql`), dev seed, connection string.
2. **Backend core CRUD** (depends on 1) — Projects/UnitTypes/Inventory/Amenities/Locations/Lookups, new exceptions, Postman, xUnit (EF InMemory).
3. **Backend auth & RBAC** (depends on 2) — `Users` table+migration, JWT wiring, `AuthController`, policies, retrofit `[Authorize]`, config-driven CORS.
4. **Backend CRM layer** (depends on 3) — Organizations/SalesAgents/Customers/Leads/Inquiries/Viewings/LeadActivities/PaymentPlanMilestones, approval-workflow fields, analytics endpoints.
5. **Frontend foundation** (parallel with 3/4, needs stable Phase-2 DTOs) — scaffold project, Tailwind+shadcn init, layouts, router skeleton, TanStack Query, `api/client.ts`+`types.ts`, auth store, env files.
6. **Public website UI** (depends on 5, 2) — home/search/detail/contact/landing pages, responsive, states, SEO.
7. **Admin dashboard UI** (depends on 5, 3, 4) — KPIs, tables/forms, approvals UI, analytics, RBAC nav.
8. **WebSDK integration** (depends on 5) — chat widget, companion persona config.
9. **EdgeMCP tool wiring** (depends on 8, 2/4) — tool registration, resource push, demo page.
10. **Testing, polish, hardening** (spans throughout, final pass) — xUnit coverage, `WebApplicationFactory` integration tests, Postman coverage, accessibility, responsive QA, rate-limit/CORS/env hardening, README updates.

## 9. Testing & quality strategy

- **Controllers:** xUnit + Moq mocking service interfaces, matching `CompanionsControllerTests.cs` (assert `OkObjectResult` / exception propagation, no controller try/catch).
- **Services:** `Microsoft.EntityFrameworkCore.InMemory` (fresh DB name per test) — Moq doesn't cleanly mock `DbSet` LINQ.
- **Integration:** first real use of the existing `partial class Program` via `WebApplicationFactory<Program>` — happy-path + validation-error test per new controller group.
- **Postman:** new folder per resource + extended "Error Handling Examples" (400/404/409/401/403).
- **Frontend:** add Vitest + React Testing Library to the new project only (don't retrofit Playground); prioritize hooks and critical forms (lead/viewing) over exhaustive component coverage; manual/Playwright checklist for search→detail→book-viewing, admin login→approve listing, AI chat→`bookViewing` (live WebRTC voice isn't practical to automate — same gap `LiveChatPanel.tsx` already has).

---

### Critical files to know before starting

- `src\NapsterAI.Api\Program.cs` — DI/CORS/auth wiring point.
- `src\NapsterAI.Api\Services\NapsterService.cs` — validation/error/mapping idiom to replicate.
- `src\NapsterAI.Api\Middleware\ExceptionHandlingMiddleware.cs` — extension point for new exceptions.
- `database\RealEstateDB.sql` — source of truth for the initial EF Core migration.
- `src\NapsterAI.Playground\src\components\LiveChatPanel.tsx` — pattern for `RealEstateChatWidget.tsx` and EdgeMCP wiring.
- `src\NapsterAI.Playground\node_modules\@touchcastllc\napster-companion-api\lib\services\edge-mcp\edge-mcp-types.d.ts` — exact contract the 7 MCP tools must satisfy.

### Verification (once implemented, for the executing developer)

- `dotnet build` + `dotnet test` on `NapsterAI.Api`/`NapsterAI.Tests` after each backend phase.
- `dotnet ef database update` against a local SQL Server to confirm migrations apply cleanly from empty.
- Import the updated Postman collection and run each folder (incl. error-handling examples) against a running `dotnet run`.
- `npm run build` + `npm run dev` on the new frontend project; manually walk search→detail→inquiry/viewing, admin login→CRUD→approve, and the `/webmcp-demo` tool-call log against a live AI chat session.

# Connection String
Data Source=localhost;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name="SQL Server Management Studio";Command Timeout=0

# Database: RealEstateDB