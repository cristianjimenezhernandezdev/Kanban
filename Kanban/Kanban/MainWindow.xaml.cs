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
            Backlog = new List<Tasques>();
            Todo = new List<Tasques>();
            Doing = new List<Tasques>();
            Done = new List<Tasques>();

            listBacklog.ItemsSource = Backlog;
            listTodo.ItemsSource = Todo;
            listDoing.ItemsSource = Doing;
            listDone.ItemsSource = Done;

            CarregarParticipantsBD();
            CarregarProjecteActiu();
            CarregarTasquesProjecteActiu();
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
            public int IdTasca { get; set; }
            public int IdProjecte { get; set; }
            public byte IdColumna { get; set; }

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
                Tasques nova = new Tasques
                {
                    Titol = w.Descripcio,
                    Descripcio = w.Descripcio,
                    Estat = "Backlog",
                    Responsable = w.Responsable,
                    Prioritat = w.Prioritat,
                    DataVenciment = w.DataVenciment ?? DateTime.MinValue,
                    Notes = w.Notes,
                    DataCreacio = DateTime.Now,
                    IdColumna = 1
                };

                try
                {
                    nova.IdTasca = InserirTascaBD(nova);
                    Backlog.Add(nova);
                    listBacklog.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar la tasca: " + ex.Message);
                }
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
                Tasques nova = new Tasques
                {
                    Titol = w.Descripcio,
                    Descripcio = w.Descripcio,
                    Estat = "ToDo",
                    Responsable = w.Responsable,
                    Prioritat = w.Prioritat,
                    DataVenciment = w.DataVenciment ?? DateTime.MinValue,
                    Notes = w.Notes,
                    DataCreacio = DateTime.Now,
                    IdColumna = 2
                };

                try
                {
                    nova.IdTasca = InserirTascaBD(nova);
                    Todo.Add(nova);
                    listTodo.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar la tasca: " + ex.Message);
                }
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
                CarregarTasquesProjecteActiu();
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
            ObrirProjecte obrirProjecteWind = new ObrirProjecte();

            if (obrirProjecteWind.ShowDialog() == true)
            {
                // 1️⃣ Mostrar el títol del projecte
                txtSprintName.Text = obrirProjecteWind.TitolProjecteSeleccionat;

                // 2️⃣ Si té responsable, seleccionar-lo al combo
                if (obrirProjecteWind.IdResponsableSeleccionat.HasValue)
                {
                    using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
                    {
                        conn.Open();

                        string sql = @"SELECT Nom 
                               FROM Usuaris 
                               WHERE IdUsuari = @id";

                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", obrirProjecteWind.IdResponsableSeleccionat.Value);

                        object nom = cmd.ExecuteScalar();
                        if (nom != null)
                            cmbSprintMaster.SelectedItem = nom.ToString();
                    }
                }
            }
        }

        // ─────────────────────────────────────────────
        // COMBO PARTICIPANTS
        // ─────────────────────────────────────────────
        private void cmbParticipants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbParticipants.SelectedItem == null)
                return;

            string nom = cmbParticipants.SelectedItem.ToString();

            // Evitar duplicats visuals
            foreach (Border b in panelParticipants.Children)
            {
                if (((TextBlock)b.Child).Text.Contains(nom))
                    return;
            }

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                // Obtenir IdUsuari
                string sqlUsuari = @"SELECT IdUsuari 
                             FROM Usuaris 
                             WHERE Nom = @nom AND IdGrup = @grup";

                MySqlCommand cmdUsuari = new MySqlCommand(sqlUsuari, conn);
                cmdUsuari.Parameters.AddWithValue("@nom", nom);
                cmdUsuari.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                object resultUsuari = cmdUsuari.ExecuteScalar();
                if (resultUsuari == null)
                    return;

                int idUsuari = Convert.ToInt32(resultUsuari);

                // Obtenir IdProjecte actiu (últim del grup)
                string sqlProjecte = @"SELECT IdProjecte 
                               FROM Projectes 
                               WHERE IdGrup = @grup 
                               ORDER BY IdProjecte DESC 
                               LIMIT 1";

                MySqlCommand cmdProjecte = new MySqlCommand(sqlProjecte, conn);
                cmdProjecte.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                object resultProjecte = cmdProjecte.ExecuteScalar();
                if (resultProjecte == null)
                    return;

                int idProjecte = Convert.ToInt32(resultProjecte);

                // Insertar relació Projecte i Usuari (si no existeix)
                string sqlInsert = @"INSERT IGNORE INTO Usuaris_projectes (IdProjecte, IdUsuari)
                             VALUES (@idProjecte, @idUsuari)";

                MySqlCommand cmdInsert = new MySqlCommand(sqlInsert, conn);
                cmdInsert.Parameters.AddWithValue("@idProjecte", idProjecte);
                cmdInsert.Parameters.AddWithValue("@idUsuari", idUsuari);

                cmdInsert.ExecuteNonQuery();
            }

            // Afegir visualment al MainWindow
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

            // 2. Afegir a la llista destí i actualitzar Estat / IdColumna
            if (targetListBox == listBacklog)
            {
                tasca.Estat = "Backlog";
                tasca.IdColumna = 1;
                Backlog.Add(tasca);
            }
            else if (targetListBox == listTodo)
            {
                tasca.Estat = "ToDo";
                tasca.IdColumna = 2;
                Todo.Add(tasca);
            }
            else if (targetListBox == listDoing)
            {
                tasca.Estat = "Doing";
                tasca.IdColumna = 3;
                Doing.Add(tasca);
            }
            else if (targetListBox == listDone)
            {
                tasca.Estat = "Done";
                tasca.IdColumna = 4;
                Done.Add(tasca);
            }

            try
            {
                ActualitzarColumnaTascaBD(tasca);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualitzar la columna: " + ex.Message);
            }

            // 3. Refrescar totes les columnes
            listBacklog.Items.Refresh();
            listTodo.Items.Refresh();
            listDoing.Items.Refresh();
            listDone.Items.Refresh();
        }

        private void CarregarTasquesProjecteActiu()
        {
            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();
                int? idProjecte = ObtenirProjecteActiuId(conn);
                Backlog.Clear();
                Todo.Clear();
                Doing.Clear();
                Done.Clear();

                if (!idProjecte.HasValue)
                {
                    listBacklog.Items.Refresh();
                    listTodo.Items.Refresh();
                    listDoing.Items.Refresh();
                    listDone.Items.Refresh();
                    return;
                }

                const string sql = @"SELECT t.IdTasca,
                                               t.IdProjecte,
                                               t.IdColumna,
                                               t.IdUsuariResponsable,
                                               t.Descripcio,
                                               t.Prioritat,
                                               t.DataCreacio,
                                               t.DataVenciment,
                                               u.Nom AS NomResponsable
                                        FROM Tasca t
                                        LEFT JOIN Usuaris u ON u.IdUsuari = t.IdUsuariResponsable
                                        WHERE t.IdProjecte = @idProjecte";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idProjecte", idProjecte.Value);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tasques tasca = new Tasques
                            {
                                IdTasca = Convert.ToInt32(reader["IdTasca"]),
                                IdProjecte = Convert.ToInt32(reader["IdProjecte"]),
                                IdColumna = Convert.ToByte(reader["IdColumna"]),
                                Descripcio = reader["Descripcio"].ToString(),
                                Titol = reader["Descripcio"].ToString(),
                                Responsable = reader["NomResponsable"] == DBNull.Value
                                    ? null
                                    : reader["NomResponsable"].ToString(),
                                Prioritat = reader["Prioritat"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(reader["Prioritat"]),
                                DataCreacio = reader["DataCreacio"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(reader["DataCreacio"]),
                                DataVenciment = reader["DataVenciment"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(reader["DataVenciment"])
                            };

                            switch (tasca.IdColumna)
                            {
                                case 1:
                                    tasca.Estat = "Backlog";
                                    Backlog.Add(tasca);
                                    break;
                                case 2:
                                    tasca.Estat = "ToDo";
                                    Todo.Add(tasca);
                                    break;
                                case 3:
                                    tasca.Estat = "Doing";
                                    Doing.Add(tasca);
                                    break;
                                case 4:
                                    tasca.Estat = "Done";
                                    Done.Add(tasca);
                                    break;
                            }
                        }
                    }
                }

                listBacklog.Items.Refresh();
                listTodo.Items.Refresh();
                listDoing.Items.Refresh();
                listDone.Items.Refresh();
            }
        }

        private int InserirTascaBD(Tasques tasca)
        {
            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                int? idProjecte = ObtenirProjecteActiuId(conn);
                if (!idProjecte.HasValue)
                {
                    throw new InvalidOperationException("No hi ha cap projecte actiu per al grup actual.");
                }

                tasca.IdProjecte = idProjecte.Value;
                int? idUsuariResponsable = null;
                if (!string.IsNullOrEmpty(tasca.Responsable))
                {
                    idUsuariResponsable = ObtenirIdUsuariPerNom(conn, tasca.Responsable);
                }

                const string sql = @"INSERT INTO Tasca
                                        (IdProjecte, IdColumna, IdUsuariResponsable, Descripcio, Prioritat, DataCreacio, DataVenciment)
                                    VALUES
                                        (@idProjecte, @idColumna, @idUsuariResponsable, @descripcio, @prioritat, @dataCreacio, @dataVenciment);
                                    SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idProjecte", tasca.IdProjecte);
                    cmd.Parameters.AddWithValue("@idColumna", tasca.IdColumna);
                    cmd.Parameters.AddWithValue("@idUsuariResponsable",
                        (object)idUsuariResponsable ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@descripcio", tasca.Descripcio);
                    cmd.Parameters.AddWithValue("@prioritat", tasca.Prioritat);
                    cmd.Parameters.AddWithValue("@dataCreacio", tasca.DataCreacio);
                    cmd.Parameters.AddWithValue("@dataVenciment",
                        tasca.DataVenciment == DateTime.MinValue
                            ? (object)DBNull.Value
                            : tasca.DataVenciment);

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        private void ActualitzarColumnaTascaBD(Tasques tasca)
        {
            if (tasca.IdTasca <= 0)
            {
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(Database.connectionString))
            {
                conn.Open();

                const string sql = @"UPDATE Tasca
                             SET IdColumna = @idColumna
                             WHERE IdTasca = @idTasca";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idColumna", tasca.IdColumna);
                    cmd.Parameters.AddWithValue("@idTasca", tasca.IdTasca);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int? ObtenirProjecteActiuId(MySqlConnection conn)
        {
            const string sqlProjecte = @"SELECT IdProjecte 
                                   FROM Projectes 
                                   WHERE IdGrup = @grup 
                                   ORDER BY IdProjecte DESC 
                                   LIMIT 1";

            using (MySqlCommand cmdProjecte = new MySqlCommand(sqlProjecte, conn))
            {
                cmdProjecte.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);
                object result = cmdProjecte.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt32(result);
            }
        }

        private int? ObtenirIdUsuariPerNom(MySqlConnection conn, string nomUsuari)
        {
            const string sql = @"SELECT IdUsuari 
                         FROM Usuaris 
                         WHERE Nom = @nom AND IdGrup = @grup";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@nom", nomUsuari);
                cmd.Parameters.AddWithValue("@grup", LoginWindow.grupActiu);

                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt32(result);
            }
        }
    }
}
