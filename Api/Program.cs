using Microsoft.EntityFrameworkCore;
using OklahomaTaxEngine.Data;
using OklahomaTaxEngine.Services;
using OklahomaTaxEngine.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Oklahoma Tax Engine API", 
        Version = "v1",
        Description = "A comprehensive tax management system for Oklahoma Tax Commission",
        Contact = new() { Name = "Your Name", Email = "your.email@example.com" }
    });
});

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<IRuleEngine, RuleEngine>();
builder.Services.AddScoped<ITaxCalculationService, TaxCalculationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oklahoma Tax Engine API v1");
        c.RoutePrefix = "api-docs";
    });
}

// IMPORTANT: Static files middleware must come BEFORE other middleware
app.UseDefaultFiles(); // This enables serving default files like index.html
app.UseStaticFiles();  // This enables serving static files from wwwroot

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Add a fallback route for the root
app.MapGet("/", () => Results.Redirect("/index.html"));

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        
        // Seed initial data if needed
        if (!dbContext.TaxRules.Any())
        {
            SeedDatabase(dbContext);
            Console.WriteLine("Database seeded successfully!");
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating/seeding the database.");
    }
}

Console.WriteLine($"Application started. Access the application at:");
Console.WriteLine($"  - http://localhost:{builder.Configuration["ASPNETCORE_URLS"]?.Split(';')[0].Split(':').Last() ?? "5000"}");
Console.WriteLine($"  - http://localhost:{builder.Configuration["ASPNETCORE_URLS"]?.Split(';')[0].Split(':').Last() ?? "5000"}/api-docs (API Documentation)");

app.Run();

// Seed method for initial data
void SeedDatabase(AppDbContext context)
{
    // Add sample tax rules
    var taxRules = new[]
    {
        new TaxRule
        {
            TaxType = "Income",
            RuleName = "OK State Income Tax - Bracket 1",
            MinAmount = 0,
            MaxAmount = 1000,
            Rate = 0.005m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 1,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Income",
            RuleName = "OK State Income Tax - Bracket 2",
            MinAmount = 1000.01m,
            MaxAmount = 2500,
            Rate = 0.01m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 2,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Income",
            RuleName = "OK State Income Tax - Bracket 3",
            MinAmount = 2500.01m,
            MaxAmount = 3750,
            Rate = 0.02m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 3,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Income",
            RuleName = "OK State Income Tax - Bracket 4",
            MinAmount = 3750.01m,
            MaxAmount = 4900,
            Rate = 0.03m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 4,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Income",
            RuleName = "OK State Income Tax - Bracket 5",
            MinAmount = 4900.01m,
            MaxAmount = 7200,
            Rate = 0.04m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 5,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Income",
            RuleName = "OK State Income Tax - Bracket 6",
            MinAmount = 7200.01m,
            MaxAmount = null,
            Rate = 0.05m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 6,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Sales",
            RuleName = "OK State Sales Tax",
            MinAmount = 0,
            MaxAmount = null,
            Rate = 0.045m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 1,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Property",
            RuleName = "OK Property Tax - Residential",
            MinAmount = 0,
            MaxAmount = null,
            Rate = 0.012m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 1,
            IsActive = true
        },
        new TaxRule
        {
            TaxType = "Corporate",
            RuleName = "OK Corporate Income Tax",
            MinAmount = 0,
            MaxAmount = null,
            Rate = 0.06m,
            EffectiveFrom = new DateTime(2024, 1, 1),
            Priority = 1,
            IsActive = true
        }
    };

    context.TaxRules.AddRange(taxRules);

    // Add sample taxpayers
    var taxpayers = new[]
    {
        new TaxPayer
        {
            TaxId = "OK123456789",
            Name = "Sample Corporation",
            Type = "Corporate",
            Email = "contact@samplecorp.com",
            Phone = "405-555-0100",
            Address = "123 Main Street",
            City = "Oklahoma City",
            State = "OK",
            ZipCode = "73102",
            RegistrationDate = DateTime.Now,
            IsActive = true
        },
        new TaxPayer
        {
            TaxId = "OK987654321",
            Name = "Demo Individual Taxpayer",
            Type = "Individual",
            Email = "demo@example.com",
            Phone = "405-555-0123",
            Address = "456 Demo Street",
            City = "Stillwater",
            State = "OK",
            ZipCode = "74074",
            RegistrationDate = DateTime.Now,
            IsActive = true
        }
    };

    context.TaxPayers.AddRange(taxpayers);
    context.SaveChanges();
}