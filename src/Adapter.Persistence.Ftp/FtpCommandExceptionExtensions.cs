using FluentFTP;

namespace Adapter.Persistence.Ftp
{
    internal static class FtpCommandExceptionExtensions
    {
        public static bool IsTransient(this FtpCommandException ex)
        {
            if (ex.CompletionCode == "530") // Authentication error
            {
                return false;
            }

            return true;
        }
    }
}