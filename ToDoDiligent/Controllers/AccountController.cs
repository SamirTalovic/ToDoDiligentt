using Application.Interfaces;
using Domains;
using Domains.ModelDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ToDoDiligent.DTOs;
using ToDoDiligent.Services;

namespace ToDoDiligent.Controllers
{
    
        [ApiController]
        [Route("api/[controller]")]
        public class AccountController : BaseApiController
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly SignInManager<AppUser> _signInManager;
            private readonly TokenService _tokenService;
            private readonly IUserAccessor _userAccessor;

            public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService, IUserAccessor userAccessor)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _tokenService = tokenService;
                _userAccessor = userAccessor;
            }

            [AllowAnonymous]
            [HttpPost("login")]
            public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);

                if (user == null) return Unauthorized("Email not found");

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (result.Succeeded)
                {
                    await SetRefreshToken(user);
                    return CreateUserObject(user);
                }

                return Unauthorized("Invalid password");
            }

            [AllowAnonymous]
            [HttpPost("register")]
            public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
            {
                if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
                {
                    ModelState.AddModelError("email", "Email taken");
                    return ValidationProblem();
                }

               
                var user = new AppUser
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    UserName = registerDto.Email,
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    await SetRefreshToken(user);
                    return CreateUserObject(user);
                }

                return BadRequest("Problem registering user");
            }

            [Authorize]
            [HttpGet]
            public async Task<ActionResult<UserDto>> GetCurrentUser()
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Email == _userAccessor.GetEmail());

                if (user == null) return NotFound();

                await SetRefreshToken(user);
                return CreateUserObject(user);
            }

            [Authorize]
            [HttpPost("refreshToken")]
            public async Task<ActionResult<UserDto>> RefreshToken()
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var user = await _userManager.Users
                    .Include(r => r.RefreshTokens)
                    .FirstOrDefaultAsync(x => x.Email == _userAccessor.GetEmail());

                if (user == null) return Unauthorized();

                var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);

                if (oldToken != null && !oldToken.IsActive) return Unauthorized();

                return CreateUserObject(user);
            }

            private async Task SetRefreshToken(AppUser user)
            {
                var refreshToken = _tokenService.GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(7)
                };

                Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            }

            private UserDto CreateUserObject(AppUser user)
            {
                return new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user)
                };
            }
        }
}
