namespace Evently.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetAsync(
            string email,
            string firstName,
            string resetUrl);
    }
}