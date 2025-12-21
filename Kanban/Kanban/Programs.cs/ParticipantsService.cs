using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{

    public class ParticipantsService
    {
        // Carrega tots els noms d'usuaris del grup.
        // Es fa servir per omplir els spinners (participants i sprint master) del MainWindow.
        public List<string> CarregarParticipants(int grupActiu)
        {
            var participants = new List<string>();

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string query = "SELECT Nom FROM Usuaris WHERE IdGrup = @grup";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", grupActiu);

                    // Llegim tots els registres i afegim el camp Nom a la llista.
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

        // Carrega els participants vinculats a un projecte específic.
        // Fa un JOIN entre Usuaris i Usuaris_projectes per obtenir els noms.
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

        // Afegeix un participant a un projecte.
        // Converteix el nom a IdUsuari i inserta el vincle a Usuaris_projectes.
        // "INSERT IGNORE" evita duplicats si ja estava afegit.
        public void AfegirParticipantAProjecte(string nom, int grupActiu, int? idProjecte)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);

                // Si no podem obtenir IdUsuari o no hi ha projecte, no fem res.
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

        // Treu (desvincula) un participant d'un projecte.
        // El participant continua existint a Usuaris, només desapareix el vincle a Usuaris_projectes.
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

        // Elimina un usuari del grup a la base de dades.
        // Abans d'esborrar l'usuari, neteja totes les relacions per no tenir fallos a la bdd:
        // - Tasques: si era responsable, posa IdUsuariResponsable a NULL
        // - Projectes: si era Sprint Master, posa IdResponsable a NULL
        // - Usuaris_projectes: elimina tots els vincles usuari-projecte
        // - I finalment, elimina el registre a Usuaris
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

        // Mètode intern per obtenir l'IdUsuari a partir del nom i el grup.
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

        // Mètode intern (ara mateix no s'utilitza directament en aquest servei).
        // Retorna l'IdProjecte de l'últim projecte del grup.
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

        // Executa una comanda SQL que rep el paràmetre @id.
        // Es fa servir per reutilitzar el mateix patró en les operacions d'eliminació.
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
