using MediatR;
using RoipSystem.Dominio.Mediador.Contratos;

namespace RoipSystem.Dominio.Mediador.Comandos;

public sealed record PublicarEventoRadioComando(EventoRadioContrato Contrato) : IRequest;
