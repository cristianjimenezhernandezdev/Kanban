using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class UsuariService
    {
        /// <summary>
        /// Obté tots els usuaris
        /// </summary>
        public List<Usuari> GetAll()
        {
            var result = new List<Usuari>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdUsuari, Nom, IdGrup FROM Usuari";

                using (var command = new SQLiteCommand(query, ctx))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Usuari
                        {
                            IdUsuari = Convert.ToInt64(reader["IdUsuari"]),
                            Nom = reader["Nom"].ToString(),
                            IdGrup = Convert.ToInt32(reader["IdGrup"])
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Obté les dades de l'usuari indicat
        /// </summary>
        /// <param name="idUsuari">Identificador d'usuari</param>
        public Usuari GetById(long idUsuari)
        {
            Usuari user = null;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdUsuari, Nom, IdGrup FROM Usuari WHERE IdUsuari = @IdUsuari";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", idUsuari));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new Usuari
                            {
                                IdUsuari = Convert.ToInt64(reader["IdUsuari"]),
                                Nom = reader["Nom"].ToString(),
                                IdGrup = Convert.ToInt32(reader["IdGrup"])
                            };
                        }
                    }
                }
            }

            return user;
        }

        /// <summary>
        /// Afegeix un nou usuari a la base de dades
        /// </summary>
        /// <param name="user">Entitat usuari</param>
        /// <returns>Usuari amb l'identificador assignat</returns>
        public Usuari Add(Usuari user)
        {
            using (var ctx = DbContext.GetInstance())
            {
                const string query = "INSERT INTO Usuari (Nom, IdGrup) VALUES (@Nom, @IdGrup)";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Nom", user.Nom));
                    command.Parameters.Add(new SQLiteParameter("@IdGrup", user.IdGrup));

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    user.IdUsuari = Convert.ToInt64(command.ExecuteScalar());
                }
            }

            return user;
        }

        /// <summary>
        /// Actualitza un usuari
        /// </summary>
        /// <param name="user">Entitat usuari que es vol modificar</param>
        /// <returns>Files afectades</returns>
        public int Update(Usuari user)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "UPDATE Usuari SET Nom = @Nom, IdGrup = @IdGrup WHERE IdUsuari = @IdUsuari";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Nom", user.Nom));
                    command.Parameters.Add(new SQLiteParameter("@IdGrup", user.IdGrup));
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", user.IdUsuari));

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// Elimina un usuari
        /// </summary>
        /// <param name="idUsuari">Codi d'usuari que es vol eliminar</param>
        /// <returns>Files afectades</returns>
        public int Delete(long idUsuari)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "DELETE FROM Usuari WHERE IdUsuari = @IdUsuari";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", idUsuari));
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }
    }
}
