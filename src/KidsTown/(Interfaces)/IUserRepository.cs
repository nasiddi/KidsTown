using System.Threading.Tasks;

namespace KidsTown.KidsTown;

public interface IUserRepository
{
    Task<bool> IsValidLogin(string username, string passwordHash);
}