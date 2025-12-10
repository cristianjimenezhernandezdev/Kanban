using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationAPIDemo.DAL.Service;
using WebApplicationAPIDemo.Model;


namespace WebApplicationAPIDemo.Controllers
{
    [EnableCors]
    [Route("api/users")]//el endpoint
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET: users
        [HttpGet]
        public List<Grup> Get()
        {
            var grupService = new GrupService();
            return grupService.GetAll();
        }

        // GET users/5
        //La ultima part de la ruta, en aquest cas un 5 serà el que fem serfir, el id
        [HttpGet("{id}")]
        public Grup Get(int id)
        {
            var grupService = new GrupService();
            return grupService.GetById(id);
        }

        // POST users
        [HttpPost]
        public Grup Post([FromBody] Grup user)
        {
            var grupService = new GrupService();
            return grupService.Add(user);
        }

        // PUT users/5
        [HttpPut("{id}")]
        public int Put(int id, [FromBody] Grup user)
        {
            var grupService = new GrupService();
            user.IdGrup = id;
            return grupService.Update(user);
        }

        // DELETE users/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var grupService = new GrupService();
            grupService.Delete(id);
        }
    }
}
