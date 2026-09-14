using MediatR;
using Microsoft.Extensions.Options;
using RoipSystem.Dominio.Contratos;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Framework.RabbitMq;

namespace RoipSystem.Aplicacao.DispatchWorker;

public abstract class ConsumidorDispatch<T>(
    FabricaConexaoRabbitMq fabricaConexao,
    IOptions<RabbitMqOpcoes> opcoes,
    IMediator mediator,
    ILogger<ConsumidorDispatch<T>> logger) : ConsumidorRabbitMqBase<T>(fabricaConexao, opcoes, logger) where T : EventoRadioMensagem
{
    protected override async Task<bool> ProcessarMensagemAsync(T mensagem, CancellationToken cancellationToken)
    {
        if (mensagem.RadioId == "FALHA")
        {
            logger.LogWarning("Simulando falha proposital para testar resiliência (DLQ/Retries) para o RadioId: FALHA.");
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

        var comando = new ProcessarEventoRadioComando(contrato);

        await mediator.Send(comando, cancellationToken);
        
        // Simula tempo de processamento para permitir acúmulo na fila e teste de prioridade
        await Task.Delay(20, cancellationToken);
        
        return true;
    }
}