using Fcg.Orders.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Fcg.Orders.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redisConnection,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _redisConnection = redisConnection;
            _logger = logger;
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
            {
                _logger.LogDebug("[OrderAPI] [Cache] Miss para a chave {ChaveCache}", chaveCache);
                return default;
            }

            _logger.LogDebug("[OrderAPI] [Cache] Hit para a chave {ChaveCache}", chaveCache);

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

            _logger.LogDebug(
                "[OrderAPI] [Cache] {QuantidadeChaves} chave(s) removida(s) para o prefixo {Prefixo}",
                chaves.Length, prefixo);
        }
    }
}
