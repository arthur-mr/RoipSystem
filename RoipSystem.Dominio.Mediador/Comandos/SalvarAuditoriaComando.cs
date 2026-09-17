using MediatR;
using RoipSystem.Dominio.Contratos;

namespace RoipSystem.Dominio.Mediador.Comandos;

public sealed record SalvarAuditoriaComando(EventoRadioContrato Contrato) : IRequest;
