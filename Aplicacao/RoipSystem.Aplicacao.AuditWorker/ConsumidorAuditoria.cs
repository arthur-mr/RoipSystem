using MediatR;
using Microsoft.Extensions.Options;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Dominio.Mediador.Contratos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Framework.RabbitMq;

namespace RoipSystem.Aplicacao.AuditWorker;

public sealed class ConsumidorAuditoria : ConsumidorRabbitMqBase<MensagemVozAudit>
{
    private readonly IMediator mediator;
    private readonly ILogger<ConsumidorAuditoria> logger;

    public ConsumidorAuditoria(
        FabricaConexaoRabbitMq fabricaConexao,
        IOptions<RabbitMqOpcoes> opcoes,
        IMediator mediator,
        ILogger<ConsumidorAuditoria> logger)
        : base(fabricaConexao, opcoes, logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    protected override async Task<bool> ProcessarMensagemAsync(MensagemVozAudit mensagem, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TipoEvento>(mensagem.TipoEvento, out var tipoEvento))
        {
            logger.LogError("TipoEvento inválido recebido: '{TipoEvento}'. Descartando para DLQ.", mensagem.TipoEvento);
            return false;
        }

        var contrato = new EventoRadioContrato(
            mensagem.Id,
            mensagem.RadioId,
            mensagem.TalkgroupId,
            tipoEvento,
            mensagem.DataHora,
            mensagem.Latitude,
            mensagem.Longitude);

        var comando = new SalvarAuditoriaComando(contrato);

        await mediator.Send(comando, cancellationToken);
        return true;
    }
}
