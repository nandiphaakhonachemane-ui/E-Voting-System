using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
