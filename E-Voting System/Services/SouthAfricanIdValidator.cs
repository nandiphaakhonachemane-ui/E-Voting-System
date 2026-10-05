using Microsoft.AspNetCore.Mvc;

namespace E_Voting_System.Services
{
    public static class SouthAfricanIdValidator
    {
        public static bool IsValid(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13 || !idNumber.All(char.IsDigit))
                return false;

            // Basic date check (YYMMDD)
            if (!DateTime.TryParseExact(idNumber.Substring(0, 6), "yyMMdd",
                null, System.Globalization.DateTimeStyles.None, out _))
                return false;

            // Luhn-style checksum used by SA ID
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = idNumber[i] - '0';
                if (i % 2 == 0)
                    sum += digit;
                else
                {
                    int doubled = digit * 2;
                    sum += doubled > 9 ? doubled - 9 : doubled;
                }
            }
            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (idNumber[12] - '0');
        }

        public static DateTime? ExtractDateOfBirth(string idNumber)
        {
            if (!IsValid(idNumber)) return null;
            if (DateTime.TryParseExact(idNumber.Substring(0, 6), "yyMMdd",
                null, System.Globalization.DateTimeStyles.None, out var dob))
            {
                // Adjust century (simple heuristic)
                if (dob.Year > DateTime.Now.Year - 10)
                    dob = dob.AddYears(-100);
                return dob;
            }
            return null;
        }

        public static int ExtractAge(string idNumber)
        {
            var dob = ExtractDateOfBirth(idNumber);
            if (!dob.HasValue) return 0;
            var today = DateTime.Today;
            int age = today.Year - dob.Value.Year;
            if (dob.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
