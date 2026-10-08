namespace E_Voting_System.Helpers
{
    public static class IdMasker
    {
        public static string Mask(string? idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length < 7)
                return "****";

            // Shows first 6 digits + ** + last digit
            // Example: 050125****7
            return idNumber.Substring(0, 6) + "**" + idNumber[^1];
        }
    }
}
