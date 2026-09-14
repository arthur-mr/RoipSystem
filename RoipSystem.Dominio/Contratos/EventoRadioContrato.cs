using RoipSystem.Dominio.Enums;

namespace RoipSystem.Dominio.Contratos;

public sealed record EventoRadioContrato(
    Guid Id,
    string RadioId,
    string TalkgroupId,
    TipoEvento TipoEvento,
    DateTime DataHora,
    double Latitude,
    double Longitude);
