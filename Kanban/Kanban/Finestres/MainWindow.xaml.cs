using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kanban.Programs.cs;

namespace Kanban
{
    // Finestra principal del programa (tauler Kanban).
    // Responsabilitats principals:
    // - Carregar el projecte actiu del grup i mostrar títol + data
    // - Carregar participants del grup i participants del projecte
    // - Carregar tasques del projecte i repartir-les en columnes
    // - Permetre crear/editar tasques i moure-les amb drag & drop
    // - Permetre canviar Sprint Master i vincular/desvincular participants
    public partial class MainWindow : Window
    {
        #region Propietats i camps

        // Llistes que representen cada columna del Kanban.
        public List<Tasques> Backlog { get; set; }
        public List<Tasques> Todo { get; set; }
        public List<Tasques> Doing { get; set; }
        public List<Tasques> Done { get; set; }

        // Llista de noms d'usuaris del grup (per omplir desplegables).
        public List<string> Participants { get; set; }

        // Camps per suportar el drag & drop de tasques.
        private Tasques _draggedTask;
        private ListBox _sourceListBox;

        // Serveis de dades (consultes a la BDD).
        private readonly ProjectesService _projectesService;
        private readonly ParticipantsService _participantsService;
        private readonly ConsultesTasquesService _tasquesService;

        // Id del projecte que s'està visualitzant/treballant en aquest moment.
        private int _projecteActiuId;

        // bool per evitar que els events SelectionChanged s'executin quan carreguem dades des del codi.
        // (Quan assignem SelectedItem programàticament, WPF dispara l'event.) (Aqui ens fallava tot el rato i era la manera que no)
        private bool _carregantDades = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            // Inicialitzem els serveis.
            _projectesService = new ProjectesService();
            _participantsService = new ParticipantsService();
            _tasquesService = new ConsultesTasquesService();

            // Encara no hi ha projecte carregat.
            _projecteActiuId = 0;

            // Preparem llistes i lliguem ItemsSource dels ListBox.
            InicialitzarLlistes();

            // Carreguem dades inicials (participants, projecte actiu i tasques).
            CarregarDadesInicials();
        }

        // Crea les llistes i les assigna als ListBox de la UI.
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

        // ordre d'inici del MainWindow.
        // 1) Carrega participants del grup
        // 2) Mostra títol i data del projecte actiu
        // 3) Carrega tasques + participants del projecte actiu
        private void CarregarDadesInicials()
        {
            CarregarParticipantsBD();
            CarregarProjecteActiu();
            CarregarTasquesProjecteActiu();
        }

        #endregion

        #region Carregar dades

        // Carrega tots els participants del grup i omple els ComboBox.
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

        // Carrega informació bàsica del projecte actiu (títol i data fi) i ho mostra a la capçalera.
        private void CarregarProjecteActiu()
        {
            var titol = _projectesService.ObtenirTitolProjecteActiu(DataBase.grupActiu);
            var data = _projectesService.ObtenirDataProjecteActiu(DataBase.grupActiu);
            
            // Titol al TextBlock principal
            txtSprintName.Text = titol ?? string.Empty;
            
            // Data al TextBlock separat, sota el titol
            if (!string.IsNullOrEmpty(data))
            {
                txtSprintData.Text = $"Data fi: {data}";
            }
            else
            {
                txtSprintData.Text = string.Empty;
            }
        }

        // Carrega el projecte actiu del grup (últim creat), i després:
        // - actualitza Sprint Master
        // - carrega participants del projecte
        // - carrega tasques del projecte
        private void CarregarTasquesProjecteActiu()
        {
            NetejarColumnes();

            var idProjecte = _projectesService.ObtenirProjecteActiuId(DataBase.grupActiu);
            _projecteActiuId = idProjecte;

            if (idProjecte > 0)
            {
                // Actualitzar Sprint Master del projecte actiu
                var idResponsable = _projectesService.ObtenirIdResponsableProjecte(idProjecte);
                ActualitzarSprintMasterUI(idResponsable);

                // Carregar els participants vinculats al projecte actiu
                CarregarParticipantsProjecte(idProjecte);
                CarregarTasquesProjecteSeleccionat(idProjecte);
            }

            RefrescarColumnes();
        }

