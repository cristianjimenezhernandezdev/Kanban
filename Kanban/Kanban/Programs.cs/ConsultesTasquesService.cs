using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{
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

    public class ConsultesTasquesService
    {
        public List<Tasques> CarregarTasquesProjecte(int idProjecte)
        {
            var tasques = new List<Tasques>();

            using (var conn = DataBase.ObtenirConnexio())
            {
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

        public void ActualitzarColumnaTasca(Tasques tasca)
        {
            if (tasca.IdTasca <= 0) return;

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = "UPDATE Tasca SET IdColumna = @idColumna WHERE IdTasca = @idTasca";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idColumna", tasca.IdColumna);
                    cmd.Parameters.AddWithValue("@idTasca", tasca.IdTasca);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int InserirTasca(Tasques tasca, int grupActiu, int idProjecte)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                // Utilitzem l'idProjecte que ens passen, no el busquem
                if (idProjecte <= 0)
                    throw new InvalidOperationException("No hi ha cap projecte actiu.");

                tasca.IdProjecte = idProjecte;
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

                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public void ActualitzarDetallsTasca(Tasques tasca, int grupActiu)
        {
            if (tasca.IdTasca <= 0) return;

            using (var conn = DataBase.ObtenirConnexio())
            {
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

        public void OrdenarLlista(List<Tasques> llista)
        {
            llista?.Sort(CompararTasques);
        }

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

        // Aixo agafa de la BDD i la reparteix per fer-la servir
        private static Tasques MapTascaFromReader(MySqlDataReader reader)
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
    }
}
