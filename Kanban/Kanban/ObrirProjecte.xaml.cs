using System;
using System.Windows;
using System.Windows.Controls;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    /// <summary>
    /// Lógica de interacción para ObrirProjecte.xaml
    /// </summary>
    public partial class ObrirProjecte : Window
    {
        public int IdProjecteSeleccionat { get; private set; }
        public string TitolProjecteSeleccionat { get; private set; }
        public int? IdResponsableSeleccionat { get; private set; }

        public ObrirProjecte()
        {
            InitializeComponent();
            CarregarProjectes();
        }

        private void CarregarProjectes()
        {
            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                string sqlProjectes = @"SELECT IdProjecte, Titol 
                                FROM Projectes 
                                WHERE IdGrup = @grup";

                MySqlCommand cmd = new MySqlCommand(sqlProjectes, conn);
                cmd.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                MySqlDataReader reader = cmd.ExecuteReader();

                cmbSeleccionarProjectes.Items.Clear();

                while (reader.Read())
                {
                    cmbSeleccionarProjectes.Items.Add(new ComboBoxItem
                    {
                        Content = reader["Titol"].ToString(),
                        Tag = reader["IdProjecte"]
                    });
                }
            }
        }

        private void BtnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSeleccionarProjectes.SelectedItem == null)
                return;

            ComboBoxItem item = (ComboBoxItem)cmbSeleccionarProjectes.SelectedItem;

            IdProjecteSeleccionat = (int)item.Tag;
            TitolProjecteSeleccionat = item.Content.ToString();

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                string sql = @"SELECT IdResponsable 
                                        FROM Projectes 
                                        WHERE IdProjecte = @id";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", IdProjecteSeleccionat);

                object resultat = cmd.ExecuteScalar();
                if (resultat != DBNull.Value && resultat != null)
                    IdResponsableSeleccionat = Convert.ToInt32(resultat);
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}
