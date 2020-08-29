using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataAccessLayer.Cryptography
{
    public class LoginToken: ILoginToken
    {
        private readonly IConfiguration _configuration;

        public LoginToken(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(LoginInput loginInput)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JWT").GetSection("Key").Value));
            SigningCredentials credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            Claim[] claim = new Claim[] 
            {
                new Claim(JwtRegisteredClaimNames.Email, loginInput.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            JwtSecurityToken token = new JwtSecurityToken
            (
                issuer: _configuration.GetSection("JWT").GetSection("Issuer").Value,
                audience: _configuration.GetSection("JWT").GetSection("Issuer").Value,
                claims: claim,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credential
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
