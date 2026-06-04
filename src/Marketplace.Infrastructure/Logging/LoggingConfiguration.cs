using Microsoft.Extensions.Configuration;
using Serilog;

namespace Marketplace.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static void Configure(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
    }
}
