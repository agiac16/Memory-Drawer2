using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NetServer.Abstractions;
using NetServer.Models;

namespace NetServer.Abstractions
{
    public class TokenProvider : ITokenProvider
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;

        // pull from development file
        public TokenProvider(IConfiguration configuration)
        {
            _secret = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JWT Secret missing");
            _issuer = configuration["JwtSettings:Issuer"] ?? throw new ArgumentNullException("JWT Issuer missing");
            _audience = configuration["JwtSettings:Audience"] ?? throw new ArgumentNullException("JWT Audience missing");
        }

        public string GenerateToken(User user)
        {

            // ensure they exist
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.Id == null) throw new ArgumentNullException(nameof(user.Id));
            if (user.Username == null) throw new ArgumentNullException(nameof(user.Username));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}