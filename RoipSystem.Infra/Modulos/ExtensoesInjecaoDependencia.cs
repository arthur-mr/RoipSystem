using Microsoft.Extensions.DependencyInjection;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Servicos.Servicos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Interfaces;
using RoipSystem.Framework.RabbitMq;
using RoipSystem.Infra.Persistencia;

namespace RoipSystem.Infra.Modulos;

public static class ExtensoesInjecaoDependencia
{
    public static IServiceCollection AddRoipInfrastructure(
        this IServiceCollection services,
        Action<RabbitMqOpcoes> configurarOpcoes)
    {
        services.Configure(configurarOpcoes);

        services.AddSingleton<FabricaConexaoRabbitMq>();

        services.AddSingleton<IPublicadorMensagem>(sp =>
        {
            var fabrica = sp.GetRequiredService<FabricaConexaoRabbitMq>();
            var opcoes = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOpcoes>>().Value;
            return new PublicadorRabbitMq(fabrica, opcoes.ExchangePrincipal);
        });


        services.AddSingleton<IRepositorioAuditoria, RepositorioAuditoriaMock>();

        services.AddSingleton<IEventoRadioServico, EventoRadioServico>();

        return services;
    }
}
