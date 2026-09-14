using System.Threading;
using System.Threading.Tasks;

namespace RoipSystem.Framework.Interfaces;

public interface IConsumidor
{
    Task IniciarAsync(CancellationToken cancellationToken);
    Task PararAsync(CancellationToken cancellationToken = default);
}
