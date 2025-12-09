using System.Data;
using System.Data.SqlClient;

namespace Kanban.Programs.cs
{
    public static class Database
    {
        public static string connectionString = "Server=NITRO-AMINE;Database=ProjecteKanban;Trusted_Connection=True;";

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
