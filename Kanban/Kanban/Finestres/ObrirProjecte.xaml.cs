using System;
using System.Windows;
using System.Windows.Controls;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    /// <summary>
    /// Finestra per seleccionar (obrir) un projecte existent.
    /// Mostra un ComboBox amb projectes del grup.
    /// En seleccionar, retorna:
    /// - IdProjecteSeleccionat
    /// - TitolProjecteSeleccionat
    /// - IdResponsableSeleccionat (Sprint Master)
    /// </summary>
    public partial class ObrirProjecte : Window
    {
        // Id del projecte seleccionat
        public int IdProjecteSeleccionat { get; private set; }

        // Títol del projecte seleccionat (només per mostrar-ho al MainWindow)
        public string TitolProjecteSeleccionat { get; private set; }

        // Id del responsable (Sprint Master) del projecte seleccionat
        public int? IdResponsableSeleccionat { get; private set; }

        public ObrirProjecte()
        {
            InitializeComponent();

            // Carreguem la llista de projectes quan s'obre la finestra.
            CarregarProjectes();
        }

        private void CarregarProjectes()
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                // Agafem tots els projectes del grup actiu.
                const string sql = @"SELECT IdProjecte, Titol 
                                     FROM Projectes 
                                     WHERE IdGrup = @grup";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", DataBase.grupActiu);

                    using (var reader = cmd.ExecuteReader())
                    {
                        cmbSeleccionarProjectes.Items.Clear();

                        // Afegim cada projecte com a ComboBoxItem.
                        // - Content: Títol
                        // - Tag: IdProjecte
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
            // Si no hi ha projecte seleccionat, no fem res.
            if (cmbSeleccionarProjectes.SelectedItem == null)
                return;

            // Recuperem el projecte escollit.
            var item = (ComboBoxItem)cmbSeleccionarProjectes.SelectedItem;

            IdProjecteSeleccionat = (int)item.Tag;
            TitolProjecteSeleccionat = item.Content.ToString();

            // Busquem també l'IdResponsable del projecte per poder mostrar l'Sprint Master al MainWindow.
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

            // Retornem OK i tanquem.
            DialogResult = true;
            Close();
        }
    }
}
