using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Promotions.Domain.Services;
using Promotions.Infrastructure;

// Isolated-worker Functions host (built-in HttpRequestData model). Configuration
// is layered from environment variables / local.settings.json, so the "Azure:*"
// keys bind to AzureOptions.
var builder = FunctionsApplication.CreateBuilder(args);

// Observability — Application Insights for the isolated worker.
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Reuse the exact same Azure infrastructure registrations as the API.
builder.Services.AddPromotionsInfrastructure(builder.Configuration);
builder.Services.AddScoped<PromotionService>();

builder.Build().Run();
