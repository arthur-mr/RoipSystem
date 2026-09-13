using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RoipSystem.Framework.Configuracoes;
using System.Text.Json;

using Microsoft.Extensions.Hosting;

namespace RoipSystem.Framework.RabbitMq;

public abstract class ConsumidorRabbitMqBase<T> : BackgroundService where T : class
{
    private readonly FabricaConexaoRabbitMq fabricaConexao;
    private readonly ushort prefetchCount;
    private readonly int maxRetries;
    private readonly ILogger logger;
    private IChannel? canal;

    protected ConsumidorRabbitMqBase(
        FabricaConexaoRabbitMq fabricaConexao,
        IOptions<RabbitMqOpcoes> opcoes,
        ILogger logger)
    {
        this.fabricaConexao = fabricaConexao;
        this.prefetchCount = opcoes.Value.PrefetchCount;
        this.maxRetries = opcoes.Value.MaxRetries;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var conexao = await fabricaConexao.ObterConexaoAsync(cancellationToken);
        canal = await conexao.CreateChannelAsync(cancellationToken: cancellationToken);

        await canal.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: prefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        string nomeFila = FilaMensagemCache<T>.NomeFila;

        var consumidor = new AsyncEventingBasicConsumer(canal);
        consumidor.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var mensagem = JsonSerializer.Deserialize<T>(ea.Body.Span);
                if (mensagem is null)
                {
                    logger.LogWarning("Mensagem nula recebida na fila {Fila}. Descartando para DLQ.", nomeFila);
                    await canal.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: CancellationToken.None);
                    return;
                }

                bool sucesso = await ProcessarMensagemAsync(mensagem, cancellationToken);

                if (sucesso)
                {
                    await canal.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
                }
                else
                {
                    await TratarFalhaAsync(ea, nomeFila);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar mensagem da fila {Fila}.", nomeFila);
                await TratarFalhaAsync(ea, nomeFila);
            }
        };

        await canal.BasicConsumeAsync(
            queue: nomeFila,
            autoAck: false,
            consumer: consumidor,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Consumidor iniciado na fila {Fila} com prefetchCount={PrefetchCount} e maxRetries={MaxRetries}.",
            nomeFila, prefetchCount, maxRetries);
            
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task TratarFalhaAsync(BasicDeliverEventArgs ea, string nomeFila)
    {
        if (canal == null) return;

        int retryCount = 0;
        if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var value))
        {
            if (value is int count)
                retryCount = count;
        }

        if (retryCount < maxRetries)
        {
            retryCount++;
            logger.LogWarning("Falha no processamento da fila {Fila}. Tentativa {RetryCount}/{MaxRetries}. Reenfileirando...", nomeFila, retryCount, maxRetries);

            var headers = new Dictionary<string, object?>();
            if (ea.BasicProperties.Headers != null)
            {
                foreach (var kvp in ea.BasicProperties.Headers)
                {
                    headers[kvp.Key] = kvp.Value;
                }
            }
            headers["x-retry-count"] = retryCount;

            var propriedades = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = headers
            };

            await canal.BasicPublishAsync(
                exchange: "",
                routingKey: nomeFila,
                mandatory: false,
                basicProperties: propriedades,
                body: ea.Body.ToArray(),
                cancellationToken: CancellationToken.None);

            await canal.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
        }
        else
        {
            logger.LogError("Máximo de retries ({MaxRetries}) atingido para fila {Fila}. Descartando para DLQ.", maxRetries, nomeFila);
            await canal.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: CancellationToken.None);
        }
    }

    protected abstract Task<bool> ProcessarMensagemAsync(T mensagem, CancellationToken cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (canal != null && canal.IsOpen)
        {
            logger.LogInformation("Encerrando canal RabbitMQ da fila {Fila}", FilaMensagemCache<T>.NomeFila);
            await canal.CloseAsync(cancellationToken: cancellationToken);
            await canal.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
