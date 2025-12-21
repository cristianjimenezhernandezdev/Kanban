using System;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{

    // Aquí hi ha les consultes bàsiques per:
    // - llegir el projecte "actiu" (en aquest programa, normalment l'últim projecte creat del grup)
    // - llegir i actualitzar el responsable (Sprint Master)
    public class ProjectesService
    {
        // Retorna el títol del projecte "actiu" del grup.
        // "Projecte actiu" aquí vol dir: l'últim projecte creat (IdProjecte més alt) dins del grup.
        public string ObtenirTitolProjecteActiu(int grupActiu)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                // Busquem el camp Titol del darrer projecte del grup.
                const string sql = @"SELECT Titol FROM Projectes 
                                     WHERE IdGrup = @grup 
                                     ORDER BY IdProjecte DESC LIMIT 1";
                
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    // Paràmetre per evitar SQL injection i filtrar per grup.
                    cmd.Parameters.AddWithValue("@grup", grupActiu);
                    
                    // ExecuteScalar retorna la primera columna de la primera fila.
                    var result = cmd.ExecuteScalar();

                    // Si no hi ha resultat, retornem string buit.
                    return result?.ToString() ?? "";

                }             

                }

          }
        
        // Retorna la DataFi del projecte "actiu" del grup.
        // La data es retorna en format dd/MM/yyyy i sense hora.
        public string ObtenirDataProjecteActiu(int grupActiu)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
               
                // Busquem el camp DataFi del darrer projecte del grup.
                const string sql = @"SELECT DataFi FROM Projectes 
                                     WHERE IdGrup = @grup 
                                     ORDER BY IdProjecte DESC LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", grupActiu);

                    var result = cmd.ExecuteScalar();

                    // Si a la BDD no hi ha data (NULL) o no hi ha projecte, retornem buit.
                    if (result == null || result == DBNull.Value)
                        return string.Empty;
                    
                    // Convertim a DateTime i mostrem només el dia/mes/any.
                    var data = Convert.ToDateTime(result);
                    return data.ToString("dd/MM/yyyy");
                }

            }

        }
    
        // Retorna l'IdProjecte del projecte "actiu" del grup.
        // En aquest programa és l'últim projecte creat del grup (IdProjecte DESC LIMIT 1).
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

                    // Si no hi ha projecte, retornem 0.
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
        }

        // Actualitza el Sprint Master (IdResponsable) d'un projecte concret.
        // Rep el nom del participant i el converteix a IdUsuari.
        // Després fa UPDATE a la taula Projectes.
        public void ActualitzarSprintMaster(string nomUsuari, int grupActiu, int idProjecte)
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                // Busquem l'IdUsuari del grup que coincideix amb el nom seleccionat.
                int? idUsuari = ObtenirIdUsuariPerNom(conn, nomUsuari, grupActiu);

                // Si no trobem usuari o l'id del projecte no és vàlid, no fem res.
                if (!idUsuari.HasValue) return;
                if (idProjecte <= 0) return;

                // Assignem IdResponsable al projecte (Sprint Master).
                const string sql = @"UPDATE Projectes SET IdResponsable = @idUsuari 
                                     WHERE IdProjecte = @idProjecte AND IdGrup = @grup";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idUsuari", idUsuari.Value);
                    cmd.Parameters.AddWithValue("@idProjecte", idProjecte);
                    cmd.Parameters.AddWithValue("@grup", grupActiu);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Amb IdUsuari, retorna el Nom del usuari.
        // Es fa servir per mostrar el nom del Sprint Master a la interfície.
        public string ObtenirNomResponsable(int? idResponsable)
        {
            // Si no hi ha responsable assignat, retornem buit.
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

        // Mètode private per obtenir l'IdUsuari a partir del nom dins d'un grup.
        // Serveix per passar de "nom" (UI) a "id" (BDD).
        private static int? ObtenirIdUsuariPerNom(MySqlConnection conn, string nomUsuari, int grupActiu)
        {
            const string sql = "SELECT IdUsuari FROM Usuaris WHERE Nom = @nom AND IdGrup = @grup";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@nom", nomUsuari);
                cmd.Parameters.AddWithValue("@grup", grupActiu);

                var result = cmd.ExecuteScalar();

                // Si no hi ha resultat, retornem null.
                return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
            }
        }

        // Retorna l'IdResponsable (Sprint Master) d'un projecte concret.
        // Si a la BDD està a NULL, retorna null.
        public int? ObtenirIdResponsableProjecte(int idProjecte)
        {
            // Si l'id del projecte no és vàlid, retornem null.
            if (idProjecte <= 0) return null;

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = @"SELECT IdResponsable FROM Projectes WHERE IdProjecte = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idProjecte);

                    var result = cmd.ExecuteScalar();

                    // Si és NULL o no existeix fila, retornem null.
                    return result == null || result == DBNull.Value ? (int?)null : Convert.ToInt32(result);
                }
            }
        }
    }
}
