using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniversityAPI.Framework.Model;
using UniversityAPI.Service;

namespace UniversityAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserDM> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<UserDM> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized();

            var token = await _tokenService.GenerateToken(user);
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
            var user = new UserDM { UserName = registerDto.Username, Email = registerDto.Email };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user, "User");
            var token = await _tokenService.GenerateToken(user);

            return new AuthResponse
            {
                Username = user.UserName,
                Email = user.Email,
                Token = token
            };
        }
    }
}