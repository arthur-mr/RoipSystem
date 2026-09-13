using RabbitMQ.Client;
using RoipSystem.Framework.Interfaces;
using System.Text.Json;

namespace RoipSystem.Framework.RabbitMq;

public class PublicadorRabbitMq : IPublicadorMensagem
{
    private readonly FabricaConexaoRabbitMq fabricaConexao;
    private readonly string nomeExchange;

    public PublicadorRabbitMq(FabricaConexaoRabbitMq fabricaConexao, string nomeExchange)
    {
        this.fabricaConexao = fabricaConexao;
        this.nomeExchange = nomeExchange;
    }

    public async Task PublicarAsync<T>(T mensagem, string routingKey, byte priority = 0, CancellationToken cancellationToken = default) where T : class
    {
        var conexao = await fabricaConexao.ObterConexaoAsync(cancellationToken);
        await using var canal = await conexao.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(mensagem);

        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Priority = priority
        };

        await canal.BasicPublishAsync(
            exchange: nomeExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: propriedades,
            body: body,
            cancellationToken: cancellationToken);
    }
}
