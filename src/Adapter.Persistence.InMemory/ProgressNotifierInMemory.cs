using System;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;

namespace Adapter.Persistence.InMemory
{
    public class ProgressNotifierInMemory : IProgressNotifier
    {
        public void GameAddedToUploadQueue(string gameName)
        {
        }

        public void StartingGameUpload(string gameName)
        {
        }

        public void FinishedGameUpload(string gameName, TimeSpan totalUploadTime)
        {
        }

        public void GameUploadError(string gameName, Exception ex, string errorMessage)
        {
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
        }

        public void FinishedCreatingFolderStructure(string gameName)
        {
        }

        public void CheckingForUploadedFiles(string gameName)
        {
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
        }

        public void StartingFileUpload(string gameName, string itemPath)
        {
        }
    }
}