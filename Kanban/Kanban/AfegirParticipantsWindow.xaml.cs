using System.Windows;
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

            using (var conn = DataBase.ObtenirConnexio())
            {
                const string query = @"INSERT INTO Usuaris (Nom, Cognom, IdGrup)
                                       VALUES (@nom, @cognom, @idGrup)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@cognom", cognom);
                    cmd.Parameters.AddWithValue("@idGrup", DataBase.grupActiu);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Participant afegit correctament.");
            DialogResult = true;
            Close();
        }
    }
}
