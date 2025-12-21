using System;
using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    // Finestra per crear un nou Sprint/Projecte.
    // Desa el projecte a la taula Projectes i retorna el títol creat a la finestra principal.
    public partial class CrearProjecteWindow : Window
    {
        // Propietat per retornar el títol del projecte creat al MainWindow.
        public string TitolProjecteCreat { get; private set; }

        public CrearProjecteWindow()
        {
            InitializeComponent();
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            // Validació bàsica del títol.
            if (string.IsNullOrWhiteSpace(txtTitol.Text))
            {
                MessageBox.Show("Introdueix un títol");
                return;
            }

            // Llegim la DataFi del DatePicker.
            // Si la data seleccionada és anterior a avui, la forcem a avui.
            DateTime? dataFi = null;
            if (dpDataFi.SelectedDate.HasValue)
            {
                dataFi = dpDataFi.SelectedDate.Value >= DateTime.Today 
                    ? dpDataFi.SelectedDate.Value 
                    : DateTime.Today;
            }

            // Inserim el projecte a la BDD.
            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = @"INSERT INTO Projectes (Titol, DataInici, DataFi, IdGrup)
                                     VALUES (@titol, @dataInici, @dataFi, @idGrup)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@titol", txtTitol.Text);

                    // Data d'inici: ara mateix.
                    cmd.Parameters.AddWithValue("@dataInici", DateTime.Now);

                    // Data fi: pot ser NULL si no s'ha seleccionat.
                    cmd.Parameters.AddWithValue("@dataFi", (object)dataFi ?? DBNull.Value);

                    // Grup actiu: el grup amb el qual s'ha fet login.
                    cmd.Parameters.AddWithValue("@idGrup", DataBase.grupActiu);
                    cmd.ExecuteNonQuery();
                }
            }

            // Guardem el títol per retornar-lo.
            TitolProjecteCreat = txtTitol.Text;

            // Tanquem la finestra retornant OK.
            DialogResult = true;
            Close();
        }
    }
}

