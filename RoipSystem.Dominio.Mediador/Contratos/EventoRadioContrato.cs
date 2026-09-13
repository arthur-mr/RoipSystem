using RoipSystem.Dominio.Enums;

namespace RoipSystem.Dominio.Mediador.Contratos;

public sealed record EventoRadioContrato(
    Guid Id,
    string RadioId,
    string TalkgroupId,
    TipoEvento TipoEvento,
    DateTime DataHora,
    double Latitude,
    double Longitude
);
