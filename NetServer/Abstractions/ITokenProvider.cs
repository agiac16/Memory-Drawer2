namespace NetServer.Abstractions;

using NetServer.Models;

public interface ITokenProvider
{
    string GenerateToken(User user);
}