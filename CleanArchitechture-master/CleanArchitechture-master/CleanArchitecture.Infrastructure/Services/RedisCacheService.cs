using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;
using CleanArchitecture.Application.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Infrastructure.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly TimeSpan _defaultCacheDuration;
        private readonly IConnectionMultiplexer _connection;
        public RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger, IRabbitMQService rabbitMQ, IConfiguration configuration)
        {
            _database = connection.GetDatabase();
            _logger = logger;
            _connection = connection;
            var defaultSeconds = configuration.GetValue<int>("Redis:DefaultCacheDurationInSeconds", 60);
            _defaultCacheDuration = TimeSpan.FromSeconds(defaultSeconds);
            
            // ✅ Prevent Crash: Try to subscribe, but ignore if fails
            try
            {
                rabbitMQ.Subscribe(OnMessageReceived);
            }
            catch (Exception ex)
            {
                // Log error but allow app to start
                Console.WriteLine($"RabbitMQ Error: {ex.Message}");
            }
        }
        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("CommentUpdated:") ||
                message.StartsWith("CommentDeleted:") ||
                message.StartsWith("CommentCreate:"))
            {
                ClearCacheByPrefix("comments:").GetAwaiter().GetResult();
            }
            if (message.StartsWith("ReviewDelete:") ||
                message.StartsWith("ReviewCreate:"))
            {
                ClearCacheByPrefix("reviews:").GetAwaiter().GetResult();
            }
            if (message.StartsWith("UsersDelete:") ||
                message.StartsWith("UsersUpdate:") ||
                message.StartsWith("UsersCreate:"))
            {
                ClearCacheByPrefix("users:").GetAwaiter().GetResult();
            }
        }
        public async Task ClearCacheByPrefix(string prefix)
        {
            var endpoints = _connection.GetEndPoints();
            var server = _connection.GetServer(endpoints.First());
            var keys = server.Keys(pattern: $"{prefix}*");

            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (value.IsNullOrEmpty) return default;
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis deserialization error for key {key}");
                return default;
            }
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var jsonData = JsonSerializer.Serialize(value);
            var effectiveExpiry = expiry ?? _defaultCacheDuration;
            await _database.StringSetAsync(key, jsonData, effectiveExpiry);
        }
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
