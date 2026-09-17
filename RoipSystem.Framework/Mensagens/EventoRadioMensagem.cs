using RoipSystem.Framework.Atributos;

namespace RoipSystem.Framework.Mensagens;

[FilaRabbitMq("q.roip.eventos", RoutingKeyBinding = "radio.#")]
public class EventoRadioMensagem
{
    public Guid Id { get; set; }
    public string RadioId { get; set; } = string.Empty;
    public string TalkgroupId { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}