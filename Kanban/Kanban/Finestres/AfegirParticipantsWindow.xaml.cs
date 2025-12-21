using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    /// <summary>
    /// Finestra per afegir un nou participant (usuari) al grup.
    /// guarda el participant a la taula Usuaris.
    /// </summary>
    public partial class AfegirParticipantsWindow : Window
    {
        public AfegirParticipantsWindow()
        {
            InitializeComponent();
        }

        private void btnAfegir_Click(object sender, RoutedEventArgs e)
        {
            // Llegim el formulari.
            string nom = txtNom.Text;
            string cognom = txtCognom.Text;

            // Validació bàsica.
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(cognom))
            {
                MessageBox.Show("Cal omplir tots els camps.");
                return;
            }

            // Inserim l'usuari a la BDD.
            using (var conn = DataBase.ObtenirConnexio())
            {
                const string query = @"INSERT INTO Usuaris (Nom, Cognom, IdGrup)
                                       VALUES (@nom, @cognom, @idGrup)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@cognom", cognom);

                    // L'usuari queda vinculat al grup actiu.
                    cmd.Parameters.AddWithValue("@idGrup", DataBase.grupActiu);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Participant afegit correctament.");

            // Retornem OK i tanquem.
            DialogResult = true;
            Close();
        }
    }
}
