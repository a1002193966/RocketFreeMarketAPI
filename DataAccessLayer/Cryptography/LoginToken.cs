using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Cryptography
{
    public class LoginToken: ILoginToken
    {
        private readonly IConfiguration _configuration;

        public LoginToken(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateToken(LoginInput loginInput)
        {
            try
            {
                SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
                SigningCredentials credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
                Claim[] claim = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, loginInput.Email),
                    new Claim(JwtRegisteredClaimNames.NameId, await getAccountID(loginInput.Email)),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                JwtSecurityToken token = new JwtSecurityToken
                (
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Issuer"],
                    claims: claim,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: credential
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex) { throw; }
        }



        private async Task<string> getAccountID(string email)
        {
            using SqlConnection sqlcon = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            string query = "SELECT AccountID FROM [Account] WHERE NormalizedEmail = @Email";
            using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
            sqlcmd.Parameters.AddWithValue("@Email", email.ToUpper());
            string id = string.Empty;
            try
            {
                sqlcon.Open();
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    id = (string)reader["AccountID"];
                }
                return id;
            }
            catch (Exception ex) { throw; }
        }
    }
}
