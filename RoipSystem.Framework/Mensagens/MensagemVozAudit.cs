using RoipSystem.Framework.Atributos;

namespace RoipSystem.Framework.Mensagens;

[FilaRabbitMq("q.roip.audit.voice", RoutingKeyBinding = "radio.*.voice")]
public class MensagemVozAudit : EventoRadioMensagem
{ }
