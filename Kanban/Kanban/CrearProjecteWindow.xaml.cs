using System;
using System.Data.SqlClient;
using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    /// <summary>
    /// Lógica de interacción para CrearProjecteWindow.xaml
    /// </summary>
    public partial class CrearProjecteWindow : Window
    {
        public CrearProjecteWindow()
        {
            InitializeComponent();
        }
        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitol.Text))
            {
                MessageBox.Show("Introdueix un títol");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO Projectes
                    (Titol, DataInici, DataFi, IdGrup)
                    VALUES
                    (@titol, @dataInici, @dataFi, @idGrup)";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@titol", txtTitol.Text);
                cmd.Parameters.AddWithValue("@dataInici", DateTime.Now);
                cmd.Parameters.AddWithValue("@dataFi", (object)dpDataFi.SelectedDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idGrup", LoginWindow.grupActiu);

                cmd.ExecuteNonQuery();
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}

