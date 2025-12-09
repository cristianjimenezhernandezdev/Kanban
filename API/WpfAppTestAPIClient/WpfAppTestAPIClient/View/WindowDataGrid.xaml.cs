using System;
using System.Collections.Generic;
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
using WpfAppTestAPIClient.APIClient;
using WpfAppTestAPIClient.Model;

namespace WpfAppTestAPIClient
{
    /// <summary>
    /// Interaction logic for WindowDataGrid.xaml
    /// </summary>
    public partial class WindowDataGrid : Window
    {
        UsersApiClient api;
        public WindowDataGrid()
        {
            InitializeComponent();
            api = new UsersApiClient();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refresh();
        }

        private void EditUser(object sender, RoutedEventArgs e)
        {
            //Agafem les dades del item seleccionat
            User oUser = (User)dgUsers.SelectedItem;

            //Li passem l'usuari seleccionat al formulari Edit
            WindowEditUser w = new WindowEditUser(oUser, this);
            w.ShowDialog();
        }

        private void AddUser(object sender, RoutedEventArgs e)
        {
            //Li passem l'usuari buit(null) quan afegim un usuari
            //També li passem el formulari per poder actualitzar les dades quan inserim
            WindowEditUser w = new WindowEditUser(null, this);
            w.ShowDialog();
        }

        private async void DeleteUser(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Eliminar usuario seleccionado?", "Eliminar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    //Agafem les dades del item seleccionat
                    User oUser = (User)dgUsers.SelectedItem;

                    //Eliminen usuari
                    await api.DeleteAsync(oUser.Id);
                   
                    //Actualitzem dades del grid
                    refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public async void refresh()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            dgUsers.ItemsSource = await api.GetUsersAsync();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

    }
}
