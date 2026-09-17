namespace RoipSystem.Framework.Configuracoes;

public class ConfiguracaoMensageria
{
    public string Host { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public ushort PrefetchCount { get; set; }
    public int MaxRetries { get; set; }

    public string ExchangePrincipal { get; set; } = string.Empty;
    public string ExchangeDlx { get; set; } = string.Empty;
    public string FilaDeadLetter { get; set; } = string.Empty;
    public string RoutingKeyDeadLetter { get; set; } = string.Empty;
    public int MensagemTtlMs { get; set; }
    public byte MaxPriority { get; set; }
    
    public bool SslEnabled { get; set; }
    public string SslServerName { get; set; } = string.Empty;
}
