using System;
using Terminal.Gui;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;

namespace Adapter.Notifier.TerminalGui
{
    public class TerminalGuiProgressNotifier : IProgressNotifier
    {
        private readonly Label _currentFileLabel;
        private readonly Label _currentGameLabel;
        private readonly Label _statusLabel;
        private readonly Label _currentGamePercentCompleteLabel;

        public TerminalGuiProgressNotifier(Label currentFileLabel, Label currentGameLabel, Label statusLabel,
            Label currentGamePercentCompleteLabel)
        {
            _currentFileLabel = currentFileLabel;
            _currentGameLabel = currentGameLabel;
            _statusLabel = statusLabel;
            _currentGamePercentCompleteLabel = currentGamePercentCompleteLabel;
        }
        
        public void GameAddedToUploadQueue(string gameName)
        {
            SetStatusText($"Game added to upload queue : {gameName}");
        }

        public void StartingGameUpload(string gameName)
        {
            SetGameText(gameName);
            SetGamePercentCompleteText("0");
            SetStatusText(gameName);
        }

        private void SetStatusText(string gameName)
        {
            Application.MainLoop.Invoke (() => {
                _statusLabel.Text = $"Uploading game : {gameName}";
            });
        }

        private void SetGamePercentCompleteText(string percent)
        {
            Application.MainLoop.Invoke (() => {
                _currentGamePercentCompleteLabel.Text = $"Complete: {percent}%";
            });
        }

        private void SetGameText(string gameName)
        {
            Application.MainLoop.Invoke (() => {
                _currentGameLabel.Text = $"Current Game: {gameName}";
            });
        }

        public void FinishedGameUpload(string gameName, TimeSpan totalUploadTime)
        {
            SetStatusText($"Finished game upload : {gameName}");
            SetGamePercentCompleteText("100");
        }

        public void GameUploadError(string gameName, Exception ex, string errorMessage)
        {
            SetStatusText($"Error with game upload : {gameName}");
        }

        public void ReportTotalFilesToTransfer(string gameName, int count)
        {
        }

        public void ExtractFileToDisk(string gameName, string fileName)
        {
        }

        public void AddingToUploadQueue(string gameName, string fileName)
        {
        }

        public void CreateFolderStructure(string gameName)
        {
            SetStatusText("Creating folder structure");
        }

        public void FinishedCreatingFolderStructure(string gameName)
        {
            SetStatusText("");
        }

        public void CheckingForUploadedFiles(string gameName)
        {
            SetStatusText("Checking for uploaded files");
        }

        public void CheckingForUploadedFile(string gameName, string fileName)
        {
            SetStatusText($"Checking for uploaded files - {fileName}");
        }

        public void FileAlreadyExists(string gameName, string fileName)
        {
        }

        public void WaitingForUploadsToComplete(string gameName)
        {
        }

        public void ReportTotalBytesToUpload(string gameName, long totalBytesToUpload)
        {
        }

        public void FinishedFileUpload(string gameName, IXboxTransferRequest item, int percentComplete)
        {
            SetGameText($"Current Game: {gameName}");
            SetGamePercentCompleteText($"{percentComplete}");
        }

        public void StartingFileUpload(string gameName, string itemPath)
        {
            Application.MainLoop.Invoke (() => {
                _currentFileLabel.Text = $"Current File: {itemPath}";
                _statusLabel.Text = "Uploading files";
            });
        }
    }
}
