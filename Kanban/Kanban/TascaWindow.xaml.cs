using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kanban
{
    public partial class TascaWindow : Window
    {
        public string Titol { get; private set; }
        public string Descripcio { get; private set; }
        public int Prioritat { get; private set; }
        public DateTime? DataVenciment { get; private set; }
        public string Responsable { get; private set; }
        public string Notes { get; private set; }

        // Constructor que rep la llista de participants
        public TascaWindow(List<string> participants)
        {
            InitializeComponent();

            cmbParticipants.ItemsSource = participants;
        }

        // Opcional: constructor buit per al dissenyador de WPF
        public TascaWindow() : this(new List<string>()) { }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            Titol = txtTitol.Text;
            Descripcio = txtDescripcio.Text;
            Prioritat = cmbPrioritat.SelectedIndex + 1;
            DataVenciment = dpDataVenciment.SelectedDate;
            Responsable = cmbParticipants.SelectedItem?.ToString();
            Notes = txtNotes.Text;

            this.DialogResult = true;
            this.Close();
        }
    }
}
