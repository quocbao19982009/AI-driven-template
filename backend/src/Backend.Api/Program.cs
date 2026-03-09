using System.Reflection;
using Backend.Common.Extensions;
using Backend.Common.Middleware;
using Backend.Common.Swagger;
using Backend.Features.Users;
using Backend.Features._FeatureTemplate;
using Backend.Features.Todos;
using Backend.Features.Locations;
using Backend.Features.Rooms;
using Backend.Features.Bookings;
using Backend.Identity;
using Microsoft.OpenApi;
using Serilog;
using Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDatabase(builder.Configuration);

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<JwtService>();

// Validation
builder.Services.AddValidation();

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks();

// Feature services
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<ITodosRepository, TodosRepository>();
builder.Services.AddScoped<ITodosService, TodosService>();

// Locations
builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
builder.Services.AddScoped<ILocationsService, LocationsService>();

// Rooms
builder.Services.AddScoped<IRoomsRepository, RoomsRepository>();
builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

// Bookings
builder.Services.AddScoped<IBookingsRepository, BookingsRepository>();
builder.Services.AddScoped<IBookingsService, BookingsService>();

// Logging
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.SchemaFilter<RequiredSchemaFilter>();

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

builder.Services.AddControllers();

var app = builder.Build();

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure upload directory exists
var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "rooms"));
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger(nameof(DataSeeder));
    await DataSeeder.SeedAsync(db, seederLogger);
}

app.Run();
