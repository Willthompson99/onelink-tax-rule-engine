# OneLink Tax-Rule Engine API

C# / .NET 8 API that simulates configurable tax-rule processing (think GenTax / OneLink).  
A local SQL Server backs the data; Swagger is enabled out-of-the-box.

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ | Build & run the API |
| SQL Server | 2019+ (Developer or Express) | Local database |

```bash
# one-time (first machine only)
dotnet dev-certs https --trust
