using System;
using System.Data.SqlClient;
using System.Windows;
using Kanban.Programs.cs;
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
        //string connectionString = "Server=http://ellaboratori.cat;Port=3306;Database=amine;Uid=amine;Pwd=campa123;";

        //string connectionString = "Server=http://ellaboratori.cat/phpmyadmin/index.php;Database=amine;Password=campa123";

        //static public string connectionString = "Server=NITRO-AMINE;Database=ProjecteKanban;Trusted_Connection=True;";
        //public static string connectionString = "Server=ellaboratori.cat;Port=3306;Database=amine;Uid=amine;Pwd=campa1234;SslMode=None;";

        public static int grupActiu;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Tancar la finestra de login quan clico cancelar
            this.Close();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuari = txtUsuari.Text;
            string contrasenya = txtContrasenya.Password;

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                // 1️⃣ Primer comprovem si l'usuari i contrasenya existeixen
                string queryLogin = "SELECT COUNT(*) FROM Grups WHERE Nom=@user AND Codi=@pass";

                MySqlCommand cmdLogin = new MySqlCommand(queryLogin, conn);
                cmdLogin.Parameters.AddWithValue("@user", usuari);
                cmdLogin.Parameters.AddWithValue("@pass", contrasenya);

                int count = Convert.ToInt32(cmdLogin.ExecuteScalar());

                if (count == 0)
                {
                    MessageBox.Show("Usuari o contrasenya incorrectes.");
                    return;
                }

                // 2️⃣ Obtenir IdGrup del grup logat
                string queryGrup = "SELECT IdGrup FROM Grups WHERE Nom=@user AND Codi=@pass";

                MySqlCommand cmdGrup = new MySqlCommand(queryGrup, conn);
                cmdGrup.Parameters.AddWithValue("@user", usuari);
                cmdGrup.Parameters.AddWithValue("@pass", contrasenya);

                int idGrup = Convert.ToInt32(cmdGrup.ExecuteScalar());

                // 3️⃣ Guardar IdGrup al camp estàtic accessible des de tot el projecte
                grupActiu = idGrup;

                // 4️⃣ Obrir MainWindow
                MainWindow mw = new MainWindow();
                mw.Show();
                this.Close();
            }
        }
    }
}
