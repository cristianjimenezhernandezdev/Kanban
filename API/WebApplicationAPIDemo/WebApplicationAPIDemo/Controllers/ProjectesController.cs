using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApplicationAPIDemo.DAL.Service;
using WebApplicationAPIDemo.Model;

namespace WebApplicationAPIDemo.Controllers
{
    [EnableCors]
    [Route("api")]
    [ApiController]
    public class ProjectesController : ControllerBase
    {
        /// <summary>
        /// GET /usuaris/{idUsuari}/projectes
        /// Retorna projectes on l'usuari és responsable o participant.
        /// </summary>
        [HttpGet("usuaris/{idUsuari}/projectes")]
        public ActionResult<List<Projecte>> GetProjectesPerUsuari(int idUsuari)
        {
            var service = new ProjecteLlistatService();
            var projectes = service.GetProjectesPerUsuari(idUsuari);
            return projectes;
        }

        /// <summary>
        /// GET /grups/{idGrup}/projectes (opcional)
        /// Retorna projectes associats a un grup.
        /// </summary>
        [HttpGet("grups/{idGrup}/projectes")]
        public ActionResult<List<Projecte>> GetProjectesPerGrup(int idGrup)
        {
            var service = new ProjecteLlistatService();
            var projectes = service.GetProjectesPerGrup(idGrup);
            return projectes;
        }
    }
}
