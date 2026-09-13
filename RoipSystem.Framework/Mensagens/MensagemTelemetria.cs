using RoipSystem.Framework.Atributos;

namespace RoipSystem.Framework.Mensagens;

[FilaRabbitMq("q.roip.telemetry", RoutingKeyBinding = "radio.*.telemetry")]
public class MensagemTelemetria : EventoRadioMensagem
{ }
