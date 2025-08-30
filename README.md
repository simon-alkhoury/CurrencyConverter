# Currency Converter API

A robust, production-ready currency conversion API built with ASP.NET Core (.NET 9, C# 13).  
It provides endpoints for latest rates, currency conversion, and historical rates, with caching, paging, resilience, and JWT authentication.

---

## Setup Instructions

1. **Clone the repository**
git clone https://github.com/simon-alkhoury/currency-converter-api.git

2**Install .NET 9 SDK**
   - Download and install from [dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)

3. **Restore NuGet packages**

4. **Configure environment**
   - Update `appsettings.json` with your JWT secret, issuer, audience, and any other settings.
   - Optionally configure Serilog, OpenTelemetry, and rate limiting as needed.

5. **Run the API**
  dotnet run --project CurrencyConverter.Api
- The API will be available at `https://localhost:5098` (or as configured).

6. **Run unit tests**
 - dotnet test


7. **Swagger UI**
- Access interactive API docs at `/swagger` when running in development.

---

## Assumptions Made

- The Frankfurter API (`https://api.frankfurter.app/`) is available and reliable for currency data.
- Caching is in-memory and suitable for single-instance deployments.
- Paging for historical rates is handled in-memory after fetching all data for the requested range.
- JWT authentication is required for all endpoints; valid tokens must be provided.
- Excluded currencies are hardcoded in the service and not configurable at runtime.
- The API is intended for demonstration and small-scale production use; distributed cache and scaling are not implemented.
- Rate limiting is global and not per-user or per-endpoint.
- The test users and roles are hardcoded for demonstration purposes.

---

## Possible Future Enhancements

- **Distributed Caching:** Use Redis or similar for multi-instance deployments.
- **Configurable Excluded Currencies:** Move exclusions to configuration or database.
- **Rate Limiting per Endpoint/User:** More granular controls and analytics.
- **API Key Support:** In addition to JWT, allow API key authentication for external clients.
- **Currency Symbols and Metadata:** Enrich responses with currency names, symbols, and flags.
- **Performance Optimization:** Stream/process large historical datasets instead of loading all into memory.
- **Localization:** Support for localized currency names and error messages.
- **Docker Support:** Provide Dockerfile and docker-compose for easy containerization.
- **CI/CD Integration:** Add GitHub Actions or Azure DevOps pipelines for automated testing and deployment.
- **More Unit/Integration Tests:** Increase coverage, especially for edge cases and error handling.
- **User Management:** Integrate with a real user store and role management.

