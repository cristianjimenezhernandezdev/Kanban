using System;
using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    // Finestra per seleccionar un usuari del grup i retornar-lo al MainWindow.
    // Aquesta finestra no elimina res directament: només retorna el nom.
    // El MainWindow és qui demana confirmació i crida ParticipantsService.EliminarUsuari.
    public partial class EliminarUsuariWindow : Window
    {
        // Nom de l'usuari seleccionat.
        public string UsuariSeleccionat { get; private set; }

        public EliminarUsuariWindow()
        {
            InitializeComponent();

            // Carreguem els usuaris del grup al ComboBox.
            CarregarUsuaris();
        }

        private void CarregarUsuaris()
        {
            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = "SELECT Nom FROM Usuaris WHERE IdGrup = @grup";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@grup", DataBase.grupActiu);
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
            // Validació: cal seleccionar un usuari.
            if (cmbUsuaris.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un usuari.");
                return;
            }

            // Guardem el nom seleccionat i tanquem amb OK.
            UsuariSeleccionat = cmbUsuaris.SelectedItem.ToString();
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Tanquem sense eliminar res.
            DialogResult = false;
            Close();
        }
    }
}
