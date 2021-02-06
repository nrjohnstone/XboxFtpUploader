using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.UseCases
{
    public interface IXboxGameRepositoryFactory
    {
        IXboxGameRepository Create();
    }
}