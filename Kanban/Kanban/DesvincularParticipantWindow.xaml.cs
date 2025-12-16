using System.Collections.Generic;
using System.Windows;

namespace Kanban
{
    public partial class DesvincularParticipantWindow : Window
    {
        public string ParticipantSeleccionat { get; private set; }

        public DesvincularParticipantWindow(List<string> participants)
        {
            InitializeComponent();
            cmbParticipants.ItemsSource = participants;
        }

        private void BtnDesvincular_Click(object sender, RoutedEventArgs e)
        {
            if (cmbParticipants.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un participant.");
                return;
            }

            ParticipantSeleccionat = cmbParticipants.SelectedItem.ToString();
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
