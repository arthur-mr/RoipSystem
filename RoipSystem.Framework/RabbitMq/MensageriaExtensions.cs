using Microsoft.Extensions.DependencyInjection;
using RoipSystem.Framework.Interfaces;
using System.Linq;
using System.Reflection;

namespace RoipSystem.Framework.RabbitMq;

public static class MensageriaExtensions
{
    public static IServiceCollection AdicionarMensageria(this IServiceCollection services, params Assembly[] assemblies)
    {
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

        services.AddHostedService<ProcessoMensageria>();

        return services;
    }
}
