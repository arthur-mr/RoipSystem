using Microsoft.Extensions.Logging;
using RoipSystem.Dominio.Contratos;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Modelos;
using RoipSystem.Framework.Interfaces;
using RoipSystem.Framework.Mensagens;

namespace RoipSystem.Dominio.Servicos.Servicos;

public sealed class EventoRadioServico(ILogger<EventoRadioServico> logger, IPublicadorMensagem publicadorMensagem) : IEventoRadioServico
{
    public async Task PublicarEventoAsync(EventoRadioContrato contrato, CancellationToken cancellationToken)
    {
        EventoRadioMensagem mensagem = contrato.TipoEvento switch
        {
            TipoEvento.Panico => new MensagemPanico(),
            TipoEvento.Telemetria => new MensagemTelemetria(),
            TipoEvento.Voz => new MensagemVoz(),
            _ => new MensagemVoz()
        };

        mensagem.Id = contrato.Id;
        mensagem.RadioId = contrato.RadioId;
        mensagem.TalkgroupId = contrato.TalkgroupId;
        mensagem.TipoEvento = contrato.TipoEvento.ToString();
        mensagem.DataHora = contrato.DataHora;
        mensagem.Latitude = contrato.Latitude;
        mensagem.Longitude = contrato.Longitude;

        string routingKeySuffix = contrato.TipoEvento switch
        {
            TipoEvento.Panico => "panic",
            TipoEvento.Telemetria => "telemetry",
            TipoEvento.Voz => "voice",
            _ => "unknown"
        };
        string routingKey = $"radio.{contrato.RadioId}.{routingKeySuffix}";

        byte priority = contrato.TipoEvento switch
        {
            TipoEvento.Panico => 9,
            TipoEvento.Voz => 5,
            TipoEvento.Telemetria => 1,
            _ => 0
        };

        await publicadorMensagem.PublicarAsync(mensagem, routingKey, priority, cancellationToken);
    } 

    public async Task ProcessarEventoAsync(EventoRadio eventoRadio)
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
    }
}
