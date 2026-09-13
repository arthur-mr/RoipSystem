namespace RoipSystem.Framework.Atributos;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class FilaRabbitMqAttribute : Attribute
{
    public string NomeFila { get; }
    public string RoutingKeyBinding { get; set; } = string.Empty;

    public FilaRabbitMqAttribute(string nomeFila)
    {
        NomeFila = nomeFila;
    }
}
