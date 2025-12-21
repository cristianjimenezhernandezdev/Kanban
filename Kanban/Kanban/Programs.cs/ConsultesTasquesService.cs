using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{
    // Model de dades d'una tasca.
    // Aquesta classe s'utilitza per mostrar tasques a la UI i per enviar/recuperar dades de la BDD.
    public class Tasques
    {
        // Identificador únic de la tasca (clau primària)
        public int IdTasca { get; set; }

        // Projecte al qual pertany la tasca
        public int IdProjecte { get; set; }

        // Columna del Kanban (1 Backlog, 2 ToDo, 3 Doing, 4 Done)
        public byte IdColumna { get; set; }

        // Títol 
        public string Titol { get; set; }

        // Text principal de la tasca
        public string Descripcio { get; set; }

        // Estat en format text (Backlog/ToDo/Doing/Done)
        public string Estat { get; set; }

        // Nom del responsable (es guarda a la tasca com a nom per mostrar-ho a la UI)
        public string Responsable { get; set; }

        // Data de venciment
        public DateTime DataVenciment { get; set; }

        // Prioritat numèrica: 1 Alta, 2 Mitja, 3 Baixa
        public int Prioritat { get; set; }

        // Data de creació de la tasca
        public DateTime DataCreacio { get; set; }

        // Notes extra
        public string Notes { get; set; }

        // Només lectura per mostrar la prioritat en text s'asigna el numero amb la prioritat en text
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

        // Quan una Tasques es mostra en un ComboBox o ListBox sense plantilla,
        // es mostra el valor del ToString().
        public override string ToString() => Titol;
    }

    // carregar, inserir i actualitzar tasques a la base de dades.
    // Obtenim el id
    public class ConsultesTasquesService
    {
        // Carrega totes les tasques d'un projecte.
        // També fa un LEFT JOIN amb Usuaris per obtenir el nom del responsable.
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
                            // Convertim cada fila de la BDD a un objecte Tasques.
                            tasques.Add(MapTascaFromReader(reader));
                        }
                    }
                }
            }

            return tasques;
        }

        // Actualitza només la columna (IdColumna) d'una tasca.
        // S'utilitza quan es fa drag & drop entre columnes.
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

        // Insereix una tasca nova a la base de dades.
        // IMPORTANT: rep l'idProjecte per assegurar que la tasca s'assigna al projecte obert.
        // Retorna l'IdTasca creat (LAST_INSERT_ID()).
        public int InserirTasca(Tasques tasca, int grupActiu, int idProjecte)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                // Si no hi ha projecte vàlid, parem.
                if (idProjecte <= 0)
                    throw new InvalidOperationException("No hi ha cap projecte actiu.");

                // Assignem el projecte a la tasca abans de guardar.
                tasca.IdProjecte = idProjecte;

                // Si hi ha responsable (nom), el convertim a IdUsuari.
                int? idUsuariResponsable = string.IsNullOrEmpty(tasca.Responsable)
                    ? (int?)null
                    : ObtenirIdUsuariPerNom(conn, tasca.Responsable, grupActiu);

                // Insertem la tasca i demanem l'últim id generat.
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

                    // Si no hi ha data de venciment (MinValue), guardem NULL.
                    cmd.Parameters.AddWithValue("@dataVenciment",
                        tasca.DataVenciment == DateTime.MinValue ? (object)DBNull.Value : tasca.DataVenciment);

                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        // Actualitza dades que es poden editaar d'una tasca que hi ha.
        // No canvia el projecte ni la columna, només canvia:
        // - Descripcio
        // - Prioritat
        // - DataVenciment
        // - Responsable
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

        // Ordena una llista de tasques amb el quer hem definit a la funcio CompararTasques.
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

        // Mapeja una fila del reader (resultat SQL) a un objecte Tasques.
        // Aquí s'assignen les propietats i també l'Estat segons IdColumna.
        private static Tasques MapTascaFromReader(MySqlDataReader reader)
        {
            var tasca = new Tasques
            {
                IdTasca = Convert.ToInt32(reader["IdTasca"]),
                IdProjecte = Convert.ToInt32(reader["IdProjecte"]),
                IdColumna = Convert.ToByte(reader["IdColumna"]),

                // En aquesta aplicació la Descripcio també fa de títol.
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

       //Funcio per si la necessitem més endevant de moment no
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

        // Converteix el nom d'usuari (UI) a IdUsuari (BDD) dins d'un grup.
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

        // Comparador de tasques per ordenar.
        // Criteri:
        // 1) Prioritat (numèrica)
        // 2) Responsable (A-Z)
        // 3) Descripcio (A-Z)
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
