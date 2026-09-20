using RabbitMQ.Client;
using RoipSystem.Framework.Interfaces;
using System.Text.Json;

namespace RoipSystem.Framework.RabbitMq;

public class PublicadorRabbitMq : IPublicadorMensagem, IAsyncDisposable
{
    private readonly FabricaConexaoRabbitMq fabricaConexao;
    private readonly string nomeExchange;
    private IChannel? canal;
    private readonly SemaphoreSlim semaforo = new(1, 1);

    public PublicadorRabbitMq(FabricaConexaoRabbitMq fabricaConexao, string nomeExchange)
    {
        this.fabricaConexao = fabricaConexao;
        this.nomeExchange = nomeExchange;
    }

    private async Task<IChannel> ObterCanalAsync(CancellationToken cancellationToken)
    {
        if (canal != null && canal.IsOpen)
            return canal;

        await semaforo.WaitAsync(cancellationToken);
        try
        {
            if (canal is null || !canal.IsOpen)
            {
                var conexao = await fabricaConexao.ObterConexaoAsync(cancellationToken);
                canal = await conexao.CreateChannelAsync(cancellationToken: cancellationToken);
            }
            return canal;
        }
        finally
        {
            semaforo.Release();
        }
    }

    public async Task PublicarAsync<T>(T mensagem, string routingKey, byte priority = 0, CancellationToken cancellationToken = default) where T : class
    {
        var channel = await ObterCanalAsync(cancellationToken);
        var body = JsonSerializer.SerializeToUtf8Bytes(mensagem);

        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Priority = priority
        };

        await channel.BasicPublishAsync(
            exchange: nomeExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: propriedades,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (canal != null)
        {
            await canal.CloseAsync();
            await canal.DisposeAsync();
        }
        semaforo.Dispose();
    }
}
