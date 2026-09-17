using MediatR;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Mediador.Comandos;

namespace RoipSystem.Dominio.Mediador.Handlers;

internal sealed class PublicarEventoRadioHandler(IEventoRadioServico servico) : IRequestHandler<PublicarEventoRadioComando>
{
    public async Task Handle(PublicarEventoRadioComando request, CancellationToken cancellationToken)
    {
        await servico.PublicarEventoAsync(request.Contrato, cancellationToken);
    }
}
