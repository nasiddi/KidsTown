namespace KidsTown;

public interface IUserRepository
{
    Task<bool> IsValidLogin(string username, string passwordHash);
}