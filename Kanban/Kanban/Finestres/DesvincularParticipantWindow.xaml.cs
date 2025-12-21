using System.Collections.Generic;
using System.Windows;

namespace Kanban
{
    // Finestra simple per seleccionar un participant del projecte i desvincular-lo.
    // No fa canvis a la BDD directament: només retorna el nom seleccionat.
    // El MainWindow el crida per fer el DELETE a Usuaris_projectes.
    public partial class DesvincularParticipantWindow : Window
    {
        // Nom del participant seleccionat.
        public string ParticipantSeleccionat { get; private set; }

        public DesvincularParticipantWindow(List<string> participants)
        {
            InitializeComponent();

            // Omplim el ComboBox amb la llista de participants del projecte.
            cmbParticipants.ItemsSource = participants;
        }

        private void BtnDesvincular_Click(object sender, RoutedEventArgs e)
        {
            // Validació: cal seleccionar un participant.
            if (cmbParticipants.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un participant.");
                return;
            }

            // Guardem el nom seleccionat i tanquem amb OK.
            ParticipantSeleccionat = cmbParticipants.SelectedItem.ToString();
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Tanquem sense acció.
            DialogResult = false;
            Close();
        }
    }
}
