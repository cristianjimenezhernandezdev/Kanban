using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApplicationAPIDemo.DAL.Service;
using WebApplicationAPIDemo.Model;

namespace WebApplicationAPIDemo.Controllers
{
    [EnableCors]
    [Route("api/grups")] // endpoint base per a grups
    [ApiController]
    public class GrupsController : ControllerBase
    {
        /// <summary>
        /// Retorna els usuaris d'un grup concret.
        /// </summary>
        /// <param name="idGrup">Identificador del grup.</param>
        [HttpGet("{idGrup}/usuaris")]
        public ActionResult<List<Usuari>> GetUsuarisPerGrup(int idGrup)
        {
            var grupService = new GrupService();
            var grup = grupService.GetById(idGrup);

            if (grup == null)
            {
                return NotFound($"No existeix cap grup amb IdGrup = {idGrup}.");
            }

            var grupUsuariService = new GrupUsuariService();
            var usuaris = grupUsuariService.GetUsuarisByGrup(idGrup);

            return usuaris;
        }
    }
}
