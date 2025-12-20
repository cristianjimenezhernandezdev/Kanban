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
            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = @"SELECT IdProjecte, Titol 
                                     FROM Projectes 
                                     WHERE IdGrup = @grup";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", DataBase.grupActiu);

                    using (var reader = cmd.ExecuteReader())
                    {
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
            }
        }

        private void BtnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSeleccionarProjectes.SelectedItem == null)
                return;

            var item = (ComboBoxItem)cmbSeleccionarProjectes.SelectedItem;

            IdProjecteSeleccionat = (int)item.Tag;
            TitolProjecteSeleccionat = item.Content.ToString();

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = @"SELECT IdResponsable 
                                     FROM Projectes 
                                     WHERE IdProjecte = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", IdProjecteSeleccionat);
                    var result = cmd.ExecuteScalar();
                    IdResponsableSeleccionat = (result == null || result == DBNull.Value) 
                        ? (int?)null 
                        : Convert.ToInt32(result);
                }
            }

            DialogResult = true;
            Close();
        }
    }
}
