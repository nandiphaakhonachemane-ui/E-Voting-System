using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(string userId, string email, string? phoneNumber = null);
        Task<bool> ValidateOtpAsync(string userId, string code);
    }
}
