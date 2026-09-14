using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoipSystem.Aplicacao.Api.ViewModels;
using RoipSystem.Dominio.Contratos;
using RoipSystem.Dominio.Mediador.Comandos;

using Microsoft.AspNetCore.Authorization;

namespace RoipSystem.Aplicacao.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/radio/eventos")]
public sealed class RadioEventosController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] EventoRadioViewModel request)
    {
        var contrato = new EventoRadioContrato(
            Guid.NewGuid(),
            request.RadioId,
            request.TalkgroupId,
            request.TipoEvento,
            DateTime.UtcNow,
            request.Latitude,
            request.Longitude);

        var comando = new PublicarEventoRadioComando(contrato);

        await mediator.Send(comando);
        return Accepted();
    }
}
