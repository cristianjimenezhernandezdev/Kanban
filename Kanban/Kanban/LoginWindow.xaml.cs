using System;
using System.Data.SqlClient;
using System.Windows;
using MySql.Data.MySqlClient;


namespace Kanban
{
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// http://ellaboratori.cat/phpmyadmin/index.php
    /// user: amine pass: campa123
    /// </summary>
    public partial class LoginWindow : Window
    {
        //string connectionString = "Server=http://ellaboratori.cat/phpmyadmin/index.php;Port=3306;Database=amine;Uid=amine;Pwd=campa123;";

        //string connectionString = "Server=http://ellaboratori.cat/phpmyadmin/index.php;Database=amine;Password=campa123";

        string connectionString = "Server=NITRO-AMINE;Database=ProjecteKanban;Trusted_Connection=True;";

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

                string query = "SELECT COUNT(*) FROM Grups WHERE Nom=@user AND Codi=@pass";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", usuari);
                cmd.Parameters.AddWithValue("@pass", contrasenya);

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count > 0)
                {
                    new MainWindow().Show();
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
