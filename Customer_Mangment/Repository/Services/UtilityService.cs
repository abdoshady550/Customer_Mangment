namespace Customer_Mangment.Repository.Services
{
    public class UtilityService
    {
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            var atIndex = email.IndexOf('@');

            if (atIndex < 0)
                return "****";

            if (atIndex <= 1)
                return $"****{email[atIndex..]}";

            return $"{email[0]}****{email[atIndex - 1]}{email[atIndex..]}";
        }
    }
}
