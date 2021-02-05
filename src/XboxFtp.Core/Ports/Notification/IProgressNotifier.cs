using System;
using XboxFtp.Core.Entities;
using XboxFtp.Core.UseCases;

namespace XboxFtp.Core.Ports.Notification
{
    public interface IProgressNotifier
    {
        void GameAddedToUploadQueue(string gameName);
        void StartingGameUpload(string gameName);
        void FinishedGameUpload(string gameName, TimeSpan totalUploadTime);
        void GameUploadError(string gameName, Exception ex, string errorMessage);
        void ReportTotalFilesToTransfer(string gameName, int count);
        void ExtractFileToDisk(string gameName, string fileName);
        void AddingToUploadQueue(string gameName, string fileName);
        void CreateFolderStructure(string gameName);
        void FinishedCreatingFolderStructure(string gameName);
        void CheckingForUploadedFiles(string gameName);
        void CheckingForUploadedFile(string gameName, string fileName);
        void FileAlreadyExists(string gameName, string fileName);
        void WaitingForUploadsToComplete(string gameName);
        void ReportTotalBytesToUpload(string gameName, long totalBytesToUpload);
        void FinishedFileUpload(string gameName, IXboxTransferRequest item, int percentComplete);
        void StartingFileUpload(string gameName, string itemPath);
    }
}