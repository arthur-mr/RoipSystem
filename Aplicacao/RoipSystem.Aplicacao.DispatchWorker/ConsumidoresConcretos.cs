using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Framework.RabbitMq;

namespace RoipSystem.Aplicacao.DispatchWorker;

public sealed class ConsumidorPanico(
    FabricaConexaoRabbitMq fabricaConexao,
    IOptions<RabbitMqOpcoes> opcoes,
    IMediator mediator,
    ILogger<ConsumidorDispatch<MensagemPanico>> logger) : ConsumidorDispatch<MensagemPanico>(fabricaConexao, opcoes, mediator, logger)
{
}

public sealed class ConsumidorTelemetria(
    FabricaConexaoRabbitMq fabricaConexao,
    IOptions<RabbitMqOpcoes> opcoes,
    IMediator mediator,
    ILogger<ConsumidorDispatch<MensagemTelemetria>> logger) : ConsumidorDispatch<MensagemTelemetria>(fabricaConexao, opcoes, mediator, logger)
{
}

public sealed class ConsumidorVozDispatch(
    FabricaConexaoRabbitMq fabricaConexao,
    IOptions<RabbitMqOpcoes> opcoes,
    IMediator mediator,
    ILogger<ConsumidorDispatch<MensagemVozDispatch>> logger) : ConsumidorDispatch<MensagemVozDispatch>(fabricaConexao, opcoes, mediator, logger)
{
}
