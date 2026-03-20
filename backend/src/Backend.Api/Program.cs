using System.Reflection;
using System.Text.Json;
using Backend.Common.Extensions;
using Backend.Common.Middleware;
using Backend.Common.Swagger;
using Backend.Data;
using Backend.Features._FeatureTemplate;
using Backend.Features.Auth;
using Backend.Features.Bookings;
using Backend.Features.Locations;
using Backend.Features.Rooms;
using Backend.Features.Users;
using Backend.Identity;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDatabase(builder.Configuration);

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<JwtService>();

// Validation
builder.Services.AddValidation();
builder.Services.AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// Rate limiting
builder.Services.AddRateLimiting();

// Health checks
builder.Services.AddHealthChecks();

// Feature services
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
builder.Services.AddScoped<ILocationsService, LocationsService>();
builder.Services.AddScoped<IRoomsRepository, RoomsRepository>();
builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddScoped<IBookingsRepository, BookingsRepository>();
builder.Services.AddScoped<IBookingsService, BookingsService>();

// Logging
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.SchemaFilter<RequiredSchemaFilter>();
    options.DocumentFilter<FluentValidationPatternMergeFilter>();
    options.OperationFilter<CamelCaseQueryParameterFilter>();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend API",
        Version = "v1",
        Description = "A simple ASP.NET Core Web API template with best practices for building scalable and maintainable applications."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

// Middleware
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        return Task.CompletedTask;
    });
    await next();
});

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "Backend API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger(nameof(DataSeeder));
    var seederConfig = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DataSeeder.SeedAsync(db, seederLogger, seederConfig);
}

app.Run();
