using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kanban.Programs.cs;

namespace Kanban
{
    public partial class MainWindow : Window
    {
        #region Propietats i camps

        public List<Tasques> Backlog { get; set; }
        public List<Tasques> Todo { get; set; }
        public List<Tasques> Doing { get; set; }
        public List<Tasques> Done { get; set; }
        public List<string> Participants { get; set; }

        private Tasques _draggedTask;
        private ListBox _sourceListBox;
        private readonly ProjectesService _projectesService;
        private readonly ParticipantsService _participantsService;
        private readonly ConsultesTasquesService _tasquesService;
        private int _projecteActiuId;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            _projectesService = new ProjectesService();
            _participantsService = new ParticipantsService();
            _tasquesService = new ConsultesTasquesService();
            _projecteActiuId = 0;
            InicialitzarLlistes();
            CarregarDadesInicials();
        }

        private void InicialitzarLlistes()
        {
            Backlog = new List<Tasques>();
            Todo = new List<Tasques>();
            Doing = new List<Tasques>();
            Done = new List<Tasques>();

            listBacklog.ItemsSource = Backlog;
            listTodo.ItemsSource = Todo;
            listDoing.ItemsSource = Doing;
            listDone.ItemsSource = Done;
        }

        private void CarregarDadesInicials()
        {
            CarregarParticipantsBD();
            CarregarProjecteActiu();
            CarregarTasquesProjecteActiu();
        }

        #endregion

        #region Carregar dades

        private void CarregarParticipantsBD()
        {
            Participants = _participantsService.CarregarParticipants(DataBase.grupActiu);

            cmbParticipants.Items.Clear();
            cmbSprintMaster.Items.Clear();

            foreach (var nom in Participants)
            {
                cmbParticipants.Items.Add(nom);
                cmbSprintMaster.Items.Add(nom);
            }
        }

        private void CarregarProjecteActiu()
        {
            var titol = _projectesService.ObtenirTitolProjecteActiu(DataBase.grupActiu);
            var data = _projectesService.ObtenirDataProjecteActiu(DataBase.grupActiu);
            if (titol != null)
                txtSprintName.Text = $"{titol} {data}";
            


        }


        private void CarregarTasquesProjecteActiu()
        {
            NetejarColumnes();

            var idProjecte = _projectesService.ObtenirProjecteActiuId(DataBase.grupActiu);
            _projecteActiuId = idProjecte;
            
            if (idProjecte > 0)
            {
                // Carregar els participants vinculats al projecte actiu
                CarregarParticipantsProjecte(idProjecte);
                CarregarTasquesProjecteSeleccionat(idProjecte);
            }

            RefrescarColumnes();
        }

        private void CarregarTasquesProjecteSeleccionat(int idProjecte)
        {
            var tasques = _tasquesService.CarregarTasquesProjecte(idProjecte);

            foreach (var tasca in tasques)
            {
                AfegirTascaAColumna(tasca);
            }

            OrdenarTotesLesColumnes();
        }

        #endregion

        #region Gestió de tasques

        private void btnAddBacklog_Click(object sender, RoutedEventArgs e)
        {
            var participantsProjecte = ObtenirParticipantsProjecte();
            if (participantsProjecte.Count == 0)
            {
                MessageBox.Show("Has d'afegir participants al projecte abans de crear tasques.");
                return;
            }

            var w = new TascaWindow(participantsProjecte, null, 1);
            if (w.ShowDialog() == true && w.TascaResultant != null)
            {
                Backlog.Add(w.TascaResultant);
                _tasquesService.OrdenarLlista(Backlog);
                listBacklog.Items.Refresh();
            }
        }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            var tasca = listBox?.SelectedItem as Tasques;
            if (tasca == null) return;

            var participantsProjecte = ObtenirParticipantsProjecte();
            var dialog = new TascaWindow(participantsProjecte, tasca, tasca.IdColumna);

