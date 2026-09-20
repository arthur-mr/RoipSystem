using MediatR;
using Microsoft.Extensions.Options;
using RoipSystem.Dominio.Contratos;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Framework.RabbitMq;

namespace RoipSystem.Aplicacao.DispatchWorker;

public sealed class ConsumidorDispatch(
    FabricaConexaoRabbitMq fabricaConexao,
    ConfiguracaoMensageria configuracaoMensageria,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ConsumidorDispatch> logger) : ConsumidorRabbitMqBase<EventoRadioMensagem>(fabricaConexao, configuracaoMensageria, logger)
{
    protected override async Task<bool> ProcessarMensagemAsync(EventoRadioMensagem mensagem, CancellationToken cancellationToken)
    {
        using var escopo = serviceScopeFactory.CreateScope();
        var mediator = escopo.ServiceProvider.GetRequiredService<IMediator>();

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