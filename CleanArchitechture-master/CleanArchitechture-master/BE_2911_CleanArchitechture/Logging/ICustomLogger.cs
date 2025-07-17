namespace BE_2911_CleanArchitechture.Logging
{
    public interface ICustomLogger
    {
        void LogInformation(string userId, string message);
        void LogError(string userId, string message, Exception ex);
        void LogDebug(string userId, string message);
    }
}
