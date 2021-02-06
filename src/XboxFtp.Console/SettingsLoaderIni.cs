﻿using System;
using System.IO;
using Adapter.Persistence.Ftp;
using Microsoft.Extensions.Configuration;

namespace XboxFtp.Console
{
    internal class SettingsLoaderIni
    {
        private readonly string[] _args;

        public SettingsLoaderIni(string[] args)
        {
            _args = args;
        }
        
        public Settings Load()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            AddDefaults(configurationBuilder);
            AddEnvironmentVariables(configurationBuilder);
            AddUserSettings(configurationBuilder);
            
            configurationBuilder.AddCommandLine(_args);
            
            var settings = new Settings();
            configurationBuilder.Build().Bind(settings);
            return settings;
        }

        private void AddUserSettings(IConfigurationBuilder configurationBuilder)
        {
            var xboxSettingsDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "XboxFtp");
            var xboxSettingsPath = Path.Join(xboxSettingsDirectory, "settings.ini");
            
            configurationBuilder.AddIniFile(xboxSettingsPath, true);
        }

        private static void AddEnvironmentVariables(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddEnvironmentVariables();
        }

        private void AddDefaults(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddIniFile("settings.ini");
        }
    }
}