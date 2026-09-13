using RoipSystem.Dominio.Mediador.Comandos;
using RoipSystem.Infra.Mensageria;
using RoipSystem.Infra.Modulos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRoipInfrastructure(opcoes =>
    builder.Configuration.GetSection("RabbitMq").Bind(opcoes));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PublicarEventoRadioComando).Assembly));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var topologia = scope.ServiceProvider.GetRequiredService<ConstrutorTopologiaRabbitMq>();
    await topologia.ConstruirAsync();
}

app.MapControllers();

app.Run();
