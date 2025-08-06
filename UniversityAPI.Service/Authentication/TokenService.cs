using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniversityAPI.Framework.Model;
using UniversityAPI.Utility.Interfaces;

namespace UniversityAPI.Service
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserDM> userManager;
        private readonly IConfigHelper configHelper;

        public TokenService(IConfigHelper configHelper, UserManager<UserDM> userManager)
        {
            this.userManager = userManager;
            this.configHelper = configHelper;
        }

        public async Task<string> GenerateToken(UserDM user)
        {
            var jwtKey = configHelper.GetJwtKey<string>();
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT signing key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = configHelper.GetValue<string>("Jwt:Issuer"),
                Audience = configHelper.GetValue<string>("Jwt:Audience")
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}