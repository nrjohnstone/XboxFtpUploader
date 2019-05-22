using System;
using System.IO;
using Adapter.Persistence.Ftp;
using Serilog;
using XboxFtp.Core.UseCases;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XboxFtp.Console
{
    class Program
    {
        private static string _xboxSettingsDirectory;
        private static string _xboxSettingsPath;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();

            ConfigurePaths();

            Log.Information("Starting XBox FTP Upload");

            FtpXboxSettings ftpXboxSettings = LoadSettings();

            try
            {
                FtpClientFactory ftpClientFactory = new FtpClientFactory(ftpXboxSettings);

                IXboxGameRepositoryFactory xboxGameRepositoryFactory =
                    new FtpXboxGameRepositoryFactory(ftpClientFactory, ftpXboxSettings);
                UploadArchivesUseCase useCase = new UploadArchivesUseCase(xboxGameRepositoryFactory);

                if (ftpXboxSettings.GamesToUpload == null || ftpXboxSettings.GamesToUpload.Count == 0)
                {
                    Log.Warning("Found no games configured in settings to upload");
                    Environment.Exit(0);
                }
                useCase.Execute(ftpXboxSettings.GamesToUpload);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception occured");
                Environment.Exit(-1);
            }

            Log.Information("Finished XBox FTP Upload");
            Environment.Exit(0);
        }

        private static void ConfigurePaths()
        {
            _xboxSettingsDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XboxFtp");
            _xboxSettingsPath = Path.Join(_xboxSettingsDirectory, "local.yaml");
        }

        private static FtpXboxSettings LoadSettings()
        {
            FtpXboxSettings ftpXboxSettings;

            if (!Directory.Exists(_xboxSettingsDirectory))
            {
                Directory.CreateDirectory(_xboxSettingsDirectory);
            }

            if (!File.Exists(_xboxSettingsPath))
            {
                File.Copy("./local-template.yaml", _xboxSettingsPath);
            }
            
            using (Stream stream = new FileStream(_xboxSettingsPath, FileMode.Open, FileAccess.Read))
            {
                TextReader input = new StreamReader(stream);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();

                ftpXboxSettings = deserializer.Deserialize<FtpXboxSettings>(input);
            }

            return ftpXboxSettings;
        }
    }
}
