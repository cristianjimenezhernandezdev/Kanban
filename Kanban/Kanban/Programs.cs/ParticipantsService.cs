using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{
    public class ParticipantsService
    {
        public List<string> CarregarParticipants(int grupActiu)
        {
            var participants = new List<string>();

            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                const string query = "SELECT Nom FROM Usuaris WHERE IdGrup = @grup";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", grupActiu);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            participants.Add(reader["Nom"].ToString());
                        }
                    }
                }
            }

            return participants;
        }

        public void AfegirParticipantAProjecte(string nom, int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                int? idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);
                int? idProjecte = ObtenirProjecteActiuId(conn, grupActiu);

                if (!idUsuari.HasValue || !idProjecte.HasValue) return;

                const string sql = "INSERT IGNORE INTO Usuaris_projectes (IdProjecte, IdUsuari) VALUES (@idProjecte, @idUsuari)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idProjecte", idProjecte.Value);
                    cmd.Parameters.AddWithValue("@idUsuari", idUsuari.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DesvincularParticipant(string nom, int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);
                int? idProjecte = ObtenirProjecteActiuId(conn, grupActiu);

                if (idUsuari.HasValue && idProjecte.HasValue)
                {
                    const string sql = "DELETE FROM Usuaris_projectes WHERE IdProjecte = @idProjecte AND IdUsuari = @idUsuari";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@idProjecte", idProjecte.Value);
                        cmd.Parameters.AddWithValue("@idUsuari", idUsuari.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void EliminarUsuari(string nom, int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                int idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);
               

                ExecutarComanda(conn, "DELETE FROM Usuaris_projectes WHERE IdUsuari = @id", idUsuari);
                ExecutarComanda(conn, "UPDATE Tasca SET IdUsuariResponsable = NULL WHERE IdUsuariResponsable = @id", idUsuari);
                ExecutarComanda(conn, "DELETE FROM Usuaris WHERE IdUsuari = @id", idUsuari);
            }
        }

        private static int ObtenirIdUsuariPerNom(MySqlConnection conn, string nomUsuari, int grupActiu)
        {
            const string sql = "SELECT IdUsuari FROM Usuaris WHERE Nom = @nom AND IdGrup = @grup";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@nom", nomUsuari);
                cmd.Parameters.AddWithValue("@grup", grupActiu);
                int result= cmd.ExecuteNonQuery();
                //var result = cmd.ExecuteScalar();
                return result;
            }
        }

        private static int ObtenirProjecteActiuId(MySqlConnection conn, int grupActiu)
        {
            const string sql = @"SELECT IdProjecte FROM Projectes 
                                 WHERE IdGrup = @grup ORDER BY IdProjecte DESC LIMIT 1";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@grup", grupActiu);
                int result = cmd.ExecuteNonQuery();
                
                return result;
            }
        }

        private static void ExecutarComanda(MySqlConnection conn, string sql, int idUsuari)
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", idUsuari);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
