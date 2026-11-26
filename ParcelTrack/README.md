# ParcelTrack (starter)

CWEB2302 Web Development II, Common Coursework (Individual).

## Run it
1. Open `ParcelTrack.csproj` in Visual Studio.
2. Package Manager Console: `Update-Database`.
3. F5 (the **http** profile, http://localhost:5420), then open `/parcels`. Five parcels show.

## Demo accounts (made when the app starts)
| Email | Password | Role |
|---|---|---|
| admin@parceltrack.test | Pass123! | Admin |
| staff@parceltrack.test | Pass123! | Staff |

## What the starter already does
- Parcels and Branches tables (a foreign key), seeded, one migration
- Pages: `/parcels`, `/parcels/{id}`, add, edit, delete, `/branches/{id}`
- Identity with Admin and Staff roles: Staff add and edit, only the Admin deletes
- A JSON API at `/api/parcels` (GET, POST, PUT, DELETE) with a DTO. **Not locked yet.**

## My work
**Part A — JWT Authentication & API Authorization:**
  * Added the `Jwt` configuration section (`Key`, `Issuer`, `Audience`) to `appsettings.json`.
  * Configured `JwtBearer` authentication in `Program.cs` validating the issuer, audience, token expiry, and signing key.
  * Implemented `POST /api/auth/login` (`Controllers/AuthApiController.cs`) to validate credentials, return `400` on malformed emails, return a generic `401` on invalid credentials, and issue a 60-minute JWT containing the user's Identity roles (`Admin` / `Staff`).
  * Secured `ParcelsApiController` write endpoints using `JwtBearerDefaults.AuthenticationScheme` so unauthenticated API calls return `401` instead of a `302` cookie login redirect: `POST` and `PUT` require the `CanManageParcels` policy (`Admin` or `Staff`), `DELETE` requires `Admin` (`403` for `Staff`), and both `GET` endpoints remain `[AllowAnonymous]`. Web pages retain cookie authentication.

* **Part B — Request ID & Logging Middleware:**
  * Created `Middleware/RequestLoggingMiddleware.cs` to generate an 8-character request ID on entry, attach it to the `X-Request-Id` response header before calling `_next`, and log `[id] METHOD /path -> STATUS in X ms` to the console on exit.
  * Registered in `Program.cs` after `UseStaticFiles()` (excluding CSS/JS/images) and before `UseRouting()` and `UseAuthentication()` (so `401` and `403` responses are logged).

* **Part C — `[RequireClient]` Action Filter:**
  * Created `Filters/RequireClientAttribute.cs` inheriting from `ActionFilterAttribute` and applied it to `POST`, `PUT`, and `DELETE` on `ParcelsApiController`.
  * Rejects requests missing a non-empty `X-Client` header with `400 Bad Request` (`{ "message": "Say which program is calling: add an X-Client header" }`) before the action executes.
  * Logs `X-Client: <client> -> <Action>` before execution and `<Action> finished` after execution.

* **Part D — Delivery Fee Calculator Service:**
  * Created `Services/IDeliveryFeeCalculator.cs` and `Services/DeliveryFeeCalculator.cs` implementing the pricing formula (`J$500` base + `J$150` per `Math.Ceiling(weightKg)` + `J$300` surcharge for branches outside Kingston and St Andrew).
  * Injected `IDeliveryFeeCalculator` into both `ParcelsController` and `ParcelsApiController` to display `Delivery fee: J$X,XXX.XX` on `/parcels/{id}` and include `deliveryFee` in `ParcelDto` across all API responses.
  * **Service Lifetime Choice:** `DeliveryFeeCalculator` is registered as a **Singleton** (`AddSingleton`) because it is completely stateless—it holds no per-request state or `DbContext` dependencies and performs a pure calculation, so reusing a single instance across the application avoids unnecessary memory allocations.

* **Part E — Custom `[TrackingCode]` Validation Attribute:**
  * Created `Validation/TrackingCodeAttribute.cs` inheriting from `ValidationAttribute` to enforce the `^PT-\d{6}$` format with the error message `"Tracking code must look like PT-123456"`, while allowing null/empty values so `[Required]` handles missing values independently.
  * Applied `[TrackingCode]` to `TrackingCode` in both `Parcel` (MVC forms) and `ParcelInput` (API requests).

* **Part F — End-to-End `.http` Test Suite:**
  * Updated `ParcelTrack.http` with `@host`, `@staff_token`, and `@admin_token` variables and all 10 sequential test requests with expected status codes and headers (`X-Client`) documented in `###` headers.

### Known Issues / Limitations
* None.     

Service Lifetime

Why Singleton is the best choice here:

• It is stateless: DeliveryFeeCalculator does not store any per-request data or user state—it simply takes weightKg and parish as inputs and returns a decimal.
• No Scoped dependencies: It does not depend on DbContext or any other Scoped service (injecting a Scoped service into a Singleton would cause a runtime error, which doesn't happen here).
• Efficiency: Creating a single shared instance once when the app starts avoids creating and garbage-collecting identical objects on every request.
