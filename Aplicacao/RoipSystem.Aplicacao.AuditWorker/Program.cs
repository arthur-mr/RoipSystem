using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.RabbitMq;
using RoipSystem.Infra.Modulos;
using RoipSystem.Dominio.Servicos.Modulos;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRoipServicos(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SalvarAuditoriaComando).Assembly));

builder.Services.AdicionarMensageria(builder.Configuration, typeof(Program).Assembly);

var host = builder.Build();

await host.RunAsync();
