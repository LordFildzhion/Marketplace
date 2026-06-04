using Marketplace.Infrastructure.Extensions;
using Marketplace.Application.Extensions;
using Marketplace.Web.Extensions;
using Marketplace.Web.Middleware;
using Marketplace.Web.Filters;
using Serilog;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Marketplace.IntegrationTests")]
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Marketplace")
    .WriteTo.Console());


builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddMarketplaceAuthorization();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddMarketplaceHealthChecks(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.Services.MigrateAndSeedAsync();

app.Run();

public partial class Program { }
