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

        [HttpPost("mensajes", Name = "SendMessage")]
        [AllowAnonymous]
        public IActionResult SendMessage([FromBody] DispenserMensaje mensaje) {
            _logger.LogInformation("SendMessage");
            
            Dispenser dispenser = _dispenserRepository.Get(mensaje.DireccionMAC);
            if (dispenser == null)
                return NotFound();
            string subject = "Notificación - Dispenser: " + dispenser.Nombre;
            string message = "Estimado usuario:" + NewLine();
            var parameters = new Dictionary<string, string>
            {
                { "Receptáculo", mensaje.Receptaculo.ToString() },
                { "Pastilla", mensaje.Pastilla }
            };
            switch (mensaje.Codigo) {
                case CodigoError.FALTA_DE_PASTILLAS:
                    mensaje.Mensaje = "Expendio no realizado. Cantidad de pastillas en el receptáculo menor a la cantidad de pastillas a dispensar.";
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Cantidad de pastillas en el receptáculo menor a la cantidad de pastillas a dispensar.");
                    break;
                case CodigoError.LIMITE_DE_TIEMPO:
                    mensaje.Mensaje = "Expendio no realizado. Se superó el umbral de tiempo en que el mecanismo debe realizar el expendio.";
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Se superó el umbral de tiempo en que el mecanismo debe realizar el expendio.");
                    break;
                case CodigoError.STOCK_CRITICO:
                    mensaje.Mensaje = "Expendio realizado. Se ha alcanzado el stock crítico.";
                    message += "Le informamos se ha alcanzado el stock crítico del siguiente medicamento:" + NewLine();
                    parameters.Add("Cantidad Restante", mensaje.CantidadRestante.ToString());
                    break;
                case CodigoError.BOTON_NO_PRESIONADO:
                    mensaje.Mensaje = "Expendio no realizado. El botón para iniciar el expendio de medicamentos no ha sido presionado.";
                    message += "Le informamos que el botón para iniciar el expendio de medicamentos no ha sido presionado. Expendio correspondiente:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case CodigoError.VASO_NO_RETIRADO:
                    mensaje.Mensaje = "Expendio realizado. El recipiente no ha sido retirado de su posición.";
                    message += "Le informamos que el recipiente no ha sido retirado de su posición. Último expendio realizado:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case CodigoError.VASO_NO_DEVUELTO:
                    mensaje.Mensaje = "Expendio realizado. El recipiente no ha sido devuelto a su posición.";
                    message += "Le informamos que el recipiente no ha sido devuelto a su posición. Último expendio realizado:" + NewLine();
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case CodigoError.BLOQUEO_RECIPIENTE:
                    mensaje.Mensaje = "Le informamos se ha realizado el bloqueo del receptáculo.";
                    message += "Le informamos se ha realizado el bloqueo del siguiente receptáculo. Expendio correspondiente:" + NewLine();
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
                _emailSender.SendEmail(u.Email, subject, message);
            });

            // Guardar el DispenserMensaje
            _dispenserRepository.Insert(mensaje, dispenser);

            return Ok();
        }

        [HttpPost ("planificacion", Name = "SendPlanificacion")]
        [AllowAnonymous]
        public IActionResult SendPlanificacion([FromBody] PlanificacionMensaje planificacion)
        {
            _logger.LogInformation("SendPlanificacion");

            Dispenser dispenser = _dispenserRepository.Get(planificacion.DireccionMAC);
            if (dispenser == null)
                return NotFound();

            string subject = "Notificación - Dispenser: " + dispenser.Nombre;
            string message = "Estimado usuario:" + NewLine();
            string periodicidad;
            if (planificacion.Periodicidad.Equals("0"))
                periodicidad = "Diaria";
            else if (planificacion.Periodicidad.Equals("1"))
                periodicidad = "Semanal";
            else
                periodicidad = "Personalizada";

            message += "Le informamos se ha cargado la siguiente planificación:" + NewLine();

            message += "<strong>Receptáculo</strong>: ";
            message += planificacion.Receptaculo.ToString() + "<br>";
            message += "<strong>Horario Inicio</strong>: ";
            message += planificacion.HorarioInicio + "<br>";
            message += "<strong>Intervalo</strong>: ";
            message += int.Parse(planificacion.Intervalo) / 60 + " Minutos<br>";
            message += "<strong>Cantidad</strong>: ";
            message += planificacion.Cantidad.ToString() + "<br>";
            message += "<strong>Stock Crítico</strong>: ";
            message += planificacion.StockCritico.ToString() + "<br>";
            message += "<strong>Periodicidad</strong>: ";
            message += periodicidad + "<br>";
            message += "<strong>Días</strong>: ";
            message += ParseDias(planificacion.Dias) + "<br>";
            message += "<strong>Bloquear</strong>: ";
            message += (planificacion.Bloqueo.Equals("0") ? "No" : "Sí") + NewLine();

            message += "<em>Atte. Equipo D.A.P.</em>";

            dispenser.Usuarios.ForEach(u =>
            {
                _emailSender.SendEmail(u.Email, subject, message);
            });

            return Ok();
        }

        [HttpPost("carga", Name = "SendCargaStock")]
        [AllowAnonymous]
        public IActionResult SendCargaStock([FromBody] CargaStockMensaje cargaStock)
        {
            _logger.LogInformation("SendCargaStock");

            Dispenser dispenser = _dispenserRepository.Get(cargaStock.DireccionMAC);
            if (dispenser == null)
                return NotFound();

            string subject = "Notificación - Dispenser: " + dispenser.Nombre;
            string message = "Estimado usuario:" + NewLine();

            message += "Le informamos se ha realizado la siguiente carga de stock:" + NewLine();

            message += "<strong>Receptáculo</strong>: ";
            message += cargaStock.Receptaculo.ToString() + "<br>";
            message += "<strong>Pastilla</strong>: ";
            message += cargaStock.Pastilla + "<br>";
            message += "<strong>Stock</strong>: ";
            message += cargaStock.Stock.ToString() + NewLine();

            message += "<em>Atte. Equipo D.A.P.</em>";

            dispenser.Usuarios.ForEach(u =>
            {
                _emailSender.SendEmail(u.Email, subject, message);
            });

            return Ok();
        }

        private string NewLine() {
            return "<br><br>";
        }

        private string ParseDias(string dias)
        {
            string s = "";

            if (dias[0].Equals('1'))
                s += "L-";
            if (dias[1].Equals('1'))
                s += "Ma-";
            if (dias[2].Equals('1'))
                s += "Mi-";
            if (dias[3].Equals('1'))
                s += "J-";
            if (dias[4].Equals('1'))
                s += "V-";
            if (dias[5].Equals('1'))
                s += "S-";
            if (dias[6].Equals('1'))
                s += "D-";

            return s.TrimEnd('-');
        }
    }
}
