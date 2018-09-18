using System.Collections.Generic;
using DAP.API.DAL;
using DAP.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DAP.API.Services;
using Microsoft.Extensions.Logging;

namespace DAP.API.Controllers
{
    [Route("api/[controller]")]
    public class DispensersController: Controller
    {
        private readonly DispenserRepository _dispenserRepository = new DispenserRepository();
        private readonly IEmailSender _emailSender;
        private readonly ILogger<DispensersController> _logger;

        public DispensersController(IEmailSender emailSender, ILogger<DispensersController> logger) {
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet("all", Name = "GetAllDispensers")]
        [AllowAnonymous]
        public IEnumerable<Dispenser> GetAllDispensers() {
            _logger.LogInformation("GetAllDispensers");
            return _dispenserRepository.GetAll();
        }

        [HttpPost("message", Name = "SendMessage")]
        [AllowAnonymous]
        public IActionResult SendMessage(DispenserMensaje mensaje) {
            _logger.LogInformation("SendMessage");
            Dispenser dispenser = _dispenserRepository.Get(mensaje.DireccionMAC);
            if (dispenser == null)
                return NotFound();
            string message = "Estimado usuario:" + NewLine();
            var parameters = new Dictionary<string, string>
            {
                { "Receptáculo", mensaje.Receptaculo.ToString() },
                { "Pastilla", mensaje.Pastilla }
            };
            switch (mensaje.Codigo) {
                case CodigoError.E001:
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Cantidad de pastillas en el receptáculo menor a la cantidad de pastillas a dispensar.");
                    break;
                case CodigoError.E002:
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Se superó el umbral de tiempo en que el mecanismo debe realizar el expendio.");
                    break;
                case CodigoError.E003:
                    message += "Le informamos se ha alcanzado el stock crítico del siguiente medicamento:" + NewLine();
                    parameters.Add("Cantidad Restante", mensaje.CantidadRestante.ToString());
                    break;
                case CodigoError.E004:
                    message += "Le informamos que el botón para iniciar el expendio de medicamentos no ha sido presionado. Expendio correspondiente:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case CodigoError.E005:
                    message += "Le informamos que el recipiente no ha sido devuelto a su posición. Último expendio realizado:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    break;
            }

            foreach (KeyValuePair<string, string> param in parameters) {
                message += "<strong>" + param.Key + "</strong>: ";
                message += param.Value + "<br>";
            }

            message += "<br><em>Atte. Equipo D.A.P.</em>";

            dispenser.Usuarios.ForEach(u =>
            {
                _emailSender.SendEmail(u.Email, "ERROR", message);
            });

            return Ok();
        }

        private string NewLine() {
            return "<br><br>";
        }
    }
}
