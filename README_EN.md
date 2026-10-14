# ServiceReport

[简体中文](README.md) | English

ServiceReport is an ASP.NET Core request-monitoring project with collection middleware, batched HTTP reporting, MySQL storage, and a Blazor Server dashboard built with Ant Design. It provides request, timing, and exception information for service troubleshooting and business analysis.

> The source targets .NET 7. Some features and deployment prerequisites remain incomplete; see “Current status and limitations.” The commands below are derived from the project files and have not been build-tested or verified end to end.

![ServiceReport dashboard](https://user-images.githubusercontent.com/86709205/222441329-28940016-ae86-462a-b28d-40b795b7f24e.png)

## Features and data flow

- Collects HTTP paths, methods, status codes, elapsed time, service identifiers, and exception details.
- Generates a request identifier and returns it in a response header to help locate individual requests.
- Buffers request data in process and sends it over HTTP using configurable intervals and batch sizes.
- Stores requests and related data through Entity Framework Core and the Pomelo MySQL provider.
- Provides summary cards, charts, request lists and details, and time/service filtering controls.
- Includes code for monitoring tasks, alerts, and GC metrics; their implementation status is described below.

Data flow: application API → ServiceReport middleware → HTTP sender → collection endpoints in the UI host → MySQL → Blazor pages.

The collector accepts `POST /Collection/CollectionRequest` and `POST /Collection/GcMonitor`. The sender appends endpoint paths to `PushAddress`, so keep its trailing `/`.

## Repository layout

```text
ServiceReport.sln
src/
├── ServiceReport/                    # ASP.NET Core middleware and background reporting
├── ServiceReport.Core/               # Request models, builders, and options
├── ServiceReport.Saver.Abstract/     # Storage and sender interfaces
├── ServiceReport.Saver.Http/         # HTTP sender and in-memory buffer
└── ServiceReport.Ui/
    ├── ServiceReport.Ui.Core/        # Registration, collection endpoints, scheduling
    ├── ServiceReport.Ui.Storage/     # EF Core DbContext and MySQL configuration
    ├── ServiceReport.Ui.Web.Core/    # Blazor pages and components
    └── ServiceReport.Ui.Web/         # UI host referencing source projects
test/
├── ApiService/                      # Example API integrated with ServiceReport
├── Console/                         # Placeholder console example
└── UI/                              # Separate example using HttpReports packages
tmp/
└── ServiceReport.Ui.Web/             # UI host template referencing NuGet packages
```

`test/UI` uses `HttpReports.Dashboard` and `HttpReports.MySQL`; it is not the ServiceReport UI used in this guide. The correct template location is [`tmp/ServiceReport.Ui.Web`](tmp/ServiceReport.Ui.Web), not `tem/ServiceReport.Ui.Web`.

## Requirements

- A .NET SDK capable of building `net7.0` projects. All committed C# projects target `net7.0`.
- Access to the required NuGet package sources.
- A MySQL database with a schema matching the [`ApplicationContext`](src/ServiceReport.Ui/ServiceReport.Ui.Storage/ApplicationContext.cs) models.
- A locally trusted ASP.NET Core development certificate when using the example HTTPS URLs.

The storage project references EF Core `7.0.2` and Pomelo MySQL `7.0.0`. Its provider configuration hard-codes `ServerVersion.Parse("5.5")`. This is an implementation setting, not a verified compatibility statement for every MySQL release; check it against your database before connecting.

**Database setup gap:** No EF Core migrations or SQL initialization scripts are committed, and the inspected startup path does not create tables automatically. A connection string alone does not complete a fresh deployment. Prepare and verify the schema first; this guide does not provide an unverified automatic initialization command.

## Run from source

### 1. Get the code and restore dependencies

```bash
git clone https://github.com/HaoQianCheng/ServiceReport.git
cd ServiceReport
dotnet restore src/ServiceReport.Ui/ServiceReport.Ui.Web/ServiceReport.Ui.Web.csproj
dotnet restore test/ApiService/ApiService.csproj
```

These commands restore the source-based UI and sample API individually, keeping the package-based template and other examples separate.

### 2. Configure the UI database

Set `MySqlOption:ConnectionString` in the UI host's configuration. The following shows the key and placeholder format only. Supply real values through environment variables or local secret configuration, and do not commit credentials:

```json
{
  "MySqlOption": {
    "ConnectionString": "server=localhost;port=3306;database=servicereport;uid=YOUR_USER;pwd=YOUR_PASSWORD;CharSet=utf8"
  }
}
```

The corresponding environment variable is `MySqlOption__ConnectionString`. Once the connection and schema are ready, start the source-based UI:

```bash
dotnet run --project src/ServiceReport.Ui/ServiceReport.Ui.Web/ServiceReport.Ui.Web.csproj --launch-profile ServiceReport.Ui.Web
```

The profile declares `https://localhost:5003` and `http://localhost:5004`. Use the actual listening addresses printed by the application.

### 3. Configure and start the sample API

Edit `ServiceReportOptions` in [`test/ApiService/appsettings.json`](test/ApiService/appsettings.json) so that `PushAddress` points to the running UI. A complete example is provided in the next section. Then run this in another terminal:

```bash
dotnet run --project test/ApiService/ApiService.csproj --launch-profile https
```

This profile declares `https://localhost:7082` and `http://localhost:5169`. Open `https://localhost:7082/swagger`, call the sample endpoint, and inspect the request data in the UI. If you change listening ports, also review `PushAddress` and `Server`.

## Integrate an ASP.NET Core application

### Installation and registration

The original documentation provides this NuGet installation example:

```powershell
NuGet\Install-Package ServiceReport -Version 1.0.0
```

This is a historical package example. The current source version in [`ServiceReport.csproj`](src/ServiceReport/ServiceReport.csproj) is `1.0.1`, which does not confirm that the same version is published to a package feed. Verify package availability and dependencies before using NuGet. To work against current source, follow the project-reference approach used by `test/ApiService`.

Add service registration and middleware to your existing `Program.cs`:

```csharp
using ServiceReport.Saver.Http;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddServiceReportModule()
    .AddServiceReportSendHttpModule();

var app = builder.Build();
app.UseHttpReports();
app.MapControllers();
app.Run();
```

This is a minimal integration example. Adapt authentication, authorization, exception handling, and middleware ordering to your application.

![Original integration example](https://user-images.githubusercontent.com/86709205/222441354-7b4765c2-980c-4034-8d4c-6fcec0473aef.png)

### Instrumented application configuration

Merge this section into your application's `appsettings.json`. This configuration is intended for local testing. Use synthetic traffic without sensitive data, and read the collection limitations below first.

```json
{
  "ServiceReportOptions": {
    "ServiceReportSendOptions": {
      "PushAddress": "https://localhost:5003/",
      "PushSecond": 1,
      "PushCount": 1000
    },
    "Server": "http://localhost:5169/",
    "ServiceName": "ApiServer",
    "RecordRequest": true,
    "RecordResponse": true,
    "RecordCookie": false,
    "RecordHeader": false,
    "RequestFilter": []
  }
}
```

| Setting | Meaning and caveats |
| --- | --- |
| `ServiceReportSendOptions.PushAddress` | Base URL of the UI/collector host; keep the trailing `/`. |
| `ServiceReportSendOptions.PushSecond` | Delay in seconds after each request-reporting iteration, not a records-per-second rate. Use a positive value. |
| `ServiceReportSendOptions.PushCount` | Maximum records selected for a batch. Use a positive value. |
| `Server` | Absolute URI used to derive the service instance identifier. It does not configure the application's listening address. |
| `ServiceName` | Service name displayed in the UI. |
| `RecordRequest` / `RecordResponse` | Body-related options. In the current implementation, `false` is not a reliable way to disable body capture. |
| `RecordCookie` | Declared in the options class, but the current request builder does not use it to persist a cookie field. |
| `RecordHeader` | Controls request-header serialization. Enabling it can include cookies, authorization values, or other sensitive content. |
| `RequestFilter` | Path exclusions. The current check is case-insensitive `configuredEntry.Contains(requestPath)`, not wildcard matching or conventional path-prefix matching. |

### Use the standalone UI template

Copy [`tmp/ServiceReport.Ui.Web`](tmp/ServiceReport.Ui.Web) to use a package-based UI host. It references version `1.0.0` of `ServiceReport.Ui.Core` and `ServiceReport.Ui.Web.Core`; verify availability in your package feed separately. The database prerequisites and configuration are the same as for the source UI. Do not run both hosts on the same default ports.

![Original UI template example](https://user-images.githubusercontent.com/86709205/222441397-88ff73c5-f73c-4067-9152-72853dc1eef8.png)

## Current status and limitations

- **Collection privacy:** The body conditions in `HttpResolver` do not stop body reading simply because `RecordRequest` or `RecordResponse` is `false`. Query strings are also stored. Review and correct capture logic, sensitive-field redaction, and retention before using real data.
- **Exceptions and responses:** The resolver buffers response bodies and catches downstream exceptions without rethrowing them. Check compatibility with your exception handling, large responses, and streaming endpoints.
- **Routes and gRPC:** The root path and paths containing `.` bypass collection. The gRPC branch is unimplemented and does not invoke the next middleware; do not use it directly for gRPC services.
- **Reliability and metrics:** Pending data is held in process, without a durable queue. The actual collection call in the GC background service is commented out, so GC/CPU monitoring should not be considered enabled.
- **Alerts:** Monitoring-task pages and mail-related classes exist, but `MonitorBackgroundJob.Execute` does not implement alert evaluation, and the email body still contains a TODO.
- **Access control:** The current UI startup and collection path do not configure authentication/authorization protection. Add access control, network isolation, and transport security before deployment; do not expose the host directly to the public internet.
- **Verification:** `test/` primarily contains sample applications; no automated test project was found. This documentation update did not run builds, tests, database initialization, or NuGet availability checks.

## Contributing and license

Report reproducible problems through [Issues](https://github.com/HaoQianCheng/ServiceReport/issues), or open a Pull Request with a clear description. Remove credentials and real request data before submitting, and include the .NET/database versions and reproduction steps.

The repository currently contains no license file. Confirm permission with the maintainer before using, modifying, or redistributing the code.
