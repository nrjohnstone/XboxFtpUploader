using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XboxFtp.Console.Configuration.Logging
{
    public class SerilogConfiguration
    {
        public static LoggerConfiguration Create(string applicationName, Settings settings)
        {
            string tempPath = Path.GetTempPath();
            string logPath = Path.Combine(tempPath, "LogBuffers", applicationName);

            var configuration = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithProcessName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message,-30:lj} {Properties:j}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Literate);

            if (!string.IsNullOrWhiteSpace(settings.HttpLogEndpoint))
            {
                configuration.WriteTo.DurableHttpUsingFileSizeRolledBuffers(settings.HttpLogEndpoint, logPath);
            }                

            return configuration;
        }
    }
}
