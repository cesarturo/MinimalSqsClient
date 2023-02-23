using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MinimalSqsClient;

public static class ServiceCollectionExtensions
{
    public static OptionsBuilder<SqsClientOptions> AddSqsClient(this IServiceCollection services, string? name = null)
    {
        name ??= Options.DefaultName;

        services.TryAddSingleton<ISqsClientFactory>(serviceProvider=>
            new SqsClientFactory(serviceProvider.GetRequiredService<IEnumerable<ISqsClient>>()));
        
        services.AddSingleton<ISqsClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<SqsClientOptions>>().Get(name);
            return new SqsClient(options, name);
        });
        
        return services.AddOptions<SqsClientOptions>(name);
    }
}