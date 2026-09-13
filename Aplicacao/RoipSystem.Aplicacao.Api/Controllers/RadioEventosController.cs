using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoipSystem.Aplicacao.Api.ViewModels;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Dominio.Mediador.Contratos;

namespace RoipSystem.Aplicacao.Api.Controllers;

[ApiController]
[Route("api/radio/eventos")]
public sealed class RadioEventosController : ControllerBase
{
    private readonly IMediator mediator;

    public RadioEventosController(IMediator mediator)
    {
        this.mediator = mediator;
    }

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
