using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Mediador.Comandos;

namespace RoipSystem.Dominio.Mediador.Handlers;

internal sealed class ProcessarSinalPttHandler(
    IServicoAudioUdp servico,
    IConfiguration configuration,
    ILogger<ProcessarSinalPttHandler> logger) : IRequestHandler<ProcessarSinalPttComando>
{
    public Task Handle(ProcessarSinalPttComando request, CancellationToken cancellationToken)
    {
        var sinal = request.Contrato;
        var portaUdpStr = configuration["Ptt:PortaUdp"];

        if (string.IsNullOrWhiteSpace(portaUdpStr) || !int.TryParse(portaUdpStr, out var portaUdp))
        {
            logger.LogError("A configuração 'Ptt:PortaUdp' é obrigatória e deve ser um número inteiro válido.");
            throw new InvalidOperationException("A configuração 'Ptt:PortaUdp' é obrigatória e deve ser um número inteiro válido.");
        }

        if (sinal.Evento == PttEvento.PTT_ON)
        {
            logger.LogInformation("[Sinalização] Recebido PTT_ON. Usuário {Usuario} iniciou transmissão.", sinal.Usuario);
            servico.IniciarRecepcao(portaUdp);
        }
        else if (sinal.Evento == PttEvento.PTT_OFF)
        {
            logger.LogInformation("[Sinalização] Recebido PTT_OFF. Usuário {Usuario} encerrou transmissão.", sinal.Usuario);
            servico.PararRecepcao();
        }

        return Task.CompletedTask;
    }
}
