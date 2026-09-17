using Microsoft.Extensions.Hosting;
using RoipSystem.Framework.Interfaces;

namespace RoipSystem.Framework.RabbitMq;

public class ProcessoMensageria(IEnumerable<IConsumidor> consumidores) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = consumidores.Select(c => c.IniciarAsync(stoppingToken));
        await Task.WhenAll(tasks);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var tasks = consumidores.Select(c => c.PararAsync(cancellationToken));
        await Task.WhenAll(tasks);
        await base.StopAsync(cancellationToken);
    }
}
