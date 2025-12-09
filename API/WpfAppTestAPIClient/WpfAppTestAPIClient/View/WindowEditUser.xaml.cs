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
    /// Interaction logic for WindowEditUser.xaml
    /// </summary>
    public partial class WindowEditUser : Window
    {
        UsersApiClient api;
        User oUser;
        WindowDataGrid mainWindow;

        public WindowEditUser()
        {
            InitializeComponent();
            api = new UsersApiClient();
        }

        public WindowEditUser(User user, WindowDataGrid window)
        {
            InitializeComponent();
            api = new UsersApiClient();

            if (user == null)
            {
                btnUpdate.Content = "Afegir";

                btnUpdate.Click -= btnUpdate_Click;
                btnUpdate.Click += btnAdd_Click;
                mainWindow = window;
            }
            oUser = user;

            //Li estem dient amb que farà l'enllaç (quan fem el binding en el disseny) 
            this.DataContext = user;
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await api.UpdateAsync(oUser);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                oUser = new User();
                oUser.Name = Name.Text;
                oUser.LastName = LastName.Text;
                oUser.Birthday = BirthDate.SelectedDate.Value;
                await api.AddAsync(oUser);

                mainWindow.refresh();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
