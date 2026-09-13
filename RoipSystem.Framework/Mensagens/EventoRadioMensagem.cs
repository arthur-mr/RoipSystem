namespace RoipSystem.Framework.Mensagens;

public abstract class EventoRadioMensagem
{
    public Guid Id { get; set; }
    public string RadioId { get; set; } = string.Empty;
    public string TalkgroupId { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}