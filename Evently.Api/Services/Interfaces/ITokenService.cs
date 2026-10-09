using Evently.Api.Models;

namespace Evently.Api.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}