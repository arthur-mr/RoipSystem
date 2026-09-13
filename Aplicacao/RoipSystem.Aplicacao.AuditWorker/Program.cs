using RoipSystem.Aplicacao.AuditWorker;
using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Framework.Mensagens;
using RoipSystem.Infra.Modulos;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRoipInfrastructure(opcoes =>
    builder.Configuration.GetSection("RabbitMq").Bind(opcoes));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SalvarAuditoriaComando).Assembly));

builder.Services.AddHostedService<ConsumidorAuditoria>();

var host = builder.Build();

var topologia = host.Services.GetRequiredService<RoipSystem.Infra.Mensageria.ConstrutorTopologiaRabbitMq>();
await topologia.ConstruirAsync();

await host.RunAsync();
