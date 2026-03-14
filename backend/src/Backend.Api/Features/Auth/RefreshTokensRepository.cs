using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Auth;

// TODO: Add a background hosted service to periodically purge expired/revoked refresh tokens.
// Expired tokens accumulate over time and degrade query performance. A scheduled job
// (e.g., IHostedService running daily) should delete tokens where ExpiresAt < UtcNow
// and RevokedAt is not null, to maintain security and performance.

public class RefreshTokensRepository : IRefreshTokensRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokensRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<RefreshToken> tokens, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.UpdateRange(tokens);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
