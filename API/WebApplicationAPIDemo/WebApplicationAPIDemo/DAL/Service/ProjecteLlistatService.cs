using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    /// <summary>
    /// Lògica per llistar projectes per usuari o per grup.
    /// </summary>
    public class ProjecteLlistatService
    {
        /// <summary>
        /// Retorna els projectes on l'usuari és responsable o participant.
        /// </summary>
        public List<Projecte> GetProjectesPerUsuari(int idUsuari)
        {
            var result = new List<Projecte>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query =
                    "SELECT DISTINCT p.IdProjecte, p.Titol, p.DataInici, p.DataFi, p.IdResponsable " +
                    "FROM Projecte p " +
                    "LEFT JOIN Projecte_Usuari pu ON pu.IdProjecte = p.IdProjecte " +
                    "WHERE p.IdResponsable = @IdUsuari OR pu.IdUsuari = @IdUsuari";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", idUsuari));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(MapProjecte(reader));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retorna tots els projectes d'un grup (projectes on algun usuari del grup participa o és responsable).
        /// </summary>
        public List<Projecte> GetProjectesPerGrup(int idGrup)
        {
            var result = new List<Projecte>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query =
                    "SELECT DISTINCT p.IdProjecte, p.Titol, p.DataInici, p.DataFi, p.IdResponsable " +
                    "FROM Projecte p " +
                    "INNER JOIN Usuari u ON u.IdUsuari = p.IdResponsable OR u.IdUsuari IN (" +
                    "    SELECT pu.IdUsuari FROM Projecte_Usuari pu WHERE pu.IdProjecte = p.IdProjecte" +
                    ") " +
                    "WHERE u.IdGrup = @IdGrup";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdGrup", idGrup));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(MapProjecte(reader));
                        }
                    }
                }
            }

            return result;
        }

        private static Projecte MapProjecte(SQLiteDataReader reader)
        {
            return new Projecte
            {
                IdProjecte = Convert.ToInt32(reader["IdProjecte"]),
                Titol = reader["Titol"].ToString(),
                DataInici = Convert.ToDateTime(reader["DataInici"]),
                DataFi = reader["DataFi"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DataFi"]),
                IdResponsable = Convert.ToInt32(reader["IdResponsable"])
            };
        }
    }
}
