using System.Collections.Generic;

namespace XboxFtp.Console.Configuration
{
    public class Settings
    {
        public bool TestMode { get; set; }
        public string GameRootDirectory { get; set; }
        public string Host { get; set; }
        public string User { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        
        /// <summary>
        /// A list of games to upload
        /// </summary>
        public List<string> GamesToUpload { get; set; }
        
        /// <summary>
        /// </summary>
        public string GameToUpload { get; set; }
        
        /// <summary>
        /// A path to a file containing a list of games to upload
        /// </summary>
        public string GamesToUploadFile { get; set; }
        public string HttpLogEndpoint { get; set; }
    }
}