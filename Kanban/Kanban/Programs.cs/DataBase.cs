using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Kanban.Programs.cs
{
    public static class Database
    {
        //public static string connectionString ="Server=ellaboratori.cat;Port=3306;Database=amine;Uid=amine;Pwd=campa1234;SslMode=None;"
        public static string connectionString = "Server=NITRO-AMINE;Database=ProjecteKanban;Trusted_Connection=True;";

        //private static string laMevaConnexio =
        //       ConfigurationManager.ConnectionStrings["GestioComandesConnectionString"].ConnectionString;

        //laMevaConnexioSql = new MySqlConnection(laMevaConnexio);

        //private void MostraClients()
        //{
        //    try
        //    {
        //        string consulta = "SELECT * FROM CLIENT";

        //        MySqlDataAdapter adaptador = new MySqlDataAdapter(consulta, laMevaConnexioSql);

        //        DataTable dt = new DataTable();
        //        adaptador.Fill(dt);

        //        taulaClients.ItemsSource = dt.DefaultView;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}




        // 🔵 SELECT → retorna DataTable
        public static DataTable Select(string query)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        // 🔴 INSERT / UPDATE / DELETE
        public static void Execute(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
