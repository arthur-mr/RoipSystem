using RoipSystem.Dominio.Contratos;
using RoipSystem.Dominio.Modelos;

namespace RoipSystem.Dominio.Interfaces;

public interface IEventoRadioServico
{
    Task PublicarEventoAsync(EventoRadioContrato contrato, CancellationToken cancellationToken);

    Task ProcessarEventoAsync(EventoRadio eventoRadio);
}
