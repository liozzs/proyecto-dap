using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using DAP.API.DAL;
using DAP.API.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DAP.API.Controllers
{
    [Route("api/[controller]")]
    public class UsuariosController : Controller
    {
        private UsuarioRepository _usuarioRepository = new UsuarioRepository();
        private DispenserRepository _dispenserRepository = new DispenserRepository();
   
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(ILogger<UsuariosController> logger)
        {
            _logger = logger;
        }

        [HttpGet("all", Name = "GetAllUsuarios")]
        [AllowAnonymous]
        public IEnumerable<Usuario> GetAllUsuarios()
        {
            _logger.LogInformation("GetAllUsuarios");
            return _usuarioRepository.GetAll();
        }

        [HttpGet(Name = "GetUsuario")]
        [Authorize]
        public IActionResult GetUsuario()
        {
            _logger.LogInformation("GetUsuario");
            var usuario = GetCurrentUser(HttpContext.User);
            if (usuario == null)
                return NotFound();
            return Ok(usuario);
        }

        [HttpGet("dispensers", Name = "GetUsuarioDispensers")]
        [Authorize]
        public IActionResult GetDispensers()
        {
            _logger.LogInformation("GetUsuarioDispensers");
            Usuario usuario = GetCurrentUser(HttpContext.User);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario.Dispensers);
        }

        [HttpPost("dispensers", Name = "CreateDispenser")]
        [Authorize]
        public IActionResult Post([FromBody] Dispenser dispenser)
        {
            _logger.LogInformation("CreateDispenser");
            Usuario usuario = GetCurrentUser(HttpContext.User);
            Dispenser d = _dispenserRepository.Get(dispenser.DireccionMAC);

          
            if (d != null)
            {
                dispenser.ID = d.ID;
            }

            if (!_usuarioRepository.Insert(dispenser, usuario))
                return BadRequest();
            return Ok();
        }

        [HttpGet("dispensers/{MAC}", Name = "GetUsuarioDispenser")]
        [Authorize]
        public IActionResult GetUsuarioDispenser(string MAC)
        {
            _logger.LogInformation("GetUsuarioDispenser");
            Usuario usuario = GetCurrentUser(HttpContext.User);
            if (usuario == null)
            {
                return NotFound();
            }
            MAC = MAC.Replace("%3A", ":");
            Dispenser dispenser = usuario.Dispensers.FirstOrDefault(d => d.DireccionMAC.Equals(MAC));
            //Dispenser dispenser = _dispenserRepository.Get(MAC);
            if (dispenser == null)
            {
                return NotFound();
            }
            return Ok(dispenser);
        }

        [HttpGet("dispensers/{MAC}/mensajes", Name = "GetUsuarioDispenserMensajes")]
        [Authorize]
        public IActionResult GetUsuarioDispenserMensajes(string MAC)
        {
            _logger.LogInformation("GetUsuarioDispenserMensajes");
            Usuario usuario = GetCurrentUser(HttpContext.User);
            if (usuario == null)
            {
                return NotFound();
            }

            MAC = MAC.Replace("%3A", ":");
            Dispenser dispenser = usuario.Dispensers.FirstOrDefault(d => d.DireccionMAC.Equals(MAC));
            //Dispenser dispenser = _dispenserRepository.Get(MAC);
            if (dispenser == null)
            {
                return NotFound();
            }
            return Ok(dispenser.Mensajes);
        }

        private Usuario GetCurrentUser(ClaimsPrincipal principal)
        {
            string email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            return _usuarioRepository.Get(email);
        }
    }
}
