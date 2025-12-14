using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kanban.Programs.cs;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;

namespace Kanban
{
    public partial class MainWindow : Window
    {
        // Llistes de cada columna
        public List<Tasques> Backlog { get; set; }
        public List<Tasques> Todo { get; set; }
        public List<Tasques> Doing { get; set; }
        public List<Tasques> Done { get; set; }

        // Exemple de participants
        public List<string> Participants { get; set; }

        // Camps per al drag & drop
        private Tasques _draggedTask;
        private ListBox _sourceListBox;

        public MainWindow()
        {
            InitializeComponent();
            CarregarParticipantsBD();
        }

        // ────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        // CARREGAR PARTICIPANTS DES DE LA BASE DE DADES
        private void CarregarParticipantsBD()
        {
            Participants = new List<string>();

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                string query = "SELECT Nom FROM Usuaris WHERE IdGrup = @grup";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                MySqlDataReader reader = cmd.ExecuteReader();

                cmbParticipants.Items.Clear();
                cmbSprintMaster.Items.Clear();

                while (reader.Read())
                {
                    string nom = reader["Nom"].ToString();

                    // Omplir tots dos combos amb els paricipants del grup actiu
                    cmbParticipants.Items.Add(nom);
                    cmbSprintMaster.Items.Add(nom);

                    // I afegir a la llista interna (per TascaWindow)
                    Participants.Add(nom);
                }
            }
        }

