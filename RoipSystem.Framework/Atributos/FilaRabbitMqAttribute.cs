namespace RoipSystem.Framework.Atributos;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class FilaRabbitMqAttribute(string nomeFila) : Attribute
{
    public string NomeFila { get; } = nomeFila;
    public string RoutingKeyBinding { get; set; } = string.Empty;
}
