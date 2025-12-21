using System;
using System.Windows;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    // Finestra de login.
    // L'usuari introdueix nom de grup i codi (contrasenya).
    // Si la BDD valida l'accés, es guarda DataBase.grupActiu i s'obre el MainWindow.
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Tancar la finestra de login quan clico cancelar.
            // Si aquesta és l'única finestra oberta, l'aplicació es tancarà.
            this.Close();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Llegim els camps del formulari.
            string usuari = txtUsuari.Text;
            string contrasenya = txtContrasenya.Password;

            using (var conn = DataBase.ObtenirConnexio())
            {
                // 1) Comprovar si l'usuari i contrasenya existeixen a la taula Grups.
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

                // 2) Un cop validat, obtenir l'IdGrup del grup i guardar-lo com a grup actiu.
                // Aquest valor es fa servir després a la resta de finestres i serveis.
                const string queryGrup = "SELECT IdGrup FROM Grups WHERE Nom=@user AND Codi=@pass";
                using (var cmdGrup = new MySqlCommand(queryGrup, conn))
                {
                    cmdGrup.Parameters.AddWithValue("@user", usuari);
                    cmdGrup.Parameters.AddWithValue("@pass", contrasenya);

                    DataBase.grupActiu = Convert.ToInt32(cmdGrup.ExecuteScalar());
                }
            }

            // 3) Obrir MainWindow.
            // Assignem MainWindow com a finestra principal de l'aplicació.
            var mw = new MainWindow();
            Application.Current.MainWindow = mw;
            mw.Show();

            // 4) Tancar el login després d'haver obert el MainWindow.
            this.Close();
        }
    }
}
