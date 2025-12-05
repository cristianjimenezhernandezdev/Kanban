using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            // Inicialitzar columnes
            Backlog = new List<Tasques>();
            Todo = new List<Tasques>();
            Doing = new List<Tasques>();
            Done = new List<Tasques>();

            Participants = new List<string>()
            {
                "Cistian",
                "Amine"
               
            };

            cmbSprintMaster.ItemsSource = Participants;
            cmbSprintMaster.SelectedIndex = 0; // Sprint master inicial

            // Dades inicials
            Backlog.Add(new Tasques()
            {
                Titol = "Crear mockups UI",
                Estat = "Backlog",
                Prioritat = 1,
                DataCreacio = DateTime.Now
            });

            Todo.Add(new Tasques()
            {
                Titol = "Configurar base de dades",
                Estat = "ToDo",
                Prioritat = 2,
                DataCreacio = DateTime.Now
            });

            Doing.Add(new Tasques()
            {
                Titol = "Implementar autenticació",
                Estat = "Doing",
                Prioritat = 1,
                DataCreacio = DateTime.Now
            });

            Done.Add(new Tasques()
            {
                Titol = "Reunió inicial del projecte",
                Estat = "Done",
                Prioritat = 3,
                DataCreacio = DateTime.Now.AddDays(-2)
            });

            // Enllaçar amb ListBox
            listBacklog.ItemsSource = Backlog;
            listTodo.ItemsSource = Todo;
            listDoing.ItemsSource = Doing;
            listDone.ItemsSource = Done;

            // Afegir participants al header
            CarregarParticipants();
        }

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
        // BOTÓ INFO
        // ─────────────────────────────────────────────
        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Aplicació Kanban creada per Cistian el SCRUM MASTER i Amine el SCRUM MANDADO.\nVersió 1.0\nGestiona tasques i projectes de forma visual.",
                "Informació",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }


        // ─────────────────────────────────────────────
        // PARTICIPANTS EN EL HEADER
        // ─────────────────────────────────────────────
        private void CarregarParticipants()
        {
            panelParticipants.Children.Clear();

            foreach (var p in Participants)
            {
                panelParticipants.Children.Add(
                    CrearEtiquetaParticipant(p, "#2196F3", 0)
                );
            }
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


        private void BtnAddParticipant_Click(object sender, RoutedEventArgs e)
        {
            Participants.Add("Julia");
            CarregarParticipants();
        }

        private void cmbSprintMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSprintMaster.SelectedItem != null)
            {
                string seleccionat = cmbSprintMaster.SelectedItem.ToString();
                // Pots fer el que vulguis, com guardar-ho a BBDD
                MessageBox.Show("Nou Sprint Master: " + seleccionat);
            }
        }


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

        private void btnObrirProjecte_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
