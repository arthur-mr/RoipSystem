using RoipSystem.Dominio.Modelos.Records;

namespace RoipSystem.Dominio.Interfaces;

public interface IProcessadorEventoRadio
{
    Task ProcessarEventoAsync(EventoRadio eventoRadio);
}
