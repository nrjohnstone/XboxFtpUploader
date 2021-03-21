using System;
using FluentFTP;
using Polly;
using Polly.Registry;
using Polly.Retry;
using XboxFtp.Core.Ports.Persistence;
using XboxFtp.Core.UseCases;

namespace Adapter.Persistence.Ftp
{
    public class FtpXboxGameRepositoryFactory : IXboxGameRepositoryFactory
    {
        private readonly FtpClientFactory _ftpClientFactory;
        private readonly FtpXboxSettings _ftpXboxSettings;

        public FtpXboxGameRepositoryFactory(FtpClientFactory ftpClientFactory, FtpXboxSettings ftpXboxSettings)
        {
            _ftpClientFactory = ftpClientFactory;
            _ftpXboxSettings = ftpXboxSettings;
        }

        public IXboxGameRepository Create()
        {
            Policy ftpPolicy = Policy.Handle<FtpException>().RetryForever((exception) =>
            {
                if (exception.InnerException is TimeoutException)
                {
                    return;
                }
                
                if (exception is FtpCommandException commandException)
                {
                    Console.WriteLine($"FtpException code: {commandException.CompletionCode}");
                    if (commandException.ResponseType != FtpResponseType.TransientNegativeCompletion)
                    {
                        throw new PersistenceException("Non transient error occurred", false, commandException);
                    }
                }
                
                throw new PersistenceException("Non transient error occurred", false, exception);
            });
            PolicyRegistry policyRegistry = new PolicyRegistry();
            policyRegistry.Add("Ftp", ftpPolicy);
            
            var ftpXboxGameRepository = new FtpXboxGameRepository(_ftpClientFactory, _ftpXboxSettings, policyRegistry);
            
            return ftpXboxGameRepository;
        }
    }
    
    
}