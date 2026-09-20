using MediatR;
using RoipSystem.Dominio.Enums;

namespace RoipSystem.Dominio.Mediador.Comandos;

public sealed record EmitirSinalPttComando(
    PttEvento Evento, 
    string Usuario, 
    string IpOrigem, 
    int PortaUdp) : IRequest;
