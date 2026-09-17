using MediatR;
using RoipSystem.Dominio.Contratos;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Framework.RabbitMq;

namespace RoipSystem.Aplicacao.AuditWorker;

public sealed class ConsumidorAuditoria(
    FabricaConexaoRabbitMq fabricaConexao,
    ConfiguracaoMensageria configuracaoMensageria,
    IMediator mediator,
    ILogger<ConsumidorAuditoria> logger) : ConsumidorRabbitMqBase<MensagemVozAudit>(fabricaConexao, configuracaoMensageria, logger)
{
    protected override async Task<bool> ProcessarMensagemAsync(MensagemVozAudit mensagem, CancellationToken cancellationToken)
    {
        if (mensagem.RadioId == "FALHA")
        {
            logger.LogWarning("Simulando falha proposital de auditoria para testar resiliência (DLQ/Retries) para o RadioId: FALHA.");
            return false;
        }

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
