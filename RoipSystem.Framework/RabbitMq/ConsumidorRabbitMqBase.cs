using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RoipSystem.Framework.Atributos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Interfaces;
using System.Reflection;
using System.Text.Json;

namespace RoipSystem.Framework.RabbitMq;

public abstract class ConsumidorRabbitMqBase<T>(
    FabricaConexaoRabbitMq fabricaConexao,
    ConfiguracaoMensageria configuracaoMensageria,
    ILogger logger) : IConsumidor where T : class
{
    private readonly ushort prefetchCount = configuracaoMensageria.PrefetchCount;
    private readonly int maxRetries = configuracaoMensageria.MaxRetries;
    private readonly ILogger logger = logger;
    private IChannel? canal;
    private string? nomeFilaCache;

    private string ObterNomeFila()
    {
        if (nomeFilaCache != null) return nomeFilaCache;
        var atributo = typeof(T).GetCustomAttribute<FilaRabbitMqAttribute>();
        if (atributo is null || string.IsNullOrWhiteSpace(atributo.NomeFila))
            throw new InvalidOperationException($"A classe de mensagem {typeof(T).Name} precisa estar decorada com [FilaRabbitMq(\"nome-da-fila\")].");
        nomeFilaCache = atributo.NomeFila;
        return nomeFilaCache;
    }

    public async Task IniciarAsync(CancellationToken cancellationToken)
    {
        var conexao = await fabricaConexao.ObterConexaoAsync(cancellationToken);
        canal = await conexao.CreateChannelAsync(cancellationToken: cancellationToken);

        await canal.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: prefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        await canal.ExchangeDeclareAsync(
            exchange: configuracaoMensageria.ExchangeDlx,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.QueueDeclareAsync(
            queue: configuracaoMensageria.FilaDeadLetter,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.QueueBindAsync(
            queue: configuracaoMensageria.FilaDeadLetter,
            exchange: configuracaoMensageria.ExchangeDlx,
            routingKey: configuracaoMensageria.RoutingKeyDeadLetter,
            cancellationToken: cancellationToken);

        await canal.ExchangeDeclareAsync(
            exchange: configuracaoMensageria.ExchangePrincipal,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var atributo = typeof(T).GetCustomAttribute<FilaRabbitMqAttribute>();
        if (atributo != null)
        {
            var argumentos = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", configuracaoMensageria.ExchangeDlx },
                { "x-dead-letter-routing-key", configuracaoMensageria.RoutingKeyDeadLetter },
                { "x-message-ttl", configuracaoMensageria.MensagemTtlMs },
                { "x-max-priority", configuracaoMensageria.MaxPriority }
            };

            await canal.QueueDeclareAsync(
                queue: atributo.NomeFila,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: argumentos,
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(atributo.RoutingKeyBinding))
            {
                await canal.QueueBindAsync(
                    queue: atributo.NomeFila,
                    exchange: configuracaoMensageria.ExchangePrincipal,
                    routingKey: atributo.RoutingKeyBinding,
                    cancellationToken: cancellationToken);
            }
        }

        string nomeFila = ObterNomeFila();

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
        if (canal is null) return;

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

    public async Task PararAsync(CancellationToken cancellationToken = default)
    {
        if (canal != null && canal.IsOpen)
        {
            logger.LogInformation("Encerrando canal RabbitMQ da fila {Fila}", ObterNomeFila());
            await canal.CloseAsync(cancellationToken: cancellationToken);
            await canal.DisposeAsync();
        }
    }
}
