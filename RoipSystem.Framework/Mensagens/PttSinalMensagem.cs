using RoipSystem.Framework.Atributos;

namespace RoipSystem.Framework.Mensagens;

[FilaRabbitMq("q.roip.ptt.sinalizacao", RoutingKeyBinding = "ptt.#")]
public class PttSinalMensagem
{
    public string Evento { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;

    public string IpOrigem { get; set; } = string.Empty;

    public int PortaUdp { get; set; }
}
