using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{

    public static class DataBase
    {
        public static string connectionString = "Server=ellaboratori.cat;Port=3306;Database=amine;Uid=amine;Pwd=campa123;SslMode=Disabled;";
        public static int grupActiu;
       
        // Retorna una nova connexió MySQL oberta i llesta per fer servir
        public static MySqlConnection ObtenirConnexio()
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}
