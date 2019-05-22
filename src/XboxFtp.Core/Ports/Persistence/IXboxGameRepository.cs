namespace XboxFtp.Core.Ports.Persistence
{
    public interface IXboxGameRepository
    {
        void Connect();
        void Disconnect();
        void CreateGame(string gameName);
        void Store(string gameName, string targetFilePath, byte[] data);
        bool Exists(string gameName, string targetFilePath, long size);
        void CreateDirectory(string targetDirectory);
    }
}