using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniversityAPI.Framework.Infrastructure.Transactions;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;

namespace UniversityAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    [Transactional]
    public class AuthController(UserManager<UserDM> userManager, ITokenService tokenService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userManager.FindByNameAsync(loginDto.Username);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized();

            var token = await tokenService.GenerateToken(user);
            return new AuthResponse
            {
                Username = user.UserName,
                Email = user.Email,
                Token = token
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUserByUsername = await userManager.FindByNameAsync(registerDto.Username);
            if (existingUserByUsername != null)
            {
                return Conflict(new { message = "Username already exists" });
            }

            var existingUserByEmail = await userManager.FindByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null)
            {
                return Conflict(new { message = "Email already exists" });
            }

            var user = new UserDM { UserName = registerDto.Username, Email = registerDto.Email };
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            await userManager.AddToRoleAsync(user, "User");
            var token = await tokenService.GenerateToken(user);

            return new AuthResponse
            {
                Username = user.UserName,
                Email = user.Email,
                Token = token
            };
        }
    }
}