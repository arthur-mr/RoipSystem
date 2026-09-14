using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.RabbitMq;
using RoipSystem.Infra.Modulos;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRoipInfrastructure(opcoes =>
    builder.Configuration.GetSection("RabbitMq").Bind(opcoes));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ProcessarEventoRadioComando).Assembly));

builder.Services.AdicionarMensageria(typeof(Program).Assembly);

var host = builder.Build();

await host.RunAsync();
