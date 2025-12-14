using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    /// <summary>
    /// Lógica de interacción para AfegirParticipantsWindow.xaml
    /// </summary>
    public partial class AfegirParticipantsWindow : Window
    {
        public AfegirParticipantsWindow()
        {
            InitializeComponent();
        }

        private void btnAfegir_Click(object sender, RoutedEventArgs e)
        {
            string nom = txtNom.Text;
            string cognom = txtCognom.Text;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(cognom))
            {
                MessageBox.Show("Cal omplir tots els camps.");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                string query = @"INSERT INTO Usuaris (Nom, Cognom, IdGrup)
                                 VALUES (@nom, @cognom, @idGrup)";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@cognom", cognom);
                cmd.Parameters.AddWithValue("@idGrup", LoginWindow.grupActiu);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Participant afegit correctament.");
            this.DialogResult = true;
            this.Close();
        }
    }
}
