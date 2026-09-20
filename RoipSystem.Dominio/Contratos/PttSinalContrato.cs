using System.Text.Json.Serialization;
using RoipSystem.Dominio.Enums;

namespace RoipSystem.Dominio.Contratos;

public sealed class PttSinalContrato
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PttEvento Evento { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string IpOrigem { get; set; } = string.Empty;

    public int PortaUdp { get; set; }
}
