namespace UserManagementService.Services.Interfacces
{
    public interface ILoggingFactory<T>
    {
        void LogInfo(string header, string message, params string[] messageParams);
        void LogWarning(string header, string message, params string[] messageParams);
        void LogError(string header, string message, params string[] messageParams);
        void LogDebug(string header, string message, params string[] messageParams);
        void LogTrace(string header, string message, params string[] messageParams);
    }
}
