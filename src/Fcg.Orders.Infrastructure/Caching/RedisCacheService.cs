using Fcg.Orders.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace Fcg.Orders.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redisConnection;

        public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redisConnection)
        {
            _cache = cache;
            _redisConnection = redisConnection;
        }


        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task SetAsync<T>(string chave, T Amount, TimeSpan tempoExpiracao,
            CancellationToken cancellationToken)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = tempoExpiracao
            };

            var jsonData = JsonSerializer.Serialize(Amount);
            await _cache.SetStringAsync(chave, jsonData, options, cancellationToken);
        }

        public async Task<T?> GetAsync<T>(string chaveCache, CancellationToken cancellationToken)
        {
            var cacheData = await _cache.GetStringAsync(chaveCache, cancellationToken);
            if (string.IsNullOrEmpty(cacheData))
                return default;

            return JsonSerializer.Deserialize<T>(cacheData, _jsonOptions);
        }

        public async Task RemoveAsync(string chaveCache)
        {
            await _cache.RemoveAsync(chaveCache);
        }

        public async Task RemoveByPrefixAsync(string prefixo)
        {
            var endpoints = _redisConnection.GetEndPoints();
            var server = _redisConnection.GetServer(endpoints.First());
            var chaves = server.Keys(pattern: $"{prefixo}*").ToArray();

            if (chaves.Any())
            {
                var db = _redisConnection.GetDatabase();
                await db.KeyDeleteAsync(chaves);
            }
        }
    }
}
