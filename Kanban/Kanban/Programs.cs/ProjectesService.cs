using System;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{
    public class ProjectesService
    {
        public string ObtenirTitolProjecteActiu(int grupActiu)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = @"SELECT Titol FROM Projectes 
                                     WHERE IdGrup = @grup 
                                     ORDER BY IdProjecte DESC LIMIT 1";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", grupActiu);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
        }

        public int ObtenirProjecteActiuId(int grupActiu)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = @"SELECT IdProjecte FROM Projectes 
                                     WHERE IdGrup = @grup ORDER BY IdProjecte DESC LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", grupActiu);
                    var result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
        }

        public void ActualitzarSprintMaster(string nomUsuari, int grupActiu)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
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

        public string ObtenirNomResponsable(int? idResponsable)
        {
            if (!idResponsable.HasValue)
                return "";

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = "SELECT Nom FROM Usuaris WHERE IdUsuari = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idResponsable.Value);
                    var nom = cmd.ExecuteScalar();
                    return nom?.ToString() ?? "";
                }
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
    }
}
