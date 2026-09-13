using MediatR;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.Interfaces;
using RoipSystem.Framework.Mensagens;

namespace RoipSystem.Dominio.Mediador.Handlers;

public sealed class PublicarEventoRadioHandler : IRequestHandler<PublicarEventoRadioComando>
{
    private readonly IPublicadorMensagem publicador;

    public PublicarEventoRadioHandler(IPublicadorMensagem publicador)
    {
        this.publicador = publicador;
    }

    public async Task Handle(PublicarEventoRadioComando request, CancellationToken cancellationToken)
    {
        EventoRadioMensagem mensagem = request.Contrato.TipoEvento switch
        {
            TipoEvento.Panico => new MensagemPanico(),
            TipoEvento.Telemetria => new MensagemTelemetria(),
            TipoEvento.Voz => new MensagemVoz(),
            _ => new MensagemVoz()
        };

        mensagem.Id = request.Contrato.Id;
        mensagem.RadioId = request.Contrato.RadioId;
        mensagem.TalkgroupId = request.Contrato.TalkgroupId;
        mensagem.TipoEvento = request.Contrato.TipoEvento.ToString();
        mensagem.DataHora = request.Contrato.DataHora;
        mensagem.Latitude = request.Contrato.Latitude;
        mensagem.Longitude = request.Contrato.Longitude;

        string routingKeySuffix = request.Contrato.TipoEvento switch
        {
            TipoEvento.Panico => "panic",
            TipoEvento.Telemetria => "telemetry",
            TipoEvento.Voz => "voice",
            _ => "unknown"
        };
        string routingKey = $"radio.{request.Contrato.RadioId}.{routingKeySuffix}";

        byte priority = request.Contrato.TipoEvento switch
        {
            TipoEvento.Panico => 9,
            TipoEvento.Voz => 5,
            TipoEvento.Telemetria => 1,
            _ => 0
        };

        await publicador.PublicarAsync(mensagem, routingKey, priority, cancellationToken);
    }
}
