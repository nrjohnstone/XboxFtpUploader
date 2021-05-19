using NUnit.Framework;

namespace Adapter.Persistence.Ftp.Tests.Integration
{
    [SetUpFixture]
    internal class Global
    {
        internal static Settings Settings { get; private set; }
        
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            ConfigureSettings();
            ConfigureDefaultFtpData();
        }

        private void ConfigureDefaultFtpData()
        {
            FtpClientFactory ftpClientFactory = new FtpClientFactory(new FtpXboxSettings()
            {
                Host = Settings.XboxFtpHost,
                User = Settings.XboxFtpUser,
                Password = Settings.XboxFtpPassword
            });

            var ftpClient = ftpClientFactory.Create();

            ftpClient.Connect();

            if (ftpClient.DirectoryExists("F"))
            {
                ftpClient.DeleteDirectory("F");    
            }
            
            ftpClient.CreateDirectory("F/Games/GameA", true);
            
            ftpClient.SetWorkingDirectory("/F/Games/GameA");
            ftpClient.UploadFile("SampleFile.txt", "SampleFile.txt");
        }

        public static void ConfigureSettings()
        {
            Settings = new Settings()
            {
                XboxFtpHost = "localhost",
                XboxFtpUser = "xbox",
                XboxFtpPassword = "xbox"
            };
        }
    }
}