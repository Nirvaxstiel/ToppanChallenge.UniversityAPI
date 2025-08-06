using Microsoft.AspNetCore.Identity;

namespace UniversityAPI.Tests.Shared.Helpers
{
    public static class TestPasswordGenerator
    {
        public static string GeneratePassword(PasswordOptions options)
        {
            var rand = new Random();
            var password = new List<char>();

            if (options.RequireUppercase)
            {
                password.Add(GetRandomChar("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            }

            if (options.RequireLowercase)
            {
                password.Add(GetRandomChar("abcdefghijklmnopqrstuvwxyz"));
            }

            if (options.RequireDigit)
            {
                password.Add(GetRandomChar("0123456789"));
            }

            if (options.RequireNonAlphanumeric)
            {
                password.Add(GetRandomChar("!@#$%^&*"));
            }

            var all = string.Empty;

            if (options.RequireUppercase)
            {
                all += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            }

            if (options.RequireLowercase)
            {
                all += "abcdefghijklmnopqrstuvwxyz";
            }

            if (options.RequireDigit)
            {
                all += "0123456789";
            }

            if (options.RequireNonAlphanumeric)
            {
                all += "!@#$%^&*";
            }

            if (string.IsNullOrEmpty(all))
            {
                all = "abcdefghijklmnopqrstuvwxyz";
            }

            while (password.Count < options.RequiredLength)
            {
                password.Add(GetRandomChar(all));
            }

            var uniqueChars = new HashSet<char>(password);
            while (uniqueChars.Count < options.RequiredUniqueChars)
            {
                var c = GetRandomChar(all);
                if (uniqueChars.Add(c))
                {
                    password.Add(c);
                }
            }

            return new string(password.OrderBy(_ => rand.Next()).ToArray());
        }

        private static char GetRandomChar(string from) =>
            from[new Random().Next(from.Length)];
    }

}