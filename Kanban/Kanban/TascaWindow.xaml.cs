using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kanban
{
    public partial class TascaWindow : Window
    {
        public string Descripcio { get; private set; }
        public int Prioritat { get; private set; }
        public DateTime? DataVenciment { get; private set; }
        public string Responsable { get; private set; }
        public string Notes { get; private set; }

        private readonly bool _isEditMode;

        // Constructor que rep la llista de participants
        public TascaWindow(List<string> participants, MainWindow.Tasques tasca = null)
        {
            InitializeComponent();
            cmbParticipants.ItemsSource = participants;
            cmbPrioritat.ItemsSource = new[] { "Alta", "Mitja", "Baixa" };

            if (tasca != null)
            {
                _isEditMode = true;
                txtDescripcio.Text = tasca.Descripcio;
                
                // Convertir prioritat numèrica a índex del combo
                int indexPrioritat = Math.Max(0, tasca.Prioritat - 1);
                if (indexPrioritat < 3)
                    cmbPrioritat.SelectedIndex = indexPrioritat;

                if (!string.IsNullOrEmpty(tasca.Responsable))
                    cmbParticipants.SelectedItem = tasca.Responsable;

                dpDataVenciment.SelectedDate =
                    tasca.DataVenciment == DateTime.MinValue ? (DateTime?)null : tasca.DataVenciment;
                txtNotes.Text = tasca.Notes;
            }
        }

        // Opcional: constructor buit per al dissenyador de WPF
        public TascaWindow() : this(new List<string>()) { }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPrioritat.SelectedIndex < 0)
            {
                MessageBox.Show("Selecciona una prioritat.");
                return;
            }

            Descripcio = txtDescripcio.Text;
            // Convertir índex a valor numèric: 0->1 (Alta), 1->2 (Mitja), 2->3 (Baixa)
            Prioritat = cmbPrioritat.SelectedIndex + 1;
            DataVenciment = dpDataVenciment.SelectedDate;
            Responsable = cmbParticipants.SelectedItem?.ToString();
            Notes = txtNotes.Text;

            if (string.IsNullOrWhiteSpace(Descripcio))
            {
                MessageBox.Show("Introdueix una descripció.");
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
