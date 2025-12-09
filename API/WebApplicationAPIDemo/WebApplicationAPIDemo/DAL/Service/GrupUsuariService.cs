using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class GrupUsuariService
    {
        /// <summary>
        /// Retorna tots els usuaris d'un grup determinat.
        /// </summary>
        public List<Usuari> GetUsuarisByGrup(int idGrup)
        {
            var result = new List<Usuari>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdUsuari, Nom, IdGrup FROM Usuari WHERE IdGrup = @IdGrup";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdGrup", idGrup));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Usuari
                            {
                                IdUsuari = System.Convert.ToInt64(reader["IdUsuari"]),
                                Nom = reader["Nom"].ToString(),
                                IdGrup = System.Convert.ToInt32(reader["IdGrup"])
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
