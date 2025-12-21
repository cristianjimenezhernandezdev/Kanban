using System;
using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    public partial class LoginWindow : Window
    {
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

            using (var conn = DataBase.ObtenirConnexio())
            {
                // Comprovar si l'usuari i contrasenya existeixen
                const string queryLogin = "SELECT COUNT(*) FROM Grups WHERE Nom=@user AND Codi=@pass";
                using (var cmdLogin = new MySqlCommand(queryLogin, conn))
                {
                    cmdLogin.Parameters.AddWithValue("@user", usuari);
                    cmdLogin.Parameters.AddWithValue("@pass", contrasenya);

                    int count = Convert.ToInt32(cmdLogin.ExecuteScalar());
                    if (count == 0)
                    {
                        MessageBox.Show("Usuari o contrasenya incorrectes.");
                        return;
                    }
                }

                // Obtenir IdGrup del grup logat
                const string queryGrup = "SELECT IdGrup FROM Grups WHERE Nom=@user AND Codi=@pass";
                using (var cmdGrup = new MySqlCommand(queryGrup, conn))
                {
                    cmdGrup.Parameters.AddWithValue("@user", usuari);
                    cmdGrup.Parameters.AddWithValue("@pass", contrasenya);

                    DataBase.grupActiu = Convert.ToInt32(cmdGrup.ExecuteScalar());
                }
            }

            // Obrir MainWindow
            var mw = new MainWindow();
            Application.Current.MainWindow = mw;
            mw.Show();
            this.Close();
        }
    }
}
