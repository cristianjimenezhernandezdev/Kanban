using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class TascaService
    {
        public List<Tasca> GetAll()
        {
            var result = new List<Tasca>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdTasca, IdProjecte, IdColumna, IdUsuariResponsable, Descripcio, Prioritat, DataCreacio, DataVenciment FROM Tasca";

                using (var command = new SQLiteCommand(query, ctx))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapTasca(reader));
                    }
                }
            }

            return result;
        }

        public Tasca GetById(int idTasca)
        {
            Tasca tasca = null;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdTasca, IdProjecte, IdColumna, IdUsuariResponsable, Descripcio, Prioritat, DataCreacio, DataVenciment FROM Tasca WHERE IdTasca = @IdTasca";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdTasca", idTasca));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tasca = MapTasca(reader);
                        }
                    }
                }
            }

            return tasca;
        }

        public Tasca Add(Tasca tasca)
        {
            using (var ctx = DbContext.GetInstance())
            {
                const string query = "INSERT INTO Tasca (IdProjecte, IdColumna, IdUsuariResponsable, Descripcio, Prioritat, DataCreacio, DataVenciment) VALUES (@IdProjecte, @IdColumna, @IdUsuariResponsable, @Descripcio, @Prioritat, @DataCreacio, @DataVenciment)";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", tasca.IdProjecte));
                    command.Parameters.Add(new SQLiteParameter("@IdColumna", tasca.IdColumna));
                    command.Parameters.Add(new SQLiteParameter("@IdUsuariResponsable", tasca.IdUsuariResponsable.HasValue ? (object)tasca.IdUsuariResponsable.Value : DBNull.Value));
                    command.Parameters.Add(new SQLiteParameter("@Descripcio", tasca.Descripcio));
                    command.Parameters.Add(new SQLiteParameter("@Prioritat", tasca.Prioritat.HasValue ? (object)tasca.Prioritat.Value : DBNull.Value));
                    command.Parameters.Add(new SQLiteParameter("@DataCreacio", tasca.DataCreacio));
                    command.Parameters.Add(new SQLiteParameter("@DataVenciment", tasca.DataVenciment.HasValue ? (object)tasca.DataVenciment.Value : DBNull.Value));

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    tasca.IdTasca = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return tasca;
        }

        public int Update(Tasca tasca)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "UPDATE Tasca SET IdProjecte = @IdProjecte, IdColumna = @IdColumna, IdUsuariResponsable = @IdUsuariResponsable, Descripcio = @Descripcio, Prioritat = @Prioritat, DataCreacio = @DataCreacio, DataVenciment = @DataVenciment WHERE IdTasca = @IdTasca";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdProjecte", tasca.IdProjecte));
                    command.Parameters.Add(new SQLiteParameter("@IdColumna", tasca.IdColumna));
                    command.Parameters.Add(new SQLiteParameter("@IdUsuariResponsable", tasca.IdUsuariResponsable.HasValue ? (object)tasca.IdUsuariResponsable.Value : DBNull.Value));
                    command.Parameters.Add(new SQLiteParameter("@Descripcio", tasca.Descripcio));
                    command.Parameters.Add(new SQLiteParameter("@Prioritat", tasca.Prioritat.HasValue ? (object)tasca.Prioritat.Value : DBNull.Value));
                    command.Parameters.Add(new SQLiteParameter("@DataCreacio", tasca.DataCreacio));
                    command.Parameters.Add(new SQLiteParameter("@DataVenciment", tasca.DataVenciment.HasValue ? (object)tasca.DataVenciment.Value : DBNull.Value));
                    command.Parameters.Add(new SQLiteParameter("@IdTasca", tasca.IdTasca));

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        public int Delete(int idTasca)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "DELETE FROM Tasca WHERE IdTasca = @IdTasca";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdTasca", idTasca));
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        private static Tasca MapTasca(SQLiteDataReader reader)
        {
            return new Tasca
            {
                IdTasca = Convert.ToInt32(reader["IdTasca"]),
                IdProjecte = Convert.ToInt32(reader["IdProjecte"]),
                IdColumna = Convert.ToInt32(reader["IdColumna"]),
                IdUsuariResponsable = reader["IdUsuariResponsable"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IdUsuariResponsable"]),
                Descripcio = reader["Descripcio"].ToString(),
                Prioritat = reader["Prioritat"] == DBNull.Value ? (byte?)null : Convert.ToByte(reader["Prioritat"]),
                DataCreacio = Convert.ToDateTime(reader["DataCreacio"]),
                DataVenciment = reader["DataVenciment"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DataVenciment"])
            };
        }
    }
}
