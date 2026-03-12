using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Auth;

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
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == tokenHash, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
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
