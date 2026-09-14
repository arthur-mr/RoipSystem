using RoipSystem.Dominio.Enums;

namespace RoipSystem.Dominio.Modelos;

public sealed class EventoRadio
{
    public Guid Id { get; private set; }
    public string RadioId { get; private set; }
    public string TalkgroupId { get; private set; }
    public TipoEvento TipoEvento { get; private set; }
    public DateTime DataHora { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    protected EventoRadio()
    { }

    public EventoRadio(
        Guid id,
        string radioId,
        string talkgroupId,
        TipoEvento tipoEvento,
        DateTime dataHora,
        double latitude,
        double longitude)
    {
        Id = id;
        RadioId = radioId;
        TalkgroupId = talkgroupId;
        TipoEvento = tipoEvento;
        DataHora = dataHora;
        Latitude = latitude;
        Longitude = longitude;
    }
}