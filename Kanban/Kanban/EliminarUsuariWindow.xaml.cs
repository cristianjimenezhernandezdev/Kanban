using System;
using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    public partial class EliminarUsuariWindow : Window
    {
        public string UsuariSeleccionat { get; private set; }

        public EliminarUsuariWindow()
        {
            InitializeComponent();
            CarregarUsuaris();
        }

        private void CarregarUsuaris()
        {
            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                const string sql = "SELECT Nom FROM Usuaris WHERE IdGrup = @grup";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbUsuaris.Items.Add(reader["Nom"].ToString());
                        }
                    }
                }
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUsuaris.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un usuari.");
                return;
            }

            UsuariSeleccionat = cmbUsuaris.SelectedItem.ToString();
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
