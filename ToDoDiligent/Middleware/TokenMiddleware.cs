using Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToDoDiligent.Services;

public class TokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public TokenMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        using (var scope = _serviceProvider.CreateScope()) // Create a scope
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>(); // Resolve TokenService
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(); // Resolve UserManager<AppUser>
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>(); // Resolve Configuration

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                var principal = GetPrincipalFromExpiredToken(token, config);
                if (principal != null)
                {
                    var email = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

                    if (email != null)
                    {
                        var user = await userManager.Users.Include(r => r.RefreshTokens)
                            .FirstOrDefaultAsync(x => x.Email == email);
                        if (user != null)
                        {
                            var refreshToken = context.Request.Cookies["refreshToken"];
                            var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

                            if (oldToken != null)
                            {
                                // Invalidate old refresh token
                                oldToken.Revoked = DateTime.UtcNow;

                                // Generate new tokens
                                var newJwtToken = tokenService.CreateToken(user);
                                var newRefreshToken = tokenService.GenerateRefreshToken();
                                user.RefreshTokens.Add(newRefreshToken);

                                await userManager.UpdateAsync(user);

                                // Add new tokens to the response
                                context.Response.Headers.Add("Authorization", $"Bearer {newJwtToken}");
                                context.Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
                                {
                                    HttpOnly = true,
                                    Expires = DateTime.UtcNow.AddDays(7),
                                    Secure = true,
                                    SameSite = SameSiteMode.Strict
                                });
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration config)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
            ValidateLifetime = false // Ignore token expiration
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
        {
            return principal;
        }

        return null;
    }
}


