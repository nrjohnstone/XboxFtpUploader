using System;

namespace XboxFtp.Core.Ports.Notification
{
    public interface IProgressNotifier
    {
        void GameAddedToUploadQueue(string gameName);
        void StartingGameUpload(string gameName);
        void FinishedGameUpload(string gameName, TimeSpan totalUploadTime);
        void GameUploadError(string gameName, Exception ex, string errorMessage);
    }
}