using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Infra.Modulos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRoipInfrastructure(opcoes =>
    builder.Configuration.GetSection("RabbitMq").Bind(opcoes));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PublicarEventoRadioComando).Assembly));

var app = builder.Build();


app.MapControllers();

app.Run();
