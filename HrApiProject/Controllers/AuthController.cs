using HrApiProject.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HrApiProject.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        // la interfaz IConfiguration nos permita acceder a la configuración appseting.json
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]

        public IActionResult Login(LoginDto loginDto)
        {
            string rolAsignado;
            if(loginDto.Username == "admin" && loginDto.Password == "12345")
            {
                rolAsignado = "Admin";

            }else if(loginDto.Username == "empleado"  && loginDto.Password == "123456"){

                rolAsignado = "user";
            }
            else
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

            }

            // mapear el payload con la información que queremos que viaje
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, loginDto.Username),
                new Claim(ClaimTypes.Role, rolAsignado),
                new Claim(ClaimTypes.Country,"Honduras")
            };

            //signature
           
            var jwtSettings = _configuration.GetSection("jwtsettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("SecretKey")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            //emsamblar el jwt
            var token = new JwtSecurityToken(
                issuer: jwtSettings.GetValue<string>("Issuer"),
                audience: jwtSettings.GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(jwtSettings.GetValue<int>("ExpirationMinutes")),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new {token = tokenString});
        }

    }
}
