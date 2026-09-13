using MediatR;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Dominio.Modelos.Records;

namespace RoipSystem.Dominio.Mediador.Handlers;

public sealed class ProcessarEventoRadioHandler : IRequestHandler<ProcessarEventoRadioComando>
{
    private readonly IProcessadorEventoRadio processador;

    public ProcessarEventoRadioHandler(IProcessadorEventoRadio processador)
    {
        this.processador = processador;
    }

    public async Task Handle(ProcessarEventoRadioComando request, CancellationToken cancellationToken)
    {
        var eventoRadio = new EventoRadio(
            request.Contrato.Id,
            request.Contrato.RadioId,
            request.Contrato.TalkgroupId,
            request.Contrato.TipoEvento,
            request.Contrato.DataHora,
            request.Contrato.Latitude,
            request.Contrato.Longitude);

        await processador.ProcessarEventoAsync(eventoRadio);
    }
}
