using RoipSystem.Framework.Atributos;
using System.Reflection;

namespace RoipSystem.Framework.RabbitMq;

public static class FilaMensagemCache<T> where T : class
{
    public static readonly string NomeFila;

    static FilaMensagemCache()
    {
        var atributo = typeof(T).GetCustomAttribute<FilaRabbitMqAttribute>();
        if (atributo == null || string.IsNullOrWhiteSpace(atributo.NomeFila))
            throw new InvalidOperationException(
                $"A classe {typeof(T).Name} precisa estar decorada com [FilaRabbitMq(\"nome-da-fila\")].");

        NomeFila = atributo.NomeFila;
    }
}
