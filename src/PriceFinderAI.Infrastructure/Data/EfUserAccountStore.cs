using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Data;

public sealed class EfUserAccountStore(AppDbContext db) : IUserAccountStore
{
    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> TryCreateAsync(User user, CancellationToken cancellationToken = default)
    {
        var exists = await db.Users.AnyAsync(u => u.NormalizedEmail == user.NormalizedEmail, cancellationToken);
        if (exists)
            return false;

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // Yarış koşulu: iki eşzamanlı kayıt aynı anda AnyAsync kontrolünü geçebilir.
            // Gerçek kaynak, veritabanındaki benzersiz indeks kısıtıdır (bkz. AppDbContext).
            db.Entry(user).State = EntityState.Detached;
            return false;
        }
    }
}
