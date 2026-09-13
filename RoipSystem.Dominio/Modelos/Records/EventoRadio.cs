using RoipSystem.Dominio.Enums;

namespace RoipSystem.Dominio.Modelos.Records;

public sealed record EventoRadio(
    Guid Id,
    string RadioId,
    string TalkgroupId,
    TipoEvento TipoEvento,
    DateTime DataHora,
    double Latitude,
    double Longitude
);