        // Carrega tasques d'un projecte concret i les reparteix a les columnes.
        private void CarregarTasquesProjecteSeleccionat(int idProjecte)
        {
            var tasques = _tasquesService.CarregarTasquesProjecte(idProjecte);

            foreach (var tasca in tasques)
            {
                AfegirTascaAColumna(tasca);
            }

            // Ordenem perquè dins de cada columna les tasques quedin ordenades segons prioritat/responsable/text.
            OrdenarTotesLesColumnes();
        }

        #endregion

        #region Gestió de tasques

        // Crea una tasca nova (sempre al Backlog) per al projecte obert.
        private void btnAddBacklog_Click(object sender, RoutedEventArgs e)
        {
            var participantsProjecte = ObtenirParticipantsProjecte();
            if (participantsProjecte.Count == 0)
            {
                MessageBox.Show("Has d'afegir participants al projecte abans de crear tasques.");
                return;
            }

            // Obrim la finestra de tasca en mode "crear".
            // Passem: participants del projecte, tasca null, columna 1 (Backlog), i id projecte actiu.
            var w = new TascaWindow(participantsProjecte, null, 1, _projecteActiuId);
            if (w.ShowDialog() == true && w.TascaResultant != null)
            {
                // Afegim a la llista i refresquem.
                Backlog.Add(w.TascaResultant);
                _tasquesService.OrdenarLlista(Backlog);
                listBacklog.Items.Refresh();
            }
        }

        // Doble clic sobre una tasca: obre la finestra de tasca en mode editar.
        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            var tasca = listBox?.SelectedItem as Tasques;
            if (tasca == null) return;

            var participantsProjecte = ObtenirParticipantsProjecte();

            // Obrim la finestra de tasca en mode "editar".
            var dialog = new TascaWindow(participantsProjecte, tasca, tasca.IdColumna, _projecteActiuId);

