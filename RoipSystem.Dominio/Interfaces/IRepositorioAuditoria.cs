using RoipSystem.Dominio.Modelos.Records;

namespace RoipSystem.Dominio.Interfaces;

public interface IRepositorioAuditoria
{
    Task SalvarAuditoriaAsync(EventoRadio eventoRadio);
}
