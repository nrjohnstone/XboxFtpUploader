using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Adapter.Notifier.Serilog;
using Adapter.Notifier.TerminalGui;
using Adapter.Persistence.Ftp;
using Adapter.Persistence.InMemory;
using Serilog;
using XboxFtp.Console.Configuration.Logging;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.UseCases;

namespace XboxFtp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            SettingsLoaderIni settingsLoader = new SettingsLoaderIni(args);
            var settings = settingsLoader.Load();

            Log.Logger = SerilogConfiguration.Create("XboxFtpUpload", settings).CreateLogger();

            Log.Information("Starting XBox FTP Upload");

            SerilogProgressNotifier serilogNotifier = new SerilogProgressNotifier(Log.Logger);
            TerminalGuiAdapter adapter = new TerminalGuiAdapter();
            adapter.Initialize();

            TerminalGuiProgressNotifier terminalGuiProgressNotifier = adapter.CreateNotifier();
            
            IProgressNotifier notifier = new ChainedProgressNotifier(new List<IProgressNotifier>()
            {
                terminalGuiProgressNotifier
            });
            
            try
            {
                IXboxGameRepositoryFactory xboxGameRepositoryFactory = null;

                if (settings.TestMode)
                {
                    xboxGameRepositoryFactory = UseInMemoryAdapter();
                }
                else
                {
                    FtpXboxSettings xboxFtpsettings = new FtpXboxSettings()
                    {
                        Host = settings.Host,
                        Password = settings.Password,
                        User = settings.User,
                        Port = settings.Port,
                        GameRootDirectory = settings.GameRootDirectory
                    };
                    
                    xboxGameRepositoryFactory = UseFtpAdapter(xboxFtpsettings);
                }

                UploadArchivesUseCase useCase = new UploadArchivesUseCase(xboxGameRepositoryFactory, notifier);
                
                List<string> gamesToUpload = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(settings.GameToUpload))
                {
                    Log.Warning("Found single games for upload specified");
                    gamesToUpload.Add(settings.GameToUpload);
                }
                else if (!string.IsNullOrWhiteSpace(settings.GamesToUploadFile))
                {
                    if (!File.Exists(settings.GamesToUploadFile))
                    {
                        Log.Warning("File specified in GameToUploadFile does not exist");
                        adapter.Shutdown();
                        Environment.Exit(-1);
                    }

                    gamesToUpload = File.ReadAllLines(settings.GamesToUploadFile).Select(x => RemoveSurroundingQuotes(x))
                        .ToList();
                }
                else if (settings.GamesToUpload == null || settings.GamesToUpload.Count == 0)
                {
                    Log.Warning("Found no games configured to upload");
                    adapter.Shutdown();
                    Environment.Exit(0);
                }
                else
                {
                    gamesToUpload = settings.GamesToUpload;
                }
                
                useCase.Execute(gamesToUpload);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception occured");
                adapter.Shutdown();
                Environment.Exit(-1);
            }

            Log.Information("Finished XBox FTP Upload");
            adapter.Shutdown();
            Environment.Exit(0);
        }

        private static string RemoveSurroundingQuotes(string s)
        {
            return s.Trim('"');
        }

        private static IXboxGameRepositoryFactory UseInMemoryAdapter()
        {
            var factory = new XboxGameRepositoryFactory(new Dictionary<string, long>(), TimeSpan.FromMilliseconds(1000));
            return factory;
        }

        private static IXboxGameRepositoryFactory UseFtpAdapter(FtpXboxSettings ftpXboxSettings)
        {
            FtpClientFactory ftpClientFactory = new FtpClientFactory(ftpXboxSettings);

            IXboxGameRepositoryFactory xboxGameRepositoryFactory =
                new FtpXboxGameRepositoryFactory(ftpClientFactory, ftpXboxSettings);
            return xboxGameRepositoryFactory;
        }
    }
}
