using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RoipSystem.Framework.Configuracoes;

namespace RoipSystem.Framework.RabbitMq;

public sealed class FabricaConexaoRabbitMq : IAsyncDisposable
{
    private readonly ConnectionFactory fabrica;
    private readonly SemaphoreSlim semaforo = new(1, 1);
    private IConnection? conexao;

    public FabricaConexaoRabbitMq(ConfiguracaoMensageria configuracaoMensageria)
    {
        fabrica = new ConnectionFactory
        {
            Uri = new Uri(configuracaoMensageria.Uri),
            HostName = configuracaoMensageria.Host,
            VirtualHost = configuracaoMensageria.VirtualHost,
            UserName = configuracaoMensageria.Usuario,
            Password = configuracaoMensageria.Senha
        };

        if (configuracaoMensageria.SslEnabled)
        {
            fabrica.Ssl.Enabled = true;
            if (!string.IsNullOrWhiteSpace(configuracaoMensageria.SslServerName))
            {
                fabrica.Ssl.ServerName = configuracaoMensageria.SslServerName;
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
            if (conexao is null || !conexao.IsOpen)
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
