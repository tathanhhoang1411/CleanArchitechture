using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Text.Json;
using IDatabase = StackExchange.Redis.IDatabase;
using CleanArchitecture.Application.IServices;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration; // Thêm namespace này

namespace CleanArchitecture.Application.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly TimeSpan _defaultCacheDuration;
        private readonly IConnectionMultiplexer _connection;
        public RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger, RabbitMQService rabbitMQ, IConfiguration configuration)
        {
            _database = connection.GetDatabase();
            _logger = logger;
            _connection = connection;
            // Đọc thời gian cache mặc định từ appsettings.json
            var defaultSeconds = configuration.GetValue<int>("Redis:DefaultCacheDurationInSeconds", 60);
            _defaultCacheDuration = TimeSpan.FromSeconds(defaultSeconds);
            // ✅ Đăng ký listener một lần duy nhất tại đây
            rabbitMQ.Subscribe(OnMessageReceived);
        }
        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("CommentUpdated:") ||
                message.StartsWith("CommentDeleted:") ||
                message.StartsWith("CommentCreate:"))
            {
                // Xóa toàn bộ cache liên quan đến comment
                ClearCacheByPrefix("comments:");
                Console.WriteLine($"🧹 [RedisCacheService] Cleared cache for prefix 'comments:' due to {message}");
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

            Console.WriteLine($"[Redis] Cleared cache with prefix: {prefix}");
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
            // Dùng thời gian mặc định nếu không truyền expiry
            var effectiveExpiry = expiry ?? _defaultCacheDuration;
            await _database.StringSetAsync(key, jsonData, effectiveExpiry);
        }
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

    }
}