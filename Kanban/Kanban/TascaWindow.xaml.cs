using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;

namespace Kanban
{
    public partial class TascaWindow : Window
    {
    
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
            cmbPrioritat.ItemsSource = new[] { "1", "2", "3" };
        }

        // Opcional: constructor buit per al dissenyador de WPF
        public TascaWindow() : this(new List<string>()) { }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
          
            Descripcio = txtDescripcio.Text;
            Prioritat = cmbPrioritat.SelectedIndex + 1;
            DataVenciment = dpDataVenciment.SelectedDate;
            Responsable = cmbParticipants.SelectedItem?.ToString();
            Notes = txtNotes.Text;

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                string query = @"INSERT INTO Usuaris (Nom, Cognom, IdGrup)
                                 VALUES (@nom, @cognom, @idGrup)";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nom", Descripcio);
                cmd.Parameters.AddWithValue("@cognom", Prioritat);
                //cmd.
                //cmd.Parameters.AddWithValue("@idGrup", LoginWindow.grupActiu);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Participant afegit correctament.");
            this.DialogResult = true;
            this.Close();
        }
    }
}
