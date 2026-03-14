using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Routsky.Api.Data;
using Routsky.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.AddPolicy("AllowAll", policy =>
            policy.WithOrigins("https://routsky.com", "https://routsky.xyz")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials());
    }
    else
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    }
});

// ── Database ──
builder.Services.AddDbContext<RoutskyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();

// ── In-Memory Cache ──
builder.Services.AddMemoryCache();

// ── Travel Buddy API (Visa) ──
builder.Services.AddHttpClient<TravelBuddyApiService>();
builder.Services.AddSingleton<TravelBuddyApiService>();

// ── Flight Price Providers (Hybrid: Turkish Airlines + Gemini) ──
builder.Services.AddHttpClient<TurkishAirlinesFlightPriceProvider>();
builder.Services.AddSingleton<TurkishAirlinesFlightPriceProvider>();
builder.Services.AddScoped<GeminiFlightPriceProvider>();
builder.Services.AddScoped<HybridFlightPriceService>();

// ── MCP Decision Services (Agent-as-Orchestrator) ──
builder.Services.AddScoped<RouteFeasibilityService>();
builder.Services.AddScoped<BudgetConsistencyService>();
builder.Services.AddScoped<TimeOverlapService>();
builder.Services.AddScoped<DecisionSolverService>();
builder.Services.AddHttpClient<AgentInsightService>();

// ── Semantic Kernel ──
var geminiKey = builder.Configuration["Gemini:ApiKey"];
if (string.IsNullOrWhiteSpace(geminiKey) || geminiKey == "mock-key")
    throw new InvalidOperationException(
        "FATAL: Gemini:ApiKey is not configured. Set the GEMINI_API_KEY environment variable or Gemini__ApiKey in your configuration.");

builder.Services.AddKernel()
    .AddGoogleAIGeminiChatCompletion(
        modelId: "gemini-2.5-flash",
        apiKey: geminiKey);

// ── JWT Authentication ──
var key = System.Text.Encoding.ASCII.GetBytes(
    builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForDevelopmentOnly123!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie() // Required for temporary social auth storage
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:Github:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Github:ClientSecret"] ?? "";
});
// .AddApple(options =>
// {
//     options.ClientId = builder.Configuration["Authentication:Apple:ClientId"] ?? "";
//     options.TeamId = builder.Configuration["Authentication:Apple:TeamId"] ?? "";
//     options.KeyId = builder.Configuration["Authentication:Apple:KeyId"] ?? "";
//     options.PrivateKey = async (keyId, cancellationToken) =>
//     {
//         var privateKeyPath = builder.Configuration["Authentication:Apple:PrivateKey"] ?? "";
//         return await System.IO.File.ReadAllTextAsync(privateKeyPath, cancellationToken);
//     };
// });

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Allow React to send enum values as strings (e.g. "Budget", "SoutheastAsia")
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Routsky API", Version = "v2" });
});

var app = builder.Build();

// ── V2 Database Seed ──
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RoutskyDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the V2 database.");
    }
}

// ── Pipeline ──
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async ctx =>
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = 500;
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        if (feature?.Error is not null)
        {
            var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
            logger.LogError(feature.Error, "Unhandled exception [{CorrelationId}]", correlationId);
        }
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = "Internal Server Error",
            correlationId,
            message = app.Environment.IsDevelopment() ? feature?.Error?.Message : "An unexpected error occurred."
        }));
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
