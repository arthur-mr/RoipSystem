using Microsoft.Extensions.Logging;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Modelos.Records;

namespace RoipSystem.Infra.Persistencia;

public sealed class RepositorioAuditoriaMock : IRepositorioAuditoria
{
    private readonly ILogger<RepositorioAuditoriaMock> logger;

    public RepositorioAuditoriaMock(ILogger<RepositorioAuditoriaMock> logger)
    {
        this.logger = logger;
    }

    public Task SalvarAuditoriaAsync(EventoRadio eventoRadio)
    {
        logger.LogInformation(
            "[AUDITORIA MOCK] EventoId={Id} | Tipo={Tipo} | Radio={RadioId} | Talkgroup={TalkgroupId} | Lat={Lat} | Lon={Lon}",
            eventoRadio.Id,
            eventoRadio.TipoEvento,
            eventoRadio.RadioId,
            eventoRadio.TalkgroupId,
            eventoRadio.Latitude,
            eventoRadio.Longitude);

        return Task.CompletedTask;
    }
}
