using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{
    public class ParticipantsService
    {
        // Carrega tots els usuaris del grup (per al desplegable)
        public List<string> CarregarParticipants(int grupActiu)
        {
            var participants = new List<string>();

            using (var conn = DataBase.ObtenirConnexio())
            {
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

        // Carrega els participants vinculats a un projecte específic
        public List<string> CarregarParticipantsProjecte(int idProjecte)
        {
            var participants = new List<string>();

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string query = @"SELECT u.Nom 
                                       FROM Usuaris u
                                       INNER JOIN Usuaris_projectes up ON u.IdUsuari = up.IdUsuari
                                       WHERE up.IdProjecte = @idProjecte";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idProjecte", idProjecte);

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

        public void AfegirParticipantAProjecte(string nom, int grupActiu, int? idProjecte)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);
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

        public void DesvincularParticipant(string nom, int grupActiu, int? idProjecte)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);
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

        // Elimina un usuari i totes les seves relacions a la base de dades
        public void EliminarUsuari(string nom, int grupActiu)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);
                if (!idUsuari.HasValue) return;

                // 1. Posar a NULL les tasques on l'usuari és responsable
                ExecutarComanda(conn, 
                    "UPDATE Tasca SET IdUsuariResponsable = NULL WHERE IdUsuariResponsable = @id", 
                    idUsuari.Value);

                // 2. Posar a NULL els projectes on l'usuari és responsable (IdResponsable)
                ExecutarComanda(conn, 
                    "UPDATE Projectes SET IdResponsable = NULL WHERE IdResponsable = @id", 
                    idUsuari.Value);

                // 3. Eliminar de la taula Usuaris_projectes (participant de projectes)
                ExecutarComanda(conn, 
                    "DELETE FROM Usuaris_projectes WHERE IdUsuari = @id", 
                    idUsuari.Value);

                // 4. Finalment, eliminar l'usuari
                ExecutarComanda(conn, 
                    "DELETE FROM Usuaris WHERE IdUsuari = @id", 
                    idUsuari.Value);
            }
        }

        private static int? ObtenirIdUsuariPerNom(MySqlConnection conn, string nomUsuari, int grupActiu)
        {
            const string sql = "SELECT IdUsuari FROM Usuaris WHERE Nom = @nom AND IdGrup = @grup";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@nom", nomUsuari);
                cmd.Parameters.AddWithValue("@grup", grupActiu);
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        private static int? ObtenirProjecteActiuId(MySqlConnection conn, int grupActiu)
        {
            const string sql = @"SELECT IdProjecte FROM Projectes 
                                 WHERE IdGrup = @grup ORDER BY IdProjecte DESC LIMIT 1";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@grup", grupActiu);
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
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
