using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ───── Services ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OneLink API", Version = "v1" });
});

// ───── Pipeline ───────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // RoutePrefix = "" → Swagger UI at https://localhost:5001 (root)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OneLink API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();     // redirects :5000 → :5001 automatically
app.UseAuthorization();
app.MapControllers();

app.Run();
