namespace XboxFtp.Core.UseCases
{
    public class XboxTransferRequest
    {
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }
}