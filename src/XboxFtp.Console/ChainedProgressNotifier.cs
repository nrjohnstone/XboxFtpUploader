using System;
using System.Collections.Generic;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;

namespace XboxFtp.Console
{
    public class ChainedProgressNotifier : IProgressNotifier
    {
        private readonly List<IProgressNotifier> _progressNotifiers;

        public ChainedProgressNotifier(List<IProgressNotifier> progressNotifiers)
        {
            _progressNotifiers = progressNotifiers;
        }
        
        public void GameAddedToUploadQueue(string gameName)
        {
            _progressNotifiers.ForEach(x => x.GameAddedToUploadQueue(gameName));
        }

        public void StartingGameUpload(string gameName)
        {
            _progressNotifiers.ForEach(x => x.StartingGameUpload(gameName));
        }

        public void FinishedGameUpload(string gameName, TimeSpan totalUploadTime)
        {
            _progressNotifiers.ForEach(x => x.FinishedGameUpload(gameName, totalUploadTime));
        }

        public void GameUploadError(string gameName, Exception ex, string errorMessage)
        {
            _progressNotifiers.ForEach(x => x.GameUploadError(gameName, ex, errorMessage));
        }

        public void ReportTotalFilesToTransfer(string gameName, int count)
        {
            _progressNotifiers.ForEach(x => x.ReportTotalFilesToTransfer(gameName, count));
        }

        public void ExtractFileToDisk(string gameName, string fileName)
        {
            _progressNotifiers.ForEach(x => x.ExtractFileToDisk(gameName, fileName));
        }

        public void AddingToUploadQueue(string gameName, string fileName)
        {
            _progressNotifiers.ForEach(x => x.AddingToUploadQueue(gameName, fileName));
        }

        public void CreateFolderStructure(string gameName)
        {
            _progressNotifiers.ForEach(x => x.CreateFolderStructure(gameName));
        }

        public void FinishedCreatingFolderStructure(string gameName)
        {
            _progressNotifiers.ForEach(x => x.FinishedCreatingFolderStructure(gameName));
        }

        public void CheckingForUploadedFiles(string gameName)
        {
            _progressNotifiers.ForEach(x => x.CheckingForUploadedFiles(gameName));
        }

        public void CheckingForUploadedFile(string gameName, string fileName)
        {
            _progressNotifiers.ForEach(x => x.CheckingForUploadedFile(gameName, fileName));
        }

        public void FileAlreadyExists(string gameName, string fileName)
        {
            _progressNotifiers.ForEach(x => x.FileAlreadyExists(gameName, fileName));
        }

        public void WaitingForUploadsToComplete(string gameName)
        {
            _progressNotifiers.ForEach(x => x.WaitingForUploadsToComplete(gameName));
        }

        public void ReportTotalBytesToUpload(string gameName, long totalBytesToUpload)
        {
            _progressNotifiers.ForEach(x => x.ReportTotalBytesToUpload(gameName, totalBytesToUpload));
        }

        public void FinishedFileUpload(string gameName, IXboxTransferRequest item, int percentComplete)
        {
            _progressNotifiers.ForEach(x => x.FinishedFileUpload(gameName, item, percentComplete));
        }

        public void StartingFileUpload(string gameName, string itemPath)
        {
            _progressNotifiers.ForEach(x => x.StartingFileUpload(gameName, itemPath));
        }
    }
}