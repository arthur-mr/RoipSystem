using RoipSystem.Dominio.Enums;

namespace RoipSystem.Aplicacao.Api.ViewModels;

public sealed record EventoRadioViewModel(
    string RadioId,
    string TalkgroupId,
    TipoEvento TipoEvento,
    double Latitude,
    double Longitude);
