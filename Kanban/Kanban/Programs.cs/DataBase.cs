using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{

    // Classe estatica per centralitzar la connexio a la base de dades.
    // Tot el programa fa servir aquest punt unic per obtenir connexions MySQL.
    public static class DataBase
    {
        // connexio cap al servidor MySQL.
        // Conté servidor, port, base de dades, usuari i contrasenya.
        public static string connectionString = "Server=ellaboratori.cat;Port=3306;Database=amine;Uid=amine;Pwd=campa123;SslMode=Disabled;";

        // Identificador del grup amb el qual s'ha iniciat sessio.
        // S'omple al Login i es fa servir a totes les consultes per filtrar dades del grup.
        public static int grupActiu;
       
        // Retorna una nova connexio MySQL oberta i llesta per fer servir.
        // IMPORTANT: cada metode que crida ObtenirConnexio() ha de tancar la connexio amb el using.
        public static MySqlConnection ObtenirConnexio()
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}
