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

namespace WpfAppTestAPIClient.View
{
    /// <summary>
    /// Interaction logic for WindowListBox.xaml
    /// </summary>
    public partial class WindowListBox : Window
    {
        UsersApiClient api;

        public WindowListBox()
        {
            InitializeComponent();
            api = new UsersApiClient();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            //Enllacem el control visual amb les dades
            listViewUsers.ItemsSource = await api.GetUsersAsync();

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }


        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            //Agafem les dades del item seleccionat
            var item = ((ListBoxItem)listViewUsers.ContainerFromElement((Button)sender)).Content;

            //Li passem l'usuari seleccionat al formulari Edit
            User user = (User)item;
            WindowEditUser w = new WindowEditUser(user, null);
            w.ShowDialog();
        }

       
    }
}
