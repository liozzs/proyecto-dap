using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using DAP.API.DAL;
using DAP.API.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DAP.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class LoginController: Controller
    {
        UsuarioRepository _usuarioRepository = new UsuarioRepository();
        private readonly IConfiguration _config;

        public LoginController(IConfiguration configuration) 
        {
            _config = configuration;
        }

        [HttpPost]
        public IActionResult Login(LoginRequest loginRequest)
        {
            if (loginRequest == null)
                return BadRequest();
            Usuario usuario = Authenticate(loginRequest);
            if (usuario != null)
            {
                var token = GenerateTokenJwt(usuario);
                return Ok(new { token });
            }
            return Unauthorized();
        }

        [HttpPost("create")]
        public IActionResult Post(Usuario usuario)
        {
            CreatePassword(usuario);
            var inserted = _usuarioRepository.Insert(usuario);
            if (inserted == null)
            {
                return BadRequest();
            }
            var token = GenerateTokenJwt(inserted);
            return Ok(new {token});
        }

        private void CreatePassword(Usuario usuario) {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            usuario.Password = Hash(usuario.Password, salt);
            usuario.Salt = Convert.ToBase64String(salt);

        }

        private string Hash(string password, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];

            Array.Copy(hash, 0, hashBytes, 0, 20);
            Array.Copy(salt, 0, hashBytes, 20, 16);

            return Convert.ToBase64String(hashBytes);
        }

        private Usuario Authenticate(LoginRequest loginRequest) {
            Usuario saved = _usuarioRepository.Get(loginRequest.Email);
            if (saved != null)
            {
                string hashed = Hash(loginRequest.Password,Convert.FromBase64String(saved.Salt));
                if (hashed.Equals(saved.Password))
                    return saved;
            }
            return null;
        }

        private string GenerateTokenJwt(Usuario usuario)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Jti, usuario.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Nombre)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                                             _config["Jwt:Issuer"],
                                             claims,
                                             signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
