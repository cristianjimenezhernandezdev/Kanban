using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class ProjecteUsuariService
    {
        public List<ProjecteUsuari> GetAll()
        {
            var result = new List<ProjecteUsuari>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdProjecte, IdUsuari, Rol FROM Projecte_Usuari";

                using (var command = new SQLiteCommand(query, ctx))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapProjecteUsuari(reader));
                    }
                }
            }

            return result;
        }

        public ProjecteUsuari GetByIds(int idProjecte, int idUsuari)
        {
            ProjecteUsuari entry = null;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdProjecte, IdUsuari, Rol FROM Projecte_Usuari WHERE IdProjecte = @IdProjecte AND IdUsuari = @IdUsuari";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", idProjecte));
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", idUsuari));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entry = MapProjecteUsuari(reader);
                        }
                    }
                }
            }

            return entry;
        }

        public ProjecteUsuari Add(ProjecteUsuari entry)
        {
            using (var ctx = DbContext.GetInstance())
            {
                const string query = "INSERT INTO Projecte_Usuari (IdProjecte, IdUsuari, Rol) VALUES (@IdProjecte, @IdUsuari, @Rol)";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", entry.IdProjecte));
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", entry.IdUsuari));
                    command.Parameters.Add(new SQLiteParameter("@Rol", entry.Rol));

                    command.ExecuteNonQuery();
                }
            }

            return entry;
        }

        public int Update(ProjecteUsuari entry)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "UPDATE Projecte_Usuari SET Rol = @Rol WHERE IdProjecte = @IdProjecte AND IdUsuari = @IdUsuari";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Rol", entry.Rol));
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", entry.IdProjecte));
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", entry.IdUsuari));

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        public int Delete(int idProjecte, int idUsuari)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "DELETE FROM Projecte_Usuari WHERE IdProjecte = @IdProjecte AND IdUsuari = @IdUsuari";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", idProjecte));
                    command.Parameters.Add(new SQLiteParameter("@IdUsuari", idUsuari));
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        private static ProjecteUsuari MapProjecteUsuari(SQLiteDataReader reader)
        {
            return new ProjecteUsuari
            {
                IdProjecte = Convert.ToInt32(reader["IdProjecte"]),
                IdUsuari = Convert.ToInt32(reader["IdUsuari"]),
                Rol = reader["Rol"].ToString()
            };
        }
    }
}
