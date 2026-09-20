namespace RoipSystem.Dominio.Interfaces;

public interface IServicoAudioUdp : IDisposable
{
    void IniciarRecepcao(int portaLocal);

    void IniciarTransmissao(string ipDestino, int portaDestino);
    
    void PararRecepcao();
    
    void PararTransmissao();
}