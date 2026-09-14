using RoipSystem.Dominio.Modelos;

namespace RoipSystem.Dominio.Interfaces;

public interface IRepositorioAuditoria
{
    Task SalvarAuditoriaAsync(EventoRadio eventoRadio);
}
