using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{
    /// <summary>
    /// Model de dades per representar una tasca del Kanban.
    /// </summary>
    public class Tasques
    {
        public int IdTasca { get; set; }
        public int IdProjecte { get; set; }
        public byte IdColumna { get; set; }
        public string Titol { get; set; }
        public string Descripcio { get; set; }
        public string Estat { get; set; }
        public string Responsable { get; set; }
        public DateTime DataVenciment { get; set; }
        public int Prioritat { get; set; }
        public DateTime DataCreacio { get; set; }
        public string Notes { get; set; }

        public string PrioritatText
        {
            get
            {
                switch (Prioritat)
                {
                    case 1: return "Alta";
                    case 2: return "Mitja";
                    case 3: return "Baixa";
                    default: return "Sense";
                }
            }
        }

        public override string ToString() => Titol;
    }

    /// <summary>
    /// Servei per gestionar totes les operacions de base de dades i lògica de negoci del Kanban.
    /// </summary>
    public class KanbanService
    {
        #region Participants

        /// <summary>
        /// Carrega els participants del grup actiu des de la base de dades.
        /// </summary>
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

        /// <summary>
        /// Afegeix un participant a un projecte.
        /// </summary>
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

        /// <summary>
        /// Desvincula un participant d'un projecte.
        /// </summary>
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

        /// <summary>
        /// Elimina un usuari de la base de dades.
        /// </summary>
        public void EliminarUsuari(string nom, int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nom, grupActiu);
                if (!idUsuari.HasValue)
                    throw new InvalidOperationException("No s'ha trobat l'usuari.");

                ExecutarComanda(conn, "DELETE FROM Usuaris_projectes WHERE IdUsuari = @id", idUsuari.Value);
                ExecutarComanda(conn, "UPDATE Tasca SET IdUsuariResponsable = NULL WHERE IdUsuariResponsable = @id", idUsuari.Value);
                ExecutarComanda(conn, "DELETE FROM Usuaris WHERE IdUsuari = @id", idUsuari.Value);
            }
        }

        #endregion

        #region Projectes

        /// <summary>
        /// Obté el títol del projecte actiu.
        /// </summary>
        public string ObtenirTitolProjecteActiu(int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                const string sql = @"SELECT Titol FROM Projectes 
                                     WHERE IdGrup = @grup 
                                     ORDER BY IdProjecte DESC LIMIT 1";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", grupActiu);
                    object result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        /// <summary>
        /// Obté l'ID del projecte actiu.
        /// </summary>
        public int? ObtenirProjecteActiuId(int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                return ObtenirProjecteActiuId(conn, grupActiu);
            }
        }

        /// <summary>
        /// Actualitza el responsable (Sprint Master) d'un projecte.
        /// </summary>
        public void ActualitzarSprintMaster(string nomUsuari, int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nomUsuari, grupActiu);
                if (!idUsuari.HasValue) return;

                const string sql = @"UPDATE Projectes SET IdResponsable = @idUsuari 
                                     WHERE IdGrup = @grup ORDER BY IdProjecte DESC LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idUsuari", idUsuari.Value);
                    cmd.Parameters.AddWithValue("@grup", grupActiu);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Obté el nom del responsable d'un projecte per ID.
        /// </summary>
        public string ObtenirNomResponsable(int? idResponsable)
        {
            if (!idResponsable.HasValue) return null;

            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                const string sql = "SELECT Nom FROM Usuaris WHERE IdUsuari = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idResponsable.Value);
                    object nom = cmd.ExecuteScalar();
                    return nom?.ToString();
                }
            }
        }

        #endregion

        #region Tasques

        /// <summary>
        /// Carrega les tasques d'un projecte des de la base de dades.
        /// </summary>
        public List<Tasques> CarregarTasquesProjecte(int idProjecte)
        {
            var tasques = new List<Tasques>();

            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                const string sql = @"SELECT t.IdTasca, t.IdProjecte, t.IdColumna, t.Descripcio,
                                            t.Prioritat, t.DataCreacio, t.DataVenciment,
                                            u.Nom AS NomResponsable
                                     FROM Tasca t
                                     LEFT JOIN Usuaris u ON u.IdUsuari = t.IdUsuariResponsable
                                     WHERE t.IdProjecte = @idProjecte";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idProjecte", idProjecte);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasques.Add(MapTascaFromReader(reader));
                        }
                    }
                }
            }

            return tasques;
        }

        /// <summary>
        /// Actualitza la columna d'una tasca a la base de dades.
        /// </summary>
        public void ActualitzarColumnaTasca(Tasques tasca)
        {
            if (tasca.IdTasca <= 0) return;

            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                const string sql = "UPDATE Tasca SET IdColumna = @idColumna WHERE IdTasca = @idTasca";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idColumna", tasca.IdColumna);
                    cmd.Parameters.AddWithValue("@idTasca", tasca.IdTasca);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Insereix una nova tasca a la base de dades i retorna el seu ID.
        /// </summary>
        public int InserirTasca(Tasques tasca, int grupActiu)
        {
            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                int? idProjecte = ObtenirProjecteActiuId(conn, grupActiu);
                if (!idProjecte.HasValue)
                    throw new InvalidOperationException("No hi ha cap projecte actiu per al grup actual.");

                tasca.IdProjecte = idProjecte.Value;
                int? idUsuariResponsable = string.IsNullOrEmpty(tasca.Responsable)
                    ? (int?)null
                    : ObtenirIdUsuariPerNom(conn, tasca.Responsable, grupActiu);

                const string sql = @"INSERT INTO Tasca
                                        (IdProjecte, IdColumna, IdUsuariResponsable, Descripcio, Prioritat, DataCreacio, DataVenciment)
                                      VALUES
                                        (@idProjecte, @idColumna, @idUsuariResponsable, @descripcio, @prioritat, @dataCreacio, @dataVenciment);
                                      SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idProjecte", tasca.IdProjecte);
                    cmd.Parameters.AddWithValue("@idColumna", tasca.IdColumna);
                    cmd.Parameters.AddWithValue("@idUsuariResponsable", (object)idUsuariResponsable ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@descripcio", tasca.Descripcio);
                    cmd.Parameters.AddWithValue("@prioritat", tasca.Prioritat);
                    cmd.Parameters.AddWithValue("@dataCreacio", tasca.DataCreacio);
                    cmd.Parameters.AddWithValue("@dataVenciment",
                        tasca.DataVenciment == DateTime.MinValue ? (object)DBNull.Value : tasca.DataVenciment);

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Actualitza els detalls d'una tasca existent a la base de dades.
        /// </summary>
        public void ActualitzarDetallsTasca(Tasques tasca, int grupActiu)
        {
            if (tasca.IdTasca <= 0) return;

            using (var conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                int? idUsuariResponsable = string.IsNullOrEmpty(tasca.Responsable)
                    ? (int?)null
                    : ObtenirIdUsuariPerNom(conn, tasca.Responsable, grupActiu);

                const string sql = @"UPDATE Tasca
                                        SET Descripcio = @descripcio,
                                            Prioritat = @prioritat,
                                            DataVenciment = @dataVenciment,
                                            IdUsuariResponsable = @responsable
                                        WHERE IdTasca = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@descripcio", tasca.Descripcio);
                    cmd.Parameters.AddWithValue("@prioritat", tasca.Prioritat);
                    cmd.Parameters.AddWithValue("@dataVenciment",
                        tasca.DataVenciment == DateTime.MinValue ? (object)DBNull.Value : tasca.DataVenciment);
                    cmd.Parameters.AddWithValue("@responsable", (object)idUsuariResponsable ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", tasca.IdTasca);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Ordena una llista de tasques per prioritat, responsable i descripció.
        /// </summary>
        public void OrdenarLlista(List<Tasques> llista)
        {
            llista?.Sort(CompararTasques);
        }

        #endregion

        #region Utilitats de columnes

        /// <summary>
        /// Retorna l'estat en text per una columna.
        /// </summary>
        public static string GetEstatPerColumna(byte idColumna)
        {
            switch (idColumna)
            {
                case 1: return "Backlog";
                case 2: return "ToDo";
                case 3: return "Doing";
                case 4: return "Done";
                default: return string.Empty;
            }
        }

        #endregion

        #region Mètodes privats

        private int? ObtenirProjecteActiuId(MySqlConnection conn, int grupActiu)
        {
            const string sql = @"SELECT IdProjecte FROM Projectes 
                                 WHERE IdGrup = @grup ORDER BY IdProjecte DESC LIMIT 1";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@grup", grupActiu);
                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        private int? ObtenirIdUsuariPerNom(MySqlConnection conn, string nomUsuari, int grupActiu)
        {
            const string sql = "SELECT IdUsuari FROM Usuaris WHERE Nom = @nom AND IdGrup = @grup";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@nom", nomUsuari);
                cmd.Parameters.AddWithValue("@grup", grupActiu);
                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        private void ExecutarComanda(MySqlConnection conn, string sql, int idUsuari)
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", idUsuari);
                cmd.ExecuteNonQuery();
            }
        }

        private Tasques MapTascaFromReader(MySqlDataReader reader)
        {
            var tasca = new Tasques
            {
                IdTasca = Convert.ToInt32(reader["IdTasca"]),
                IdProjecte = Convert.ToInt32(reader["IdProjecte"]),
                IdColumna = Convert.ToByte(reader["IdColumna"]),
                Descripcio = reader["Descripcio"].ToString(),
                Titol = reader["Descripcio"].ToString(),
                Responsable = reader["NomResponsable"] == DBNull.Value ? null : reader["NomResponsable"].ToString(),
                Prioritat = reader["Prioritat"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Prioritat"]),
                DataCreacio = reader["DataCreacio"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["DataCreacio"]),
                DataVenciment = reader["DataVenciment"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["DataVenciment"])
            };
            tasca.Estat = GetEstatPerColumna(tasca.IdColumna);
            return tasca;
        }

        private static int CompararTasques(Tasques x, Tasques y)
        {
            if (x == null || y == null) return 0;

            int cmp = x.Prioritat.CompareTo(y.Prioritat);
            if (cmp != 0) return cmp;

            string respX = string.IsNullOrWhiteSpace(x.Responsable) ? "~~~~" : x.Responsable;
            string respY = string.IsNullOrWhiteSpace(y.Responsable) ? "~~~~" : y.Responsable;
            cmp = string.Compare(respX, respY, StringComparison.CurrentCultureIgnoreCase);
            if (cmp != 0) return cmp;

            return string.Compare(x.Descripcio ?? "", y.Descripcio ?? "", StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion
    }
}
