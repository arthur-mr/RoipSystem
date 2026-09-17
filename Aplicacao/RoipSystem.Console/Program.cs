using System.Net.Http.Json;

var baseUrl = "http://localhost:5056";
var apiUrl = $"{baseUrl}/api/radio/eventos";
using var httpClient = new HttpClient();

Console.WriteLine("=== Simulador de Carga RoipSystem (Com JWT) ===");
Console.WriteLine("Autenticando na API...");
var tokenResponse = await httpClient.GetFromJsonAsync<TokenResponse>($"{baseUrl}/api/auth/token");
if (tokenResponse?.Token != null)
{
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.Token);
    Console.WriteLine("Autenticado com sucesso!\n");
}
else
{
    Console.WriteLine("Falha ao obter o token. As requisições falharão com 401 Unauthorized.\n");
}

while (true)
{
    Console.WriteLine("Selecione uma opção:");
    Console.WriteLine("1. Teste de Carga Normal (10.000 mensagens mistas)");
    Console.WriteLine("2. Teste de Prioridade (9.900 Telemetria, 100 Pânico enviadas em lote)");
    Console.WriteLine("3. Teste de Resiliência (Enviar mensagens que vão falhar propositalmente)");
    Console.Write("Opção: ");
    var opcao = Console.ReadLine();

    Console.WriteLine("Iniciando envio...");
    var watch = System.Diagnostics.Stopwatch.StartNew();

    if (opcao == "1")
    {
        var tasks = new List<Task>();
        for (int i = 0; i < 10000; i++)
        {
            int tipoEvento = i % 3;
            tasks.Add(EnviarMensagemAsync(httpClient, apiUrl, $"R-{i}", "TG-1", tipoEvento));
        }
        await Task.WhenAll(tasks);
    }
    else if (opcao == "2")
    {
        // Teste de prioridade: envia telemetria primeiro, depois pânico. 
        // Como a fila estará cheia (devido ao Task.Delay adicionado no consumidor), 
        // as mensagens de pânico (prioridade 9) devem pular para o início da fila!
        var tasks = new List<Task>();
        for (int i = 0; i < 9900; i++)
        {
            tasks.Add(EnviarMensagemAsync(httpClient, apiUrl, $"T-{i}", "TG-TELE", 1)); // 1 = Telemetria
        }
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(EnviarMensagemAsync(httpClient, apiUrl, $"P-{i}", "TG-URGENT", 3)); // 3 = Pânico (Prioridade Alta)
        }
        await Task.WhenAll(tasks);
    }
    else if (opcao == "3")
    {
        // Teste de resiliência: envia mensagens com RadioId="FALHA" para testar o re-enfileiramento e DLQ
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(EnviarMensagemAsync(httpClient, apiUrl, "FALHA", "TG-FAIL", 2)); // 2 = Voz
        }
        await Task.WhenAll(tasks);
        Console.WriteLine("Acompanhe os logs dos workers. As mensagens devem ser tentadas X vezes e depois descartadas para a DLQ.");
    }

    watch.Stop();
    Console.WriteLine($"\nEnvio finalizado em {watch.ElapsedMilliseconds}ms.");
    Console.WriteLine("Verifique os logs dos workers para atestar os testes.");
}

static async Task EnviarMensagemAsync(HttpClient client, string url, string radioId, string talkgroupId, int tipoEvento)
{
    var payload = new
    {
        RadioId = radioId,
        TalkgroupId = talkgroupId,
        TipoEvento = tipoEvento,
        Latitude = -23.5505,
        Longitude = -46.6333
    };

    try
    {
        var response = await client.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao enviar {radioId}: {ex.Message}");
    }
}

public record TokenResponse(string Token);
