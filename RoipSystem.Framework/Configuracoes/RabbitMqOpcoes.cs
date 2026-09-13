namespace RoipSystem.Framework.Configuracoes;

public class RabbitMqOpcoes
{
    public const string Secao = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public string Usuario { get; set; } = "guest";
    public string Senha { get; set; } = "guest";
    public ushort PrefetchCount { get; set; } = 10;
    public int MaxRetries { get; set; } = 3;

    public string ExchangePrincipal { get; set; } = "roip.core";
    public string ExchangeDlx { get; set; } = "roip.dlx";
    public string FilaDeadLetter { get; set; } = "q.roip.deadletter";
    public string RoutingKeyDeadLetter { get; set; } = "deadletter";
    public int MensagemTtlMs { get; set; } = 60000;
    public byte MaxPriority { get; set; } = 10;
}
