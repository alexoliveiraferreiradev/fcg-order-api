namespace Fcg.Orders.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string chaveCache, CancellationToken cancellation);
        Task SetAsync<T>(string chave, T Amount, TimeSpan expirationTime, CancellationToken cancellation);
        Task RemoveAsync(string chaveCache);
        Task RemoveByPrefixAsync(string prefixo);
    }
}
