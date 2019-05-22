using System.Collections.Generic;

namespace Adapter.Persistence.Ftp
{
    public class FtpXboxSettings
    {
        public string GameRootDirectory { get; set; }
        public string Host { get; set; }
        public string User { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }

        public List<string> GamesToUpload { get; set; }
    }
}