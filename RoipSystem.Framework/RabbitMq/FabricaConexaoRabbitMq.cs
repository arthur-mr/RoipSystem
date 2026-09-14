using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RoipSystem.Framework.Configuracoes;

namespace RoipSystem.Framework.RabbitMq;

public sealed class FabricaConexaoRabbitMq : IAsyncDisposable
{
    private readonly ConnectionFactory fabrica;
    private readonly SemaphoreSlim semaforo = new(1, 1);
    private IConnection? conexao;

    public FabricaConexaoRabbitMq(IOptions<RabbitMqOpcoes> opcoes)
    {
        var cfg = opcoes.Value;
        fabrica = new ConnectionFactory
        {
            HostName = cfg.Host,
            UserName = cfg.Usuario,
            Password = cfg.Senha
        };

        if (cfg.SslEnabled)
        {
            fabrica.Ssl.Enabled = true;
            if (!string.IsNullOrWhiteSpace(cfg.SslServerName))
            {
                fabrica.Ssl.ServerName = cfg.SslServerName;
            }
        }
    }

    public async Task<IConnection> ObterConexaoAsync(CancellationToken cancellationToken = default)
    {
        if (conexao != null && conexao.IsOpen)
            return conexao;

        await semaforo.WaitAsync(cancellationToken);
        try
        {
            if (conexao == null || !conexao.IsOpen)
            {
                conexao?.Dispose();
                conexao = await fabrica.CreateConnectionAsync(cancellationToken);
            }

            return conexao;
        }
        finally
        {
            semaforo.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (conexao != null)
        {
            await conexao.CloseAsync();
            await conexao.DisposeAsync();
        }

        semaforo.Dispose();
    }
}
