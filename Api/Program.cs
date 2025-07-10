using Microsoft.EntityFrameworkCore;
using OklahomaTaxEngine.Data;
using OklahomaTaxEngine.Services;

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

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseDefaultFiles();

app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Seed initial data if needed
    if (!dbContext.TaxRules.Any())
    {
        SeedDatabase(dbContext);
    }
}

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

    // Add sample taxpayer
    var taxpayer = new TaxPayer
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
    };

    context.TaxPayers.Add(taxpayer);
    context.SaveChanges();
}