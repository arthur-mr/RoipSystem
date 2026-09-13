using RoipSystem.Framework.Atributos;

namespace RoipSystem.Framework.Mensagens;

public class MensagemVoz : EventoRadioMensagem
{ }

[FilaRabbitMq("q.roip.audit.voice", RoutingKeyBinding = "radio.*.voice")]
public class MensagemVozAudit : MensagemVoz
{ }

[FilaRabbitMq("q.roip.dispatch.voice", RoutingKeyBinding = "radio.*.voice")]
public class MensagemVozDispatch : MensagemVoz
{ }
