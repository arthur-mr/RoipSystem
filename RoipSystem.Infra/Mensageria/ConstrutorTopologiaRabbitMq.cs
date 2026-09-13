using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RoipSystem.Framework.Atributos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Framework.RabbitMq;
using System.Reflection;

namespace RoipSystem.Infra.Mensageria;

public class ConstrutorTopologiaRabbitMq
{
    private readonly FabricaConexaoRabbitMq fabricaConexao;
    private readonly RabbitMqOpcoes opcoes;

    public ConstrutorTopologiaRabbitMq(
        FabricaConexaoRabbitMq fabricaConexao,
        IOptions<RabbitMqOpcoes> opcoes)
    {
        this.fabricaConexao = fabricaConexao;
        this.opcoes = opcoes.Value;
    }

    public async Task ConstruirAsync(CancellationToken cancellationToken = default)
    {
        var conexao = await fabricaConexao.ObterConexaoAsync(cancellationToken);
        await using var canal = await conexao.CreateChannelAsync(cancellationToken: cancellationToken);

        await canal.ExchangeDeclareAsync(
            exchange: opcoes.ExchangeDlx,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.QueueDeclareAsync(
            queue: opcoes.FilaDeadLetter,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.QueueBindAsync(
            queue: opcoes.FilaDeadLetter,
            exchange: opcoes.ExchangeDlx,
            routingKey: opcoes.RoutingKeyDeadLetter,
            cancellationToken: cancellationToken);

        await canal.ExchangeDeclareAsync(
            exchange: opcoes.ExchangePrincipal,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var assemblyMensagens = typeof(EventoRadioMensagem).Assembly;

        foreach (var tipo in assemblyMensagens.GetTypes())
        {
            var filaAttr = tipo.GetCustomAttribute<FilaRabbitMqAttribute>();

            if (filaAttr != null)
            {
                var argumentos = new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", opcoes.ExchangeDlx },
                    { "x-dead-letter-routing-key", opcoes.RoutingKeyDeadLetter },
                    { "x-message-ttl", opcoes.MensagemTtlMs },
                    { "x-max-priority", opcoes.MaxPriority }
                };

                await canal.QueueDeclareAsync(
                    queue: filaAttr.NomeFila,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: argumentos,
                    cancellationToken: cancellationToken);

                if (!string.IsNullOrWhiteSpace(filaAttr.RoutingKeyBinding))
                {
                    await canal.QueueBindAsync(
                        queue: filaAttr.NomeFila,
                        exchange: opcoes.ExchangePrincipal,
                        routingKey: filaAttr.RoutingKeyBinding,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}
