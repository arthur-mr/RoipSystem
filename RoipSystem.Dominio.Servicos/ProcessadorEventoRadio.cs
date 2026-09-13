using Microsoft.Extensions.Logging;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Modelos.Records;

namespace RoipSystem.Dominio.Servicos;

public sealed class ProcessadorEventoRadio : IProcessadorEventoRadio
{
    private readonly ILogger<ProcessadorEventoRadio> logger;

    public ProcessadorEventoRadio(ILogger<ProcessadorEventoRadio> logger)
    {
        this.logger = logger;
    }

    public Task ProcessarEventoAsync(EventoRadio eventoRadio)
    {
        switch (eventoRadio.TipoEvento)
        {
            case TipoEvento.Panico:
                logger.LogCritical(
                    "[URGENTE] Evento de pânico recebido | RadioId={RadioId} | Talkgroup={TalkgroupId} | Lat={Lat} | Lon={Lon}",
                    eventoRadio.RadioId, eventoRadio.TalkgroupId, eventoRadio.Latitude, eventoRadio.Longitude);
                break;

            case TipoEvento.Telemetria:
                logger.LogInformation(
                    "Evento de telemetria recebido | RadioId={RadioId}",
                    eventoRadio.RadioId);
                break;

            case TipoEvento.Voz:
                logger.LogInformation(
                    "Evento de voz recebido | RadioId={RadioId}",
                    eventoRadio.RadioId);
                break;

            default:
                logger.LogWarning("TipoEvento desconhecido: {TipoEvento}", eventoRadio.TipoEvento);
                break;
        }

        return Task.CompletedTask;
    }
}
