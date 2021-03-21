namespace XboxFtp.Core.UseCases
{
    public interface IZipFileProcessor
    {
        IZipFile Read(string archivePath);
    }
}