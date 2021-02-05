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
            _statusLabel.Text = $"Game added to upload queue : {gameName}";
            Application.Refresh();
        }

        public void StartingGameUpload(string gameName)
        {
            _currentGameLabel.Text = $"Current Game: {gameName}";
            _currentGamePercentCompleteLabel.Text = "Complete: 0%";
            _statusLabel.Text = $"Uploading game : {gameName}";
            Application.Refresh();
        }

        public void FinishedGameUpload(string gameName, TimeSpan totalUploadTime)
        {
            _statusLabel.Text = $"Finished game upload : {gameName}";
            _currentGamePercentCompleteLabel.Text = "Complete: 100%";
            Application.Refresh();
        }

        public void GameUploadError(string gameName, Exception ex, string errorMessage)
        {
            _statusLabel.Text = $"Error with game upload : {gameName}";
            Application.Refresh();
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
            _statusLabel.Text = $"Creating folder structure";
            Application.Refresh();
        }

        public void FinishedCreatingFolderStructure(string gameName)
        {
            _statusLabel.Text = $"";
            Application.Refresh();
        }

        public void CheckingForUploadedFiles(string gameName)
        {
            _statusLabel.Text = $"Checking for uploaded files";
            Application.Refresh();
        }

        public void CheckingForUploadedFile(string gameName, string fileName)
        {
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
            _currentGameLabel.Text = $"Current Game: {gameName}";
            _currentGamePercentCompleteLabel.Text = $"Complete: {percentComplete}%";
            Application.Refresh();
        }

        public void StartingFileUpload(string gameName, string itemPath)
        {
            _currentFileLabel.Text = $"Current File: {itemPath}";
            Application.Refresh();
        }
    }
}
