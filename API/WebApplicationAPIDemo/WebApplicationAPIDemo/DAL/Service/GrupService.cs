using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebApplicationAPIDemo.Model;
using WebApplicationAPIDemo.Persistence;

namespace WebApplicationAPIDemo.DAL.Service
{
    public class GrupService
    {
        /// <summary>
        /// Obté tots els grups.
        /// </summary>
        public List<Grup> GetAll()
        {
            var result = new List<Grup>();

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdGrup, Nom, Codi FROM Grup";

                using (var command = new SQLiteCommand(query, ctx))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Grup
                        {
                            IdGrup = Convert.ToInt32(reader["IdGrup"]),
                            Nom = reader["Nom"].ToString(),
                            Codi = reader["Codi"].ToString()
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Obté les dades del grup indicat.
        /// </summary>
        public Grup GetById(int idGrup)
        {
            Grup grup = null;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdGrup, Nom, Codi FROM Grup WHERE IdGrup = @IdGrup";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdGrup", idGrup));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            grup = new Grup
                            {
                                IdGrup = Convert.ToInt32(reader["IdGrup"]),
                                Nom = reader["Nom"].ToString(),
                                Codi = reader["Codi"].ToString()
                            };
                        }
                    }
                }
            }

            return grup;
        }

        /// <summary>
        /// Obté un grup pel seu nom i codi.
        /// </summary>
        public Grup GetByNomICodi(string nom, string codi)
        {
            Grup grup = null;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "SELECT IdGrup, Nom, Codi FROM Grup WHERE Nom = @Nom AND Codi = @Codi";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Nom", nom));
                    command.Parameters.Add(new SQLiteParameter("@Codi", codi));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            grup = new Grup
                            {
                                IdGrup = Convert.ToInt32(reader["IdGrup"]),
                                Nom = reader["Nom"].ToString(),
                                Codi = reader["Codi"].ToString()
                            };
                        }
                    }
                }
            }

            return grup;
        }

        /// <summary>
        /// Afegeix un nou grup a la base de dades.
        /// </summary>
        public Grup Add(Grup grup)
        {
            using (var ctx = DbContext.GetInstance())
            {
                const string query = "INSERT INTO Grup (Nom, Codi) VALUES (@Nom, @Codi)";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Nom", grup.Nom));
                    command.Parameters.Add(new SQLiteParameter("@Codi", grup.Codi));

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT last_insert_rowid()";
                    grup.IdGrup = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return grup;
        }

        /// <summary>
        /// Actualitza un grup.
        /// </summary>
        public int Update(Grup grup)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "UPDATE Grup SET Nom = @Nom, Codi = @Codi WHERE IdGrup = @IdGrup";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@Nom", grup.Nom));
                    command.Parameters.Add(new SQLiteParameter("@Codi", grup.Codi));
                    command.Parameters.Add(new SQLiteParameter("@IdGrup", grup.IdGrup));

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// Elimina un grup.
        /// </summary>
        public int Delete(int idGrup)
        {
            var rowsAffected = 0;

            using (var ctx = DbContext.GetInstance())
            {
                const string query = "DELETE FROM Grup WHERE IdGrup = @IdGrup";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("@IdGrup", idGrup));
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }
    }
}
