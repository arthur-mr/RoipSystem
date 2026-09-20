using MediatR;
using Microsoft.Extensions.Logging;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.Interfaces;
using RoipSystem.Framework.Mensagens;

namespace RoipSystem.Dominio.Mediador.Handlers;

internal sealed class EmitirSinalPttHandler(
    IPublicadorMensagem publicador,
    ILogger<EmitirSinalPttHandler> logger) : IRequestHandler<EmitirSinalPttComando>
{
    public async Task Handle(EmitirSinalPttComando request, CancellationToken cancellationToken)
    {
        var mensagem = new PttSinalMensagem
        {
            Evento = request.Evento.ToString(),
            Usuario = request.Usuario,
            IpOrigem = request.IpOrigem,
            PortaUdp = request.PortaUdp
        };

        var routingKey = "ptt." + request.Usuario.ToLowerInvariant();

        await publicador.PublicarAsync(mensagem, routingKey, priority: 5, cancellationToken: cancellationToken);
        
        logger.LogInformation("[Sinalização] Evento {Evento} publicado por {Usuario}", request.Evento, request.Usuario);
    }
}
