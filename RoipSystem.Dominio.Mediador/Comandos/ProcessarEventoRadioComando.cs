using MediatR;
using RoipSystem.Dominio.Mediador.Contratos;

namespace RoipSystem.Dominio.Mediador.Comandos;

public sealed record ProcessarEventoRadioComando(EventoRadioContrato Contrato) : IRequest;
