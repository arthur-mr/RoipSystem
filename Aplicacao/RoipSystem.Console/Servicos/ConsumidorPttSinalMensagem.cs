using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoipSystem.Dominio.Contratos;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Framework.RabbitMq;

namespace RoipSystem.Console.Servicos;

public sealed class ConsumidorPttSinalMensagem : ConsumidorRabbitMqBase<PttSinalMensagem>
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<ConsumidorPttSinalMensagem> logger;
    private readonly string usuarioLocal;

    public ConsumidorPttSinalMensagem(
        FabricaConexaoRabbitMq fabricaConexao,
        ConfiguracaoMensageria configuracaoMensageria,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ConsumidorPttSinalMensagem> logger,
        string usuarioLocal)
        : base(fabricaConexao, configuracaoMensageria, logger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
        this.usuarioLocal = usuarioLocal;
    }

    protected override async Task<bool> ProcessarMensagemAsync(PttSinalMensagem mensagem, CancellationToken cancellationToken)
    {
        if (string.Equals(mensagem.Usuario, usuarioLocal, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("[Sinalização] Ignorando evento próprio de {Usuario}", mensagem.Usuario);
            return true;
        }

        if (!Enum.TryParse<PttEvento>(mensagem.Evento, out var evento))
        {
            logger.LogWarning("[Sinalização] Evento desconhecido: {Evento}", mensagem.Evento);
            return true;
        }

        var contrato = new PttSinalContrato
        {
            Evento = evento,
            Usuario = mensagem.Usuario,
            IpOrigem = mensagem.IpOrigem,
            PortaUdp = mensagem.PortaUdp
        };

        logger.LogInformation("[Sinalização] Recebido {Evento} de {Usuario} (IP: {Ip}:{Porta})",
            evento, mensagem.Usuario, mensagem.IpOrigem, mensagem.PortaUdp);

        using var escopo = serviceScopeFactory.CreateScope();
        var mediator = escopo.ServiceProvider.GetRequiredService<IMediator>();
        var comando = new ProcessarSinalPttComando(contrato);

        await mediator.Send(comando, cancellationToken);

        return true;
    }
}
