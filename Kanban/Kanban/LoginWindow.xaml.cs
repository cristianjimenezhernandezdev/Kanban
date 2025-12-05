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

namespace Kanban
{
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// http://ellaboratori.cat/phpmyadmin/index.php
    /// user: amine pass: campa123
    /// </summary>
    public partial class LoginWindow : Window
    {
        string connectionString = "Server=http://ellaboratori.cat/phpmyadmin/index.php;Database=amine;Password=campa123";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuari = txtUsuari.Text;
            string contrasenya = txtContrasenya.Password;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM UsuarisLogin " +
                               "WHERE NomUsuari=@user AND Contrasenya=@pass";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", usuari);
                cmd.Parameters.AddWithValue("@pass", contrasenya);

                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    // Login correcte
                    MainWindow mw = new MainWindow();
                    mw.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuari o contrasenya incorrectes.");
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Tancar la finestra de login quan clico cancelar
            this.Close();
        }
    }
}
