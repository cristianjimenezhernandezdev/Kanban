using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class ProjecteService
    {
        public List<Projecte> GetAll()
        {
            var result = new List<Projecte>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdProjecte, Titol, DataInici, DataFi, IdResponsable FROM Projecte";

                using (var command = new SQLiteCommand(query, ctx))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapProjecte(reader));
                    }
                }
            }

            return result;
        }

        public Projecte GetById(int idProjecte)
        {
            Projecte projecte = null;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdProjecte, Titol, DataInici, DataFi, IdResponsable FROM Projecte WHERE IdProjecte = @IdProjecte";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", idProjecte));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            projecte = MapProjecte(reader);
                        }
                    }
                }
            }

            return projecte;
        }

        public Projecte Add(Projecte projecte)
        {
            using (var ctx = DbContext.GetInstance())
            {
                const string query = "INSERT INTO Projecte (Titol, DataInici, DataFi, IdResponsable) VALUES (@Titol, @DataInici, @DataFi, @IdResponsable)";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Titol", projecte.Titol));
                    command.Parameters.Add(new SQLiteParameter("@DataInici", projecte.DataInici));
                    command.Parameters.Add(new SQLiteParameter("@DataFi", projecte.DataFi.HasValue ? (object)projecte.DataFi.Value : DBNull.Value));
                    command.Parameters.Add(new SQLiteParameter("@IdResponsable", projecte.IdResponsable));

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    projecte.IdProjecte = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return projecte;
        }

        public int Update(Projecte projecte)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "UPDATE Projecte SET Titol = @Titol, DataInici = @DataInici, DataFi = @DataFi, IdResponsable = @IdResponsable WHERE IdProjecte = @IdProjecte";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Titol", projecte.Titol));
                    command.Parameters.Add(new SQLiteParameter("@DataInici", projecte.DataInici));
                    command.Parameters.Add(new SQLiteParameter("@DataFi", projecte.DataFi.HasValue ? (object)projecte.DataFi.Value : DBNull.Value));
                    command.Parameters.Add(new SQLiteParameter("@IdResponsable", projecte.IdResponsable));
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", projecte.IdProjecte));

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        public int Delete(int idProjecte)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "DELETE FROM Projecte WHERE IdProjecte = @IdProjecte";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", idProjecte));
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
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