        // CARREGAR PROJECTE ACTIU AL INICIAR
        private void CarregarProjecteActiu()
        {
            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                string sql = @"SELECT Titol 
                       FROM Projectes 
                       WHERE IdGrup = @grup
                       ORDER BY IdProjecte DESC
                       LIMIT 1";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                object result = cmd.ExecuteScalar();

                if (result != null)
                    txtSprintName.Text = result.ToString();
            }
        }

        // PARTICIPANTS EN EL HEADER
        private void CarregarParticipants()
        {
            panelParticipants.Children.Clear();

            foreach (var item in cmbParticipants.Items)
            {
                string nom = item.ToString();

                panelParticipants.Children.Add(
                    CrearEtiquetaParticipant(nom, "#2196F3", 0)
                );
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        // ─────────────────────────────────────────────
        // CLASSE TASQUES
        // ─────────────────────────────────────────────
        public class Tasques
        {
            public string Titol { get; set; }
            public string Descripcio { get; set; }
            public string Estat { get; set; }
            public string Responsable { get; set; }
            public DateTime DataVenciment { get; set; }
            public int Prioritat { get; set; }
            public DateTime DataCreacio { get; set; }
            public string Notes { get; set; }

            public override string ToString() => Titol;
        }


        // ─────────────────────────────────────────────
        // BOTÓ AFEGIR TASCA (BACKLOG)
        // ─────────────────────────────────────────────
        private void btnAddBacklog_Click(object sender, RoutedEventArgs e)
        {
            TascaWindow w = new TascaWindow(Participants);

            if (w.ShowDialog() == true)
            {
                Backlog.Add(new Tasques()
                {
                    
                    Descripcio = w.Descripcio,
                    Estat = "Backlog",
                    Responsable = w.Responsable,
                    Prioritat = w.Prioritat,
                    DataVenciment = w.DataVenciment ?? DateTime.Now,
                    Notes = w.Notes,
                    DataCreacio = DateTime.Now
                });

                listBacklog.Items.Refresh();
            }
        }


        // ─────────────────────────────────────────────
        // BOTÓ AFEGIR TASCA (TODO)
        // ─────────────────────────────────────────────
        private void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            TascaWindow w = new TascaWindow(Participants);

            if (w.ShowDialog() == true)
            {
                Todo.Add(new Tasques()
                {
                   
                    Descripcio = w.Descripcio,
                    Estat = "ToDo",
                    Responsable = w.Responsable,
                    Prioritat = w.Prioritat,
                    DataVenciment = w.DataVenciment ?? DateTime.Now,
                    Notes = w.Notes,
                    DataCreacio = DateTime.Now
                });

                listTodo.Items.Refresh();
            }
        }

        // ─────────────────────────────────────────────
        // BOTÓ CREAR PROJECTE
        // ─────────────────────────────────────────────
        private void btnCrearProjecte_Click(object sender, RoutedEventArgs e)
        {
            CrearProjecteWindow projecteWindow = new CrearProjecteWindow();

            if (projecteWindow.ShowDialog() == true)
            {
                txtSprintName.Text = projecteWindow.TitolProjecteCreat;
                listBacklog.Items.Refresh();
            }
        }

        // ─────────────────────────────────────────────
        // BOTÓ INFO
        // ─────────────────────────────────────────────
        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Aplicació Kanban creada per Cistian i Amine.\nVersió 1.0\nGestiona tasques i projectes desde la base de dades.",
                "Informació",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private Border CrearEtiquetaParticipant(string nom, string colorHex, int numTasques)
        {
            return new Border
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(5),
                Padding = new Thickness(7),
                Child = new TextBlock
                {
                    Text = $"{nom}  {numTasques}",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                }
            };
        }

        // ─────────────────────────────────────────────
        // BOTÓ AFEGIR PARTICIPANT
        // ─────────────────────────────────────────────
        private void BtnAddParticipant_Click(object sender, RoutedEventArgs e)
        {
            AfegirParticipantsWindow apw = new AfegirParticipantsWindow();

            if (apw.ShowDialog() == true)
            {
                CarregarParticipantsBD();  // Actualitza els ComboBox

                panelParticipants.Children.Clear();  // Netejar panell
                CarregarParticipants();              // Tornar a mostrar participants
            }
        }

        // ─────────────────────────────────────────────
        // COMBO SPRINT MASTER
        // ─────────────────────────────────────────────
        private void cmbSprintMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSprintMaster.SelectedItem == null)
                return;

            string nomUsuari = cmbSprintMaster.SelectedItem.ToString();

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                // Obtenir IdUsuari
                string query = @"SELECT IdUsuari 
                         FROM Usuaris 
                         WHERE Nom = @nom AND IdGrup = @grup";

                MySqlCommand cmdUsuari = new MySqlCommand(query, conn);
                cmdUsuari.Parameters.AddWithValue("@nom", nomUsuari);
                cmdUsuari.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                object result = cmdUsuari.ExecuteScalar();
                if (result == null)
                    return;

                int idUsuari = Convert.ToInt32(result);

                // Actualitzar projecte
                string sqlUpdate = @"UPDATE Projectes 
                             SET IdResponsable = @idUsuari 
                             WHERE IdGrup = @grup 
                             ORDER BY IdProjecte DESC 
                             LIMIT 1";

                MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdate, conn);
                cmdUpdate.Parameters.AddWithValue("@idUsuari", idUsuari);
                cmdUpdate.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                cmdUpdate.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────
        // BOTÓ OBRIR PROJECTE
        // ─────────────────────────────────────────────
        private void btnObrirProjecte_Click(object sender, RoutedEventArgs e)
        {

        }

        // ─────────────────────────────────────────────
        // COMBO PARTICIPANTS
        // ─────────────────────────────────────────────
        private void cmbParticipants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbParticipants.SelectedItem == null)
                return;

            string nom = cmbParticipants.SelectedItem.ToString();

            foreach (Border b in panelParticipants.Children)
            {
                if (((TextBlock)b.Child).Text.Contains(nom))
                    return;
            }

            panelParticipants.Children.Add(
                CrearEtiquetaParticipant(nom, "#2196F3", 0)
            );
        }

        // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────── 
        // ─────────────────────────────────────────────
        // DRAG & DROP ENTRE COLUMNES
        // ─────────────────────────────────────────────
        private void ListBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var listBox = sender as ListBox;
                if (listBox == null) return;

                var tasca = listBox.SelectedItem as Tasques;
                if (tasca == null) return;

                _draggedTask = tasca;
                _sourceListBox = listBox;

                DragDrop.DoDragDrop(listBox,
                    new DataObject("Tasca", tasca),
                    DragDropEffects.Move);

                _draggedTask = null;
                _sourceListBox = null;
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Tasca"))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Tasca")) return;

            var tasca = e.Data.GetData("Tasca") as Tasques;
            var targetListBox = sender as ListBox;

            if (tasca == null || targetListBox == null) return;
            if (_sourceListBox == null || _sourceListBox == targetListBox) return;

            // 1. Treure de la llista origen
            if (_sourceListBox == listBacklog) Backlog.Remove(tasca);
            else if (_sourceListBox == listTodo) Todo.Remove(tasca);
            else if (_sourceListBox == listDoing) Doing.Remove(tasca);
            else if (_sourceListBox == listDone) Done.Remove(tasca);

            // 2. Afegir a la llista destí i actualitzar Estat
            if (targetListBox == listBacklog)
            {
                tasca.Estat = "Backlog";
                Backlog.Add(tasca);
            }
            else if (targetListBox == listTodo)
            {
                tasca.Estat = "ToDo";
                Todo.Add(tasca);
            }
            else if (targetListBox == listDoing)
            {
                tasca.Estat = "Doing";
                Doing.Add(tasca);
            }
            else if (targetListBox == listDone)
            {
                tasca.Estat = "Done";
                Done.Add(tasca);
            }

            // 3. Refrescar totes les columnes
            listBacklog.Items.Refresh();
            listTodo.Items.Refresh();
            listDoing.Items.Refresh();
            listDone.Items.Refresh();
        }
    }
}
