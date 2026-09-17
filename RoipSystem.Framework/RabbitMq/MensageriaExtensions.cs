using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Interfaces;
using System.Reflection;

namespace RoipSystem.Framework.RabbitMq;

public static class MensageriaExtensions
{
    public static IServiceCollection AdicionarMensageria(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        var configMensageria = configuration.GetSection("RabbitMq").Get<ConfiguracaoMensageria>() ?? new ConfiguracaoMensageria();
        services.AddSingleton(configMensageria);

        var tipoConsumidor = typeof(IConsumidor);
        
        foreach (var assembly in assemblies)
        {
            var consumidores = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && tipoConsumidor.IsAssignableFrom(t));

            foreach (var consumidor in consumidores)
            {
                services.AddSingleton(tipoConsumidor, consumidor);
            }
        }

        services.AddSingleton<FabricaConexaoRabbitMq>();
        services.AddScoped<IPublicadorMensagem>(sp => 
        {
            var fabrica = sp.GetRequiredService<FabricaConexaoRabbitMq>();
            var cfg = sp.GetRequiredService<ConfiguracaoMensageria>();
            return new PublicadorRabbitMq(fabrica, cfg.ExchangePrincipal);
        });

        services.AddHostedService<ProcessoMensageria>();

        return services;
    }
}
