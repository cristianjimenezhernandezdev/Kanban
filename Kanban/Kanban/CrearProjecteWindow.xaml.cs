using System;
using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    public partial class CrearProjecteWindow : Window
    {
        public string TitolProjecteCreat { get; private set; }

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

            // Si la data de fi és anterior a avui o no s'ha seleccionat, posem avui
            DateTime? dataFi = null;
            if (dpDataFi.SelectedDate.HasValue)
            {
                dataFi = dpDataFi.SelectedDate.Value >= DateTime.Today 
                    ? dpDataFi.SelectedDate.Value 
                    : DateTime.Today;
            }

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string sql = @"INSERT INTO Projectes (Titol, DataInici, DataFi, IdGrup)
                                     VALUES (@titol, @dataInici, @dataFi, @idGrup)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@titol", txtTitol.Text);
                    cmd.Parameters.AddWithValue("@dataInici", DateTime.Now);
                    cmd.Parameters.AddWithValue("@dataFi", (object)dataFi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idGrup", DataBase.grupActiu);
                    cmd.ExecuteNonQuery();
                }
            }

            TitolProjecteCreat = txtTitol.Text;
            DialogResult = true;
            Close();
        }
    }
}

