using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoipSystem.Console.Configuracoes;
using RoipSystem.Console.Servicos;
using RoipSystem.Dominio.Enums;
using RoipSystem.Dominio.Interfaces;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Dominio.Servicos.Servicos;
using RoipSystem.Framework.Configuracoes;
using RoipSystem.Framework.Interfaces;
using RoipSystem.Framework.RabbitMq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);
const int VK_SPACE = 0x20;
const int VK_ESCAPE = 0x1B;
static bool TeclaPressionada(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

static string ObterIpLocal()
{
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
    socket.Connect("8.8.8.8", 65530);
    return ((IPEndPoint)socket.LocalEndPoint!).Address.ToString();
}

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var configPtt = config.GetSection("Ptt").Get<ConfiguracaoPtt>();

if (configPtt is null)
    throw new InvalidOperationException("Seção 'Ptt' não encontrada no appsettings.json.");

if (string.IsNullOrWhiteSpace(configPtt.NomeUsuario))
    throw new InvalidOperationException("A configuração 'Ptt:NomeUsuario' é obrigatória no appsettings.json.");

if (string.IsNullOrWhiteSpace(configPtt.IpDestino))
    throw new InvalidOperationException("A configuração 'Ptt:IpDestino' é obrigatória no appsettings.json.");

var ipLocal = ObterIpLocal();

Console.WriteLine("--------------------------------------------------");
Console.WriteLine(" RoIP Push-To-Talk System                         ");
Console.WriteLine("--------------------------------------------------");
Console.WriteLine($" Usuario local : {configPtt.NomeUsuario}");
Console.WriteLine($" IP Local      : {ipLocal}");
Console.WriteLine($" IP Destino    : {configPtt.IpDestino}");
Console.WriteLine($" Porta UDP     : {configPtt.PortaUdp}");
Console.WriteLine("--------------------------------------------------");
Console.WriteLine(" [ESPACO] Pressionar e segurar para falar         ");
Console.WriteLine(" [ESC] Encerrar aplicacao                         ");
Console.WriteLine("--------------------------------------------------");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cb =>
    {
        cb.SetBasePath(AppContext.BaseDirectory);
        cb.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices((ctx, services) =>
    {
        var configMensageria = ctx.Configuration.GetSection("RabbitMq").Get<ConfiguracaoMensageria>();
        if (configMensageria is null)
        {
            throw new InvalidOperationException("Seção 'RabbitMq' não encontrada no appsettings.json.");
        }

        services.AddSingleton(configMensageria);
        services.AddSingleton<FabricaConexaoRabbitMq>();
        services.AddSingleton<IPublicadorMensagem>(sp =>
        {
            var fabrica = sp.GetRequiredService<FabricaConexaoRabbitMq>();
            var cfg = sp.GetRequiredService<ConfiguracaoMensageria>();
            return new PublicadorRabbitMq(fabrica, cfg.ExchangePrincipal);
        });

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarSinalPttComando).Assembly));
        services.AddSingleton<IServicoAudioUdp, ServicoAudioUdp>();

        services.AddSingleton<ConsumidorPttSinalMensagem>(sp =>
        {
            var fabrica = sp.GetRequiredService<FabricaConexaoRabbitMq>();
            var cfgMsg = sp.GetRequiredService<ConfiguracaoMensageria>();
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var logger = sp.GetRequiredService<ILogger<ConsumidorPttSinalMensagem>>();

            return new ConsumidorPttSinalMensagem(
                fabricaConexao: fabrica,
                configuracaoMensageria: cfgMsg,
                serviceScopeFactory: scopeFactory,
                logger: logger,
                usuarioLocal: configPtt.NomeUsuario);
        });
        services.AddSingleton<IConsumidor>(sp => sp.GetRequiredService<ConsumidorPttSinalMensagem>());

        services.AddHostedService<ProcessoMensageria>();
    })
    .Build();

var audio = host.Services.GetRequiredService<IServicoAudioUdp>();
var mediator = host.Services.GetRequiredService<IMediator>();

var cts = new CancellationTokenSource();
_ = host.StartAsync(cts.Token);

Console.WriteLine("Sistema PTT iniciado.");

bool transmitindo = false;

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        await Task.Delay(30, cts.Token);

        if (TeclaPressionada(VK_ESCAPE))
        {
            Console.WriteLine("Encerrando...");
            break;
        }

        bool espacoPressionado = TeclaPressionada(VK_SPACE);

        if (espacoPressionado && !transmitindo)
        {
            transmitindo = true;
            Console.WriteLine("[Audio TX] Iniciando transmissao...");

            var comando = new EmitirSinalPttComando(
                Evento: PttEvento.PTT_ON,
                Usuario: configPtt.NomeUsuario,
                IpOrigem: ipLocal,
                PortaUdp: configPtt.PortaUdp);

            await mediator.Send(comando, cts.Token);
            audio.IniciarTransmissao(configPtt.IpDestino, configPtt.PortaUdp);
        }
        else if (!espacoPressionado && transmitindo)
        {
            transmitindo = false;
            Console.WriteLine("[Audio TX] Transmissao encerrada.");
            audio.PararTransmissao();

            var comando = new EmitirSinalPttComando(
                Evento: PttEvento.PTT_OFF,
                Usuario: configPtt.NomeUsuario,
                IpOrigem: ipLocal,
                PortaUdp: configPtt.PortaUdp);

            await mediator.Send(comando, cts.Token);
        }
    }
}
catch (OperationCanceledException)
{ }
finally
{
    if (transmitindo)
    {
        audio.PararTransmissao();

        var comando = new EmitirSinalPttComando(
            Evento: PttEvento.PTT_OFF,
            Usuario: configPtt.NomeUsuario,
            IpOrigem: ipLocal,
            PortaUdp: configPtt.PortaUdp);

        try { await mediator.Send(comando, CancellationToken.None); } catch { }
    }

    audio.Dispose();
    cts.Cancel();
    await host.StopAsync(CancellationToken.None);
    host.Dispose();

    Console.WriteLine("Processo finalizado.");
}

public partial class Program { }
