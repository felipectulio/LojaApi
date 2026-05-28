using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LojaApi.Auth;

namespace LojaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string chaveJwt = "MINHA_CHAVE_SUPER_SECRETA_123456";

        [HttpPost("login")]
        public IActionResult Login(Login login)
        {
            if (login.Usuario == "admin" && login.Senha == "123")
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(chaveJwt);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, login.Usuario) }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key), 
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new { token = tokenString });
            }

            return Unauthorized("Usuário ou senha inválidos");
        }
    }
}