using Kanban.Programs.cs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace Kanban
{
    public partial class TascaWindow : Window
    {
        // Propietats amb les dades que l'usuari omple al formulari
        public string Descripcio { get; private set; }
        public int Prioritat { get; private set; }
        public DateTime? DataVenciment { get; private set; }
        public string Responsable { get; private set; }
        public string Notes { get; private set; }
        // Tasca creada o modificada que retornarem al MainWindow
        public Tasques TascaResultant { get; private set; }

        // Indica si estem editant una tasca existent o creant-ne una de nova
        private readonly bool _isEditMode;
        // Referència a la tasca original quan estem editant
        private readonly Tasques _tascaOriginal;
        // Columna del kanban on s'afegirà la tasca nova per defecte
        private readonly byte _columnaPerDefecte;
        // Servei que fa les consultes de base de dades per a les tasques
        private readonly ConsultesTasquesService _tasquesService;

        private DateTime dataVencimentPerDefecte = DateTime.Today;

        // Constructor principal que rep la llista de participants, la tasca a editar (opcional) i la columna per defecte
        public TascaWindow(List<string> participants, Tasques tasca = null, byte columnaPerDefecte = 1)
        {
            InitializeComponent();

            _tasquesService = new ConsultesTasquesService();
            // Omplim els desplegables amb participants i nivells de prioritat
            cmbParticipants.ItemsSource = participants;
            cmbPrioritat.ItemsSource = new[] { "Alta", "Mitja", "Baixa" };

            _tascaOriginal = tasca;
            _isEditMode = tasca != null; // True si rebem una tasca (mode editar)
            _columnaPerDefecte = columnaPerDefecte;

            // Si estem editant, carreguem les dades de la tasca als controls
            if (_isEditMode)
            {
                txtDescripcio.Text = tasca.Descripcio;

                var indexPrioritat = Math.Max(0, tasca.Prioritat - 1);
                if (indexPrioritat < 3)
                    cmbPrioritat.SelectedIndex = indexPrioritat;

                if (!string.IsNullOrEmpty(tasca.Responsable))
                    cmbParticipants.SelectedItem = tasca.Responsable;

                // Si la data de venciment és anterior a avui, la posem a avui; si no, mostrem la data guardada
                if (dpDataVenciment.SelectedDate < DateTime.Now)
                    tasca.DataVenciment = DateTime.Today;
                else
                    dpDataVenciment.SelectedDate = tasca.DataVenciment;

                txtNotes.Text = tasca.Notes;
            }
        }

        // Constructor buit per compatibilitat XAML
        public TascaWindow() : this(new List<string>()) { }

        // Botó Acceptar: valida el formulari, crea oactualitza la tasca i tanca la finestra
        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulari())
                return;

            if (_isEditMode)
            {
                if (!ActualitzarTascaExistent())
                    return;
            }
            else
            {
                if (!CrearNovaTasca())
                    return;
            }

            DialogResult = true; // Indiquem al MainWindow que tot ha anat bé
            Close();
        }

        // Llegeix i comprova les dades del formulari
        private bool ValidarFormulari()
        {
            Descripcio = txtDescripcio.Text;

            if (string.IsNullOrWhiteSpace(Descripcio))
            {
                MessageBox.Show("Introdueix una descripció.");
                return false;
            }

            // Si no s'ha seleccionat prioritat, per defecte és Alta (índex 0)
            if (cmbPrioritat.SelectedIndex < 0)
            {
                MessageBox.Show("Selecciona una prioritat.");
                return false;
            }

            // Prioritat: índex 0 = Alta (1), índex 1 = Mitja (2), índex 2 = Baixa (3)
            Prioritat = cmbPrioritat.SelectedIndex + 1;
            DataVenciment = dpDataVenciment.SelectedDate;
            Responsable = cmbParticipants.SelectedItem?.ToString();
            Notes = txtNotes.Text;

            return true;
        }

        // Crea una tasca nova i la guarda a la base de dades
        private bool CrearNovaTasca()
        {
            // Si la data de venciment és anterior a avui o no s'ha seleccionat, posem avui
            
            if (DataVenciment.HasValue && DataVenciment.Value >= DateTime.Today)
                dataVencimentPerDefecte = DataVenciment.Value;

            var nova = new Tasques
            {
                Descripcio = Descripcio,
                Estat = ConsultesTasquesService.GetEstatPerColumna(_columnaPerDefecte),
                Responsable = Responsable,
                Prioritat = Prioritat,
                DataVenciment = dataVencimentPerDefecte,
                Notes = Notes,
                DataCreacio = DateTime.Now,
                IdColumna = _columnaPerDefecte
            };

            try
            {
                // Inserim la tasca a la BDD i rebem el seu Id
                nova.IdTasca = _tasquesService.InserirTasca(nova, DataBase.grupActiu);
                TascaResultant = nova;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la tasca Gamba!: " + ex.Message);
                return false;
            }
        }

        // Actualitza una tasca existent i desa els canvis a la base de dades
        private bool ActualitzarTascaExistent()
        {
            // Si la data de venciment és anterior a avui o no s'ha seleccionat, posem avui
            
            if (DataVenciment.HasValue && DataVenciment.Value >= DateTime.Today)
                dataVencimentPerDefecte = DataVenciment.Value;

            _tascaOriginal.Descripcio = Descripcio;
            _tascaOriginal.Prioritat = Prioritat;
            _tascaOriginal.Responsable = Responsable;
            _tascaOriginal.DataVenciment = dataVencimentPerDefecte;
            _tascaOriginal.Notes = Notes;

            try
            {
                _tasquesService.ActualitzarDetallsTasca(_tascaOriginal, DataBase.grupActiu);
                TascaResultant = _tascaOriginal;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualitzar la tasca pillin: " + ex.Message);
                return false;
            }
        }
    }
}
