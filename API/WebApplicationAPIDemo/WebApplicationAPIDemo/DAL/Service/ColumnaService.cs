using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class ColumnaService
    {
        public List<Columna> GetAll()
        {
            var result = new List<Columna>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdColumna, IdProjecte, Nom, Ordre FROM Columna";

                using (var command = new SQLiteCommand(query, ctx))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapColumna(reader));
                    }
                }
            }

            return result;
        }

        public Columna GetById(int idColumna)
        {
            Columna columna = null;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdColumna, IdProjecte, Nom, Ordre FROM Columna WHERE IdColumna = @IdColumna";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdColumna", idColumna));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            columna = MapColumna(reader);
                        }
                    }
                }
            }

            return columna;
        }

        public Columna Add(Columna columna)
        {
            using (var ctx = DbContext.GetInstance())
            {
                const string query = "INSERT INTO Columna (IdProjecte, Nom, Ordre) VALUES (@IdProjecte, @Nom, @Ordre)";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", columna.IdProjecte));
                    command.Parameters.Add(new SQLiteParameter("@Nom", columna.Nom));
                    command.Parameters.Add(new SQLiteParameter("@Ordre", columna.Ordre));

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    columna.IdColumna = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return columna;
        }

        public int Update(Columna columna)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "UPDATE Columna SET IdProjecte = @IdProjecte, Nom = @Nom, Ordre = @Ordre WHERE IdColumna = @IdColumna";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", columna.IdProjecte));
                    command.Parameters.Add(new SQLiteParameter("@Nom", columna.Nom));
                    command.Parameters.Add(new SQLiteParameter("@Ordre", columna.Ordre));
                    command.Parameters.Add(new SQLiteParameter("@IdColumna", columna.IdColumna));

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        public int Delete(int idColumna)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "DELETE FROM Columna WHERE IdColumna = @IdColumna";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdColumna", idColumna));
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        public List<Columna> GetByProjecte(int idProjecte)
        {
            var result = new List<Columna>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdColumna, IdProjecte, Nom, Ordre FROM Columna WHERE IdProjecte = @IdProjecte ORDER BY Ordre";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", idProjecte));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(MapColumna(reader));
                        }
                    }
                }
            }

            return result;
        }

        private static Columna MapColumna(SQLiteDataReader reader)
        {
            return new Columna
            {
                IdColumna = Convert.ToInt32(reader["IdColumna"]),
                IdProjecte = Convert.ToInt32(reader["IdProjecte"]),
                Nom = reader["Nom"].ToString(),
                Ordre = Convert.ToInt32(reader["Ordre"])
            };
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ColumnaController : ControllerBase
    {
        private readonly ColumnaService _columnaService;

        public ColumnaController()
        {
            _columnaService = new ColumnaService();
        }

        [HttpGet]
        public ActionResult<List<Columna>> GetAll()
        {
            var columnes = _columnaService.GetAll();
            return columnes;
        }

        [HttpGet("{id}")]
        public ActionResult<Columna> GetById(int id)
        {
            var columna = _columnaService.GetById(id);

            if (columna == null)
            {
                return NotFound();
            }

            return columna;
        }

        [HttpPost]
        public ActionResult<Columna> Create([FromBody] Columna novaColumna)
        {
            var columna = _columnaService.Add(novaColumna);
            return CreatedAtAction(nameof(GetById), new { id = columna.IdColumna }, columna);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Columna columnaActualitzada)
        {
            if (id != columnaActualitzada.IdColumna)
            {
                return BadRequest();
            }

            var rowsAffected = _columnaService.Update(columnaActualitzada);

            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var rowsAffected = _columnaService.Delete(id);

            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("projectes/{idProjecte}/columnes")]
        public ActionResult<List<Columna>> GetColumnesPerProjecte(int idProjecte)
        {
            var columnes = _columnaService.GetByProjecte(idProjecte);
            return columnes;
        }
    }
}
