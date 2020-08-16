using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataAccessLayer.Cryptography
{
    public class LoginToken: ILoginToken
    {
        private readonly IConfiguration _config;
        public LoginToken(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateToken(LoginInput loginInput)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JWT").GetSection("Key").Value));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            var claimEmail = new Claim[] {
                new Claim(JwtRegisteredClaimNames.Email, loginInput.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };
            var token = new JwtSecurityToken(
                issuer: _config.GetSection("JWT").GetSection("Issuer").Value,
                audience: _config.GetSection("JWT").GetSection("Issuer").Value,
                claimEmail,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
