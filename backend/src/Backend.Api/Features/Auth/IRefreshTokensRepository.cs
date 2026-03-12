namespace Backend.Features.Auth;

public interface IRefreshTokensRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<RefreshToken> CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<RefreshToken> tokens, CancellationToken cancellationToken = default);
}
