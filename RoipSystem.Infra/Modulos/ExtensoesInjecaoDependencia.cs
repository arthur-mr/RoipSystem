using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Infra.Persistencia;

namespace RoipSystem.Infra.Modulos;

public static class ExtensoesInjecaoDependencia
{
    public static IServiceCollection AddRoipInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IRepositorioAuditoria, RepositorioAuditoriaMock>();
        return services;
    }
}
