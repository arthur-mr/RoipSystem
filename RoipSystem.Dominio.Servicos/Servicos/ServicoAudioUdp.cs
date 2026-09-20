using Microsoft.Extensions.Logging;
using NAudio.Wave;
using RoipSystem.Dominio.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace RoipSystem.Dominio.Servicos.Servicos;

public sealed class ServicoAudioUdp : IServicoAudioUdp
{
    private readonly ILogger<ServicoAudioUdp> logger;
    private readonly WaveFormat formatoAudio = new(8000, 16, 1);
    private WaveInEvent? waveIn;
    private UdpClient? udpTransmissor;
    private volatile bool transmitindo;
    private UdpClient? udpReceptor;
    private WaveOutEvent? waveOut;
    private BufferedWaveProvider? bufferProvider;
    private volatile bool recebendo;
    private Thread? threadRecepcao;

    public ServicoAudioUdp(ILogger<ServicoAudioUdp> logger)
    {
        this.logger = logger;
    }

    public void IniciarTransmissao(string ipDestino, int portaDestino)
    {
        if (transmitindo) 
            return;

        udpTransmissor = new UdpClient();
        var endpoint = new IPEndPoint(IPAddress.Parse(ipDestino), portaDestino);

        waveIn = new WaveInEvent
        {
            WaveFormat = formatoAudio,
            BufferMilliseconds = 40
        };

        waveIn.DataAvailable += (_, e) =>
        {
            if (e.BytesRecorded > 0)
            {
                try
                {
                    udpTransmissor?.Send(e.Buffer, e.BytesRecorded, endpoint);
                }
                catch (SocketException) { }
                catch (ObjectDisposedException) { }
            }
        };

        waveIn.StartRecording();
        transmitindo = true;
    }

    public void PararTransmissao()
    {
        if (!transmitindo) 
            return;
        
        transmitindo = false;

        waveIn?.StopRecording();
        waveIn?.Dispose();
        waveIn = null;

        try { udpTransmissor?.Close(); } catch { }
        udpTransmissor?.Dispose();
        udpTransmissor = null;
    }

    public void IniciarRecepcao(int portaLocal)
    {
        if (recebendo) 
            return;

        bufferProvider = new BufferedWaveProvider(formatoAudio)
        {
            DiscardOnBufferOverflow = true,
            BufferLength = 1024 * 1024 // 1 MB
        };

        waveOut = new WaveOutEvent { DesiredLatency = 100 };
        waveOut.Init(bufferProvider);
        waveOut.Play();

        udpReceptor = new UdpClient(portaLocal);
        recebendo = true;

        threadRecepcao = new Thread(LoopRecepcao)
        {
            IsBackground = true,
            Name = "ThreadAudioRx",
            Priority = ThreadPriority.AboveNormal
        };
        threadRecepcao.Start();
    }

    private void LoopRecepcao()
    {
        var endpointRemoto = new IPEndPoint(IPAddress.Any, 0);

        while (recebendo)
        {
            try
            {
                if (udpReceptor is not null)
                {
                    byte[] bytes = udpReceptor.Receive(ref endpointRemoto);
                    bufferProvider?.AddSamples(bytes, 0, bytes.Length);
                }
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }
    }

    public void PararRecepcao()
    {
        if (!recebendo) return;
        recebendo = false;

        try { udpReceptor?.Close(); } catch { }
        udpReceptor?.Dispose();
        udpReceptor = null;

        waveOut?.Stop();
        waveOut?.Dispose();
        waveOut = null;

        bufferProvider?.ClearBuffer();
        bufferProvider = null;
    }

    public void Dispose()
    {
        PararTransmissao();
        PararRecepcao();
    }
}
