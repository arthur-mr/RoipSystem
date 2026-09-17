namespace RoipSystem.Framework.Interfaces;

public interface IPublicadorMensagem
{
    Task PublicarAsync<T>(T mensagem, string routingKey, byte priority = 0, System.Threading.CancellationToken cancellationToken = default) where T : class;
}
