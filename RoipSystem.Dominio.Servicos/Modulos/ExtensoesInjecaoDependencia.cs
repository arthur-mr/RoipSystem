using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Servicos.Servicos;
using RoipSystem.Infra.Modulos;

namespace RoipSystem.Dominio.Servicos.Modulos;

public static class ExtensoesInjecaoDependencia
{
    public static IServiceCollection AddRoipServicos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRoipInfrastructure(configuration);
        services.AddScoped<IEventoRadioServico, EventoRadioServico>();
        return services;
    }
}
