using Microsoft.Extensions.Configuration;

namespace test.Infrastructure.Common;
public static class ConfigurationExtensions
{
    public static string ServerInfo(this IConfiguration configuration)
    {
        return configuration.GetSection("DbServerInfo").GetValue<string>("ServerInfo");
    }
}
