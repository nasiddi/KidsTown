using System.Threading.Tasks;
using KidsTown.KidsTown;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database;

public class UserRepository(IServiceScopeFactory serviceScopeFactory) : IUserRepository
{
    public async Task<bool> IsValidLogin(string username, string passwordHash)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var user = await db.Users
            .SingleOrDefaultAsync(u => u.Username == username && u.PasswordHash == passwordHash);

        return user != null;
    }
}