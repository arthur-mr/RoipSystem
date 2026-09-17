 ### 1. Descrição do Cenário
  O sistema RoipSystem simula um ecossistema de comunicação RoIP. 
  Esse tipo de sistema pode operar em cenários de missão crítica, como segurança pública, suporte médico de
  emergência, etc.

  Justificativa da necessidade de comunicação assíncrona via mensageria:
  Nesse tipo de cenário, milhares de rádios conectados aos Gateways em campo podem despachar pacotes de localização,
  início de transmissão de voz (PTT) ou até alertas de pânico ao mesmo tempo.

  1. Evitar Sobrecarga e Perda de Dados: Um modelo de requisição HTTP síncrona geraria
  gargalos severos, além de exigir que a base de dados do sistema central gravasse essas
  requisições de imediato. Usando RabbitMQ, o Gateway (produtor) só precisa despachar o evento para a rede e seguir
  em frente. A mensageria funciona como um grande "amortecedor" (buffer) de tráfego.
  2. Disponibilidade e Resiliência: Em situações de acidentes generalizados onde rádios disparam eventos de "Pânico"
  ou telemetria intensa, uma arquitetura síncrona cairia em cascata. Na arquitetura assíncrona, mesmo que os serviços
  do backend caiam ou o banco de dados fique lento, as mensagens ficam retidas em segurança no broker (RabbitMQ) até
  que os consumidores se recuperem para processá-las.
  ──────
  ### 2. Arquitetura da Solução

  Produtores e Consumidores:

  • Produtores: Os hardwares de campo que convertem radiofrequência em IP, formam a camada de produção de dados. 
    Na aplicação, essa produção é respnsabilidade da API.
  • Consumidores: No RoipSystem, são implementados pela classe abstrata ConsumidorRabbitMqBase, que se conecta às 
    filas específicas e faz o tratamento da mensagem quando ela chega e os direciona ao domínio e à infraestrutura
    de persistência (Banco de Dados).

  Estratégias de escalabilidade, confiabilidade e tolerância a falhas (Implementadas no Sistema):

  • Escalabilidade: O consumidor do RabbitMQ implementa o atributo prefetchCount
  limitando quantas mensagens um worker deve puxar em memória de cada vez. Isso impede o afogamento do processador e
  de banco de dados, permitindo instanciar dezenas de réplicas do consumidor competindo pelos mesmos dados da fila se
  o tráfego aumentar.
  • Confiabilidade: O consumidor foi implementado desativando a confirmação
  automática (autoAck: false). A mensagem só desaparece da fila caso o processamento chegue ao final com absoluto
  sucesso (com a chamada de BasicAckAsync). Além disso, as propriedades básicas são enviadas como Persistent = true
  pelo publicador e a declaração da fila força durable: true, garantindo que um reinício ou crash do servidor
  RabbitMQ não resulte na perda dos eventos.
  • Tolerância a Falhas (Retry Pattern e DLQ):
      • Retry: O Consumer possui a variável maxRetries. Se houver falha (ex: banco offline ou falha
      inesperada), o sistema recria a mensagem na fila, incrementando o cabeçalho x-retry-count.
      • Dead Letter Queue (DLQ): Após esgotar o limite de tentativas (maxRetries), em vez de descartar
      silenciosamente, a mensagem recebe uma confirmação negativa sem retorno para a fila (BasicNackAsync, com
      requeue: false). Pelos parâmetros do RabbitMQ definidos na inicialização do consumidor (x-dead-letter-exchange),
      esse evento é redirecionado para o exchange ExchangeDlx caindo na Fila Morta (FilaDeadLetter),
      possibilitando a investigação manual ou o reprocessamento posterior de eventos.