            if (dialog.ShowDialog() == true)
            {
                _tasquesService.OrdenarLlista(GetLlistaPerColumna(tasca.IdColumna));
                RefrescarColumnes();
            }
        }

        #endregion

        #region Gestió de projectes

        private void btnCrearProjecte_Click(object sender, RoutedEventArgs e)
        {
            var projecteWindow = new CrearProjecteWindow();
            if (projecteWindow.ShowDialog() == true)
            {
                txtSprintName.Text = projecteWindow.TitolProjecteCreat;
                panelParticipants.Children.Clear();
                NetejarColumnes();
                RefrescarColumnes();
                CarregarTasquesProjecteActiu();
            }
        }

        private void btnObrirProjecte_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new ObrirProjecte();
            if (wnd.ShowDialog() == true)
            {
                txtSprintName.Text = wnd.TitolProjecteSeleccionat;
                ActualitzarSprintMasterUI(wnd.IdResponsableSeleccionat);
                
                _projecteActiuId = wnd.IdProjecteSeleccionat;
                // Carregar els participants vinculats al projecte seleccionat
                CarregarParticipantsProjecte(_projecteActiuId);
                
                NetejarColumnes();
                CarregarTasquesProjecteSeleccionat(_projecteActiuId);
                RefrescarColumnes();
            }
        }

        // Carrega els participants d'un projecte específic al panell
        private void CarregarParticipantsProjecte(int idProjecte)
        {
            panelParticipants.Children.Clear();
            
            var participantsProjecte = _participantsService.CarregarParticipantsProjecte(idProjecte);
            
            // Debug: mostrar quants participants s'han carregat
            System.Diagnostics.Debug.WriteLine($"Participants carregats per projecte {idProjecte}: {participantsProjecte.Count}");
            
            foreach (var nom in participantsProjecte)
            {
                System.Diagnostics.Debug.WriteLine($"  - {nom}");
                panelParticipants.Children.Add(CrearEtiquetaParticipant(nom, "#2196F3", 0));
            }
        }

        private void ActualitzarSprintMasterUI(int? idResponsable)
        {
            cmbSprintMaster.SelectedItem = null;
            if (!idResponsable.HasValue) return;

            var nom = _projectesService.ObtenirNomResponsable(idResponsable);
            if (nom != null && cmbSprintMaster.Items.Contains(nom))
                cmbSprintMaster.SelectedItem = nom;
        }

        #endregion

        #region Gestió de participants

        private void BtnAddParticipant_Click(object sender, RoutedEventArgs e)
        {
            var apw = new AfegirParticipantsWindow();
            if (apw.ShowDialog() == true)
                CarregarParticipantsBD();
        }

        private void cmbParticipants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbParticipants.SelectedItem == null) return;

            var nom = cmbParticipants.SelectedItem.ToString();
            if (ParticipantJaAfegit(nom)) return;

            var projecteId = _projecteActiuId > 0 ? _projecteActiuId : _projectesService.ObtenirProjecteActiuId(DataBase.grupActiu);
            _participantsService.AfegirParticipantAProjecte(nom, DataBase.grupActiu, projecteId);
            panelParticipants.Children.Add(CrearEtiquetaParticipant(nom, "#2196F3", 0));
        }

        private bool ParticipantJaAfegit(string nom)
        {
            foreach (Border b in panelParticipants.Children)
            {
                var tb = b.Child as TextBlock;
                if (tb != null && tb.Text.Contains(nom))
                    return true;
            }
            return false;
        }

        private void cmbSprintMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSprintMaster.SelectedItem == null) return;

            var nomUsuari = cmbSprintMaster.SelectedItem.ToString();
            _projectesService.ActualitzarSprintMaster(nomUsuari, DataBase.grupActiu);
        }

        private void BtnDesvincularParticipant_Click(object sender, RoutedEventArgs e)
        {
            var participantsProjecte = ObtenirParticipantsProjecte();
            if (participantsProjecte.Count == 0)
            {
                MessageBox.Show("No hi ha participants al projecte per desvincular.");
                return;
            }

            var wnd = new DesvincularParticipantWindow(participantsProjecte);
            if (wnd.ShowDialog() == true && !string.IsNullOrEmpty(wnd.ParticipantSeleccionat))
            {
                var projecteId = _projecteActiuId > 0 ? _projecteActiuId : _projectesService.ObtenirProjecteActiuId(DataBase.grupActiu);
                _participantsService.DesvincularParticipant(wnd.ParticipantSeleccionat, DataBase.grupActiu, projecteId);
                TreureParticipantDelPanell(wnd.ParticipantSeleccionat);
                MessageBox.Show($"S'ha desvinculat '{wnd.ParticipantSeleccionat}' del projecte.");
            }
        }

        private void BtnEliminarUsuari_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new EliminarUsuariWindow();
            if (wnd.ShowDialog() != true || string.IsNullOrEmpty(wnd.UsuariSeleccionat)) return;

            var nom = wnd.UsuariSeleccionat;
            var result = MessageBox.Show(
                $"Estàs segur que vols eliminar l'usuari '{nom}' de la base de dades?\nAquesta acció és irreversible.",
                "Confirmar eliminació", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _participantsService.EliminarUsuari(nom, DataBase.grupActiu);
                    CarregarParticipantsBD();
                    TreureParticipantDelPanell(nom);
                    MessageBox.Show($"S'ha eliminat l'usuari '{nom}' de la base de dades.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar l'usuari: " + ex.Message);
                }
            }
        }

        private void TreureParticipantDelPanell(string nom)
        {
            Border toRemove = null;
            foreach (Border b in panelParticipants.Children)
            {
                var tb = b.Child as TextBlock;
                if (tb != null && tb.Text.StartsWith(nom))
                {
                    toRemove = b;
                    break;
                }
            }
            if (toRemove != null)
                panelParticipants.Children.Remove(toRemove);
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

        #endregion

        #region Drag & Drop

        private void ListBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;

            var listBox = sender as ListBox;
            var tasca = listBox?.SelectedItem as Tasques;
            if (tasca == null) return;

            _draggedTask = tasca;
            _sourceListBox = listBox;

            DragDrop.DoDragDrop(listBox, new DataObject("Tasca", tasca), DragDropEffects.Move);

            _draggedTask = null;
            _sourceListBox = null;
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent("Tasca") ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Tasca")) return;

            var tasca = e.Data.GetData("Tasca") as Tasques;
            var targetListBox = sender as ListBox;

            if (tasca == null || targetListBox == null) return;
            if (_sourceListBox == null || _sourceListBox == targetListBox) return;

            TreureTascaDeLlistaOrigen(tasca);
            AfegirTascaALlistaDestí(tasca, targetListBox);

            _tasquesService.ActualitzarColumnaTasca(tasca);

            OrdenarTotesLesColumnes();
            RefrescarColumnes();
        }

        private void TreureTascaDeLlistaOrigen(Tasques tasca)
        {
            GetLlistaPerListBox(_sourceListBox)?.Remove(tasca);
        }

        private void AfegirTascaALlistaDestí(Tasques tasca, ListBox targetListBox)
        {
            var novaColumna = GetColumnaPerListBox(targetListBox);
            tasca.IdColumna = novaColumna;
            tasca.Estat = ConsultesTasquesService.GetEstatPerColumna(novaColumna);
            GetLlistaPerListBox(targetListBox)?.Add(tasca);
        }

        #endregion

        #region Utilitats columnes

        private void NetejarColumnes()
        {
            Backlog.Clear();
            Todo.Clear();
            Doing.Clear();
            Done.Clear();
        }

        private void RefrescarColumnes()
        {
            listBacklog.Items.Refresh();
            listTodo.Items.Refresh();
            listDoing.Items.Refresh();
            listDone.Items.Refresh();
        }

        private void OrdenarTotesLesColumnes()
        {
            _tasquesService.OrdenarLlista(Backlog);
            _tasquesService.OrdenarLlista(Todo);
            _tasquesService.OrdenarLlista(Doing);
            _tasquesService.OrdenarLlista(Done);
        }

        private void AfegirTascaAColumna(Tasques tasca)
        {
            GetLlistaPerColumna(tasca.IdColumna)?.Add(tasca);
        }

        private List<Tasques> GetLlistaPerColumna(byte idColumna)
        {
            switch (idColumna)
            {
                case 1: return Backlog;
                case 2: return Todo;
                case 3: return Doing;
                case 4: return Done;
                default: return null;
            }
        }

        private List<Tasques> GetLlistaPerListBox(ListBox listBox)
        {
            if (listBox == listBacklog) return Backlog;
            if (listBox == listTodo) return Todo;
            if (listBox == listDoing) return Doing;
            if (listBox == listDone) return Done;
            return null;
        }

        private byte GetColumnaPerListBox(ListBox listBox)
        {
            if (listBox == listBacklog) return 1;
            if (listBox == listTodo) return 2;
            if (listBox == listDoing) return 3;
            if (listBox == listDone) return 4;
            return 0;
        }

        private List<string> ObtenirParticipantsProjecte()
        {
            var participants = new List<string>();
            foreach (Border b in panelParticipants.Children)
            {
                var tb = b.Child as TextBlock;
                if (tb != null)
                {
                    var text = tb.Text;
                    var spaceIndex = text.LastIndexOf("  ");
                    participants.Add(spaceIndex > 0 ? text.Substring(0, spaceIndex) : text.Trim());
                }
            }
            return participants;
        }

        #endregion

        #region Altres botons

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Aplicació Kanban creada per Cristian i Amine.\nVersió 1.0\n\n" +
                "1. Afegeix participants al projecte des del desplegable.\n" +
                "2. Només els participants afegits poden ser assignats com a responsables.\n" +
                "3. Crea tasques amb el botó 'Afegir Tasca'.\n" +
                "4. Assigna prioritat (Alta, Mitja, Baixa) i responsable.\n" +
                "5. Arrossega les tasques entre columnes.\n" +
                "6. Fes doble clic sobre una tasca per editar-la.",
                "Informació", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