            if (dialog.ShowDialog() == true)
            {
                // Re-ordenem la columna de la tasca i refresquem.
                _tasquesService.OrdenarLlista(GetLlistaPerColumna(tasca.IdColumna));
                RefrescarColumnes();
            }
        }

        #endregion

        #region Gestió de projectes

        // Botó "Crear Sprint": obre la finestra de crear projecte i després recarrega el projecte actiu.
        private void btnCrearProjecte_Click(object sender, RoutedEventArgs e)
        {
            var projecteWindow = new CrearProjecteWindow();
            if (projecteWindow.ShowDialog() == true)
            {
                // Esborrem UI i recarreguem.
                txtSprintName.Text = projecteWindow.TitolProjecteCreat;
                panelParticipants.Children.Clear();
                NetejarColumnes();
                RefrescarColumnes();
                CarregarTasquesProjecteActiu();
            }
        }

        // Botó "Obrir Sprint": obre un selector i carrega el projecte escollit.
        private void btnObrirProjecte_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new ObrirProjecte();
            if (wnd.ShowDialog() == true)
            {
                // Actualitzem capçalera amb el títol seleccionat.
                txtSprintName.Text = wnd.TitolProjecteSeleccionat;

                // Actualitzem l'Sprint Master de la UI segons el projecte seleccionat.
                ActualitzarSprintMasterUI(wnd.IdResponsableSeleccionat);
                
                // Guardem l'id de projecte seleccionat com a projecte actiu.
                _projecteActiuId = wnd.IdProjecteSeleccionat;

                // Carregar participants vinculats a aquell projecte.
                CarregarParticipantsProjecte(_projecteActiuId);
                
                // Carregar tasques del projecte seleccionat.
                NetejarColumnes();
                CarregarTasquesProjecteSeleccionat(_projecteActiuId);
                RefrescarColumnes();
            }
        }

        // Carrega els participants d'un projecte específic i els pinta al panell.
        private void CarregarParticipantsProjecte(int idProjecte)
        {
            panelParticipants.Children.Clear();
            
            var participantsProjecte = _participantsService.CarregarParticipantsProjecte(idProjecte);
            
            // Debug: mostrar quants participants s'han carregat
            System.Diagnostics.Debug.WriteLine($"Participants carregats per projecte {idProjecte}: {participantsProjecte.Count}");
            
            foreach (var nom in participantsProjecte)
            {
                System.Diagnostics.Debug.WriteLine($"  - {nom}");

                // Afegim una "etiqueta" visual per cada participant.
                panelParticipants.Children.Add(CrearEtiquetaParticipant(nom, "#2196F3", 0));
            }
        }

        // Actualitza la UI del Sprint Master:
        // - posa el label amb el nom
        // - selecciona el nom al ComboBox (si existeix al llistat)
        private void ActualitzarSprintMasterUI(int? idResponsable)
        {
            _carregantDades = true; // Evitar que dispari l'event SelectionChanged
            try
            {
                cmbSprintMaster.SelectedItem = null;
                if (!idResponsable.HasValue)
                {
                    lblSprintMasterActual.Content = "";
                    return;
                }

                var nom = _projectesService.ObtenirNomResponsable(idResponsable);
                lblSprintMasterActual.Content = nom ?? "";

                // Seleccionem el nom al ComboBox, però això NO ha de guardar res a la BDD.
                // Per això fem servir el flag _carregantDades.
                if (nom != null && cmbSprintMaster.Items.Contains(nom))
                    cmbSprintMaster.SelectedItem = nom;
            }
            finally
            {
                _carregantDades = false;
            }
        }

        #endregion

        #region Gestió de participants

        // Obre una finestra per crear un participant i després recarrega la llista de participants.
        private void BtnAddParticipant_Click(object sender, RoutedEventArgs e)
        {
            var apw = new AfegirParticipantsWindow();
            if (apw.ShowDialog() == true)
                CarregarParticipantsBD();
        }

        // Quan es selecciona un participant al ComboBox, s'afegeix (vincula) al projecte actiu.
        private void cmbParticipants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbParticipants.SelectedItem == null) return;

            var nom = cmbParticipants.SelectedItem.ToString();
            if (ParticipantJaAfegit(nom)) return;

            var projecteId = _projecteActiuId > 0 ? _projecteActiuId : _projectesService.ObtenirProjecteActiuId(DataBase.grupActiu);
            _participantsService.AfegirParticipantAProjecte(nom, DataBase.grupActiu, projecteId);
            panelParticipants.Children.Add(CrearEtiquetaParticipant(nom, "#2196F3", 0));
        }

        // Comprova si un participant ja està pintat al panell (per evitar duplicats a la UI).
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

        // Quan l'usuari canvia el Sprint Master amb el ComboBox, es guarda a la BDD.
        private void cmbSprintMaster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Si estem carregant dades, no fer res.
            // Això evita que quan seleccionem  un item (en carregar) es faci un UPDATE a la BDD.
            if (_carregantDades) return;
            if (cmbSprintMaster.SelectedItem == null) return;
            if (_projecteActiuId <= 0) return;

            var nomUsuari = cmbSprintMaster.SelectedItem.ToString();

            // Guardem l'Sprint Master (IdResponsable) al projecte actual.
            _projectesService.ActualitzarSprintMaster(nomUsuari, DataBase.grupActiu, _projecteActiuId);

            // Després de guardar, tornem a consultar què hi ha a la BDD i actualitzem el label.
            var idResponsable = _projectesService.ObtenirIdResponsableProjecte(_projecteActiuId);
            
            _carregantDades = true;
            lblSprintMasterActual.Content = _projectesService.ObtenirNomResponsable(idResponsable) ?? "";
            _carregantDades = false;
        }

        // Botó per desvincular un participant del projecte.
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

                // Esborrem la relació usuari-projecte.
                _participantsService.DesvincularParticipant(wnd.ParticipantSeleccionat, DataBase.grupActiu, projecteId);

                // Treure del panell.
                TreureParticipantDelPanell(wnd.ParticipantSeleccionat);
                MessageBox.Show($"S'ha desvinculat '{wnd.ParticipantSeleccionat}' del projecte.");
            }
        }

        // Botó per eliminar un usuari completament de la BDD (amb confirmació).
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

                    // Recarreguem desplegables i UI.
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

        // Treu l'etiqueta visual d'un participant del panell.
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

        // Crea un control visual (Border + TextBlock) per representar un participant al panell.
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
                    // El text mostra: "Nom  numTasques" (aquí numTasques està fixat a 0)
                    Text = $"{nom}  {numTasques}",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                }
            };
        }

        #endregion

        #region Drag & Drop

        // Detecta el moviment amb el botó esquerre per iniciar un drag d'una tasca.
        private void ListBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;

            var listBox = sender as ListBox;
            var tasca = listBox?.SelectedItem as Tasques;
            if (tasca == null) return;

            _draggedTask = tasca;
            _sourceListBox = listBox;

            // Fem el drag amb un DataObject que conté la tasca.
            DragDrop.DoDragDrop(listBox, new DataObject("Tasca", tasca), DragDropEffects.Move);

            _draggedTask = null;
            _sourceListBox = null;
        }

        // Quan arrosseguem sobre una ListBox, definim si l'efecte és Move o None.
        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent("Tasca") ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        // Quan deixem anar una tasca sobre una altra columna:
        // - la treiem de la llista origen
        // - la posem a la llista destí
        // - guardem el canvi a la BDD (IdColumna)
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Tasca")) return;

            var tasca = e.Data.GetData("Tasca") as Tasques;
            var targetListBox = sender as ListBox;

            if (tasca == null || targetListBox == null) return;
            if (_sourceListBox == null || _sourceListBox == targetListBox) return;

            TreureTascaDeLlistaOrigen(tasca);
            AfegirTascaALlistaDestí(tasca, targetListBox);

            // Guardem a la BDD la columna nova.
            _tasquesService.ActualitzarColumnaTasca(tasca);

            // Re-ordenem i refresquem la UI.
            OrdenarTotesLesColumnes();
            RefrescarColumnes();
        }

        // Treu la tasca de la llista vinculada al ListBox origen.
        private void TreureTascaDeLlistaOrigen(Tasques tasca)
        {
            GetLlistaPerListBox(_sourceListBox)?.Remove(tasca);
        }

        // Afegeix la tasca a la llista de destí i actualitza IdColumna + Estat del model.
        private void AfegirTascaALlistaDestí(Tasques tasca, ListBox targetListBox)
        {
            var novaColumna = GetColumnaPerListBox(targetListBox);
            tasca.IdColumna = novaColumna;
            tasca.Estat = ConsultesTasquesService.GetEstatPerColumna(novaColumna);
            GetLlistaPerListBox(targetListBox)?.Add(tasca);
        }

        #endregion

        #region Utilitats columnes

        // Buida totes les llistes (les columnes del Kanban).
        private void NetejarColumnes()
        {
            Backlog.Clear();
            Todo.Clear();
            Doing.Clear();
            Done.Clear();
        }

        // fa un refresh de els ListBox perque es vegin les llistes actualitzades.
        private void RefrescarColumnes()
        {
            listBacklog.Items.Refresh();
            listTodo.Items.Refresh();
            listDoing.Items.Refresh();
            listDone.Items.Refresh();
        }

        // Ordena totes les columnes.
        private void OrdenarTotesLesColumnes()
        {
            _tasquesService.OrdenarLlista(Backlog);
            _tasquesService.OrdenarLlista(Todo);
            _tasquesService.OrdenarLlista(Doing);
            _tasquesService.OrdenarLlista(Done);
        }

        // Afegeix una tasca a la llista correcta segons IdColumna.
        private void AfegirTascaAColumna(Tasques tasca)
        {
            GetLlistaPerColumna(tasca.IdColumna)?.Add(tasca);
        }

        // Retorna la llista de tasques (columna) segons l'id de columna.
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

        // Retorna la llista de tasques segons quin ListBox s'està utilitzant.
        private List<Tasques> GetLlistaPerListBox(ListBox listBox)
        {
            if (listBox == listBacklog) return Backlog;
            if (listBox == listTodo) return Todo;
            if (listBox == listDoing) return Doing;
            if (listBox == listDone) return Done;
            return null;
        }

        // Converteix un ListBox en un id de columna.
        private byte GetColumnaPerListBox(ListBox listBox)
        {
            if (listBox == listBacklog) return 1;
            if (listBox == listTodo) return 2;
            if (listBox == listDoing) return 3;
            if (listBox == listDone) return 4;
            return 0;
        }

        // Llegeix els participants mostrats al panell i en retorna els noms.
        // (Es fa servir per passar la llista de participants a TascaWindow.)
        private List<string> ObtenirParticipantsProjecte()
        {
            var participants = new List<string>();
            foreach (Border b in panelParticipants.Children)
            {
                var tb = b.Child as TextBlock;
                if (tb != null)
                {
                    // El Text s'està guardant com "Nom  numTasques".
                    // Separem per obtenir només el nom.
                    var text = tb.Text;
                    var spaceIndex = text.LastIndexOf("  ");
                    participants.Add(spaceIndex > 0 ? text.Substring(0, spaceIndex) : text.Trim());
                }
            }
            return participants;
        }

        #endregion

        #region Altres botons

        // Mostra informació bàsica del programa.
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
