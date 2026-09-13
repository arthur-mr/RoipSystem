using RoipSystem.Framework.Atributos;

namespace RoipSystem.Framework.Mensagens;

[FilaRabbitMq("q.roip.panic", RoutingKeyBinding = "radio.*.panic")]
public class MensagemPanico : EventoRadioMensagem
{ }
