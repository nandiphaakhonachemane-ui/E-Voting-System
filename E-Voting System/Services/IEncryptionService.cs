using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}

