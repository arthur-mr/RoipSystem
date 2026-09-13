using MediatR;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Dominio.Modelos.Records;

namespace RoipSystem.Dominio.Mediador.Handlers;

public sealed class SalvarAuditoriaHandler : IRequestHandler<SalvarAuditoriaComando>
{
    private readonly IRepositorioAuditoria repositorio;

    public SalvarAuditoriaHandler(IRepositorioAuditoria repositorio)
    {
        this.repositorio = repositorio;
    }

    public async Task Handle(SalvarAuditoriaComando request, CancellationToken cancellationToken)
    {
        var eventoRadio = new EventoRadio(
            request.Contrato.Id,
            request.Contrato.RadioId,
            request.Contrato.TalkgroupId,
            request.Contrato.TipoEvento,
            request.Contrato.DataHora,
            request.Contrato.Latitude,
            request.Contrato.Longitude);

        await repositorio.SalvarAuditoriaAsync(eventoRadio);
    }
}
