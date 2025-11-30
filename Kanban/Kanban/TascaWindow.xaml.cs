using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kanban
{
    /// <summary>
    /// Lógica de interacción para TascaWindow.xaml
    /// </summary>
    public partial class TascaWindow : Window
    {
        public string Titol { get; private set; }
        public string Descripcio { get; private set; }
        public int Prioritat { get; private set; }

        public DateTime? DataVenciment { get; private set; }
        public string Responsable { get; private set; }
        public string Notes { get; private set; }

        public TascaWindow()
        {
            InitializeComponent();
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {

            Titol = txtTitol.Text;
            Descripcio = txtDescripcio.Text;

            //if (cmbPrioritat.SelectedItem is ComboBoxItem item)
            //    Prioritat = int.Parse(item.Content.ToString());

            Prioritat = cmbPrioritat.SelectedIndex + 1; // Assume 1-based priority
            DataVenciment = dpDataVenciment.SelectedDate;

            this.DialogResult = true;
            this.Close();
        }

        private void txtDescripcio_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
