using Serilog;
using ILogger = Serilog.ILogger;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace BE_2911_CleanArchitechture.Logging
{
    public class CustomLogger : ICustomLogger
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new();
        private readonly string _logDirectory;

        public CustomLogger()
        {
            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void LogInformation(string userId, string message)
        {
            var logger = GetLogger(userId);
            logger.Information(message);
        }

        public void LogError(string userId, string message, Exception ex)
        {
            var logger = GetLogger(userId);
            logger.Error(ex, message);
        }

        public void LogDebug(string userId, string message)
        {
            var logger = GetLogger(userId);
            logger.Debug(message);
        }

        private ILogger GetLogger(string userId)
        {
            string logFileName = $"{userId}_.log";
            string logFilePath = Path.Combine(_logDirectory, logFileName);

            return _loggers.GetOrAdd($"{userId}_{DateTime.UtcNow:yyyy-MM-dd}", _ =>
            {
                return new LoggerConfiguration()
                    .ReadFrom.Configuration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build())
                    .Enrich.FromLogContext()
                    .WriteTo.File(logFilePath,
                                  rollingInterval: RollingInterval.Day, // Rolling theo ngày
                                  rollOnFileSizeLimit: true) // Tạo file mới nếu vượt quá kích thước
                    .CreateLogger();
            });
        }
    }
}