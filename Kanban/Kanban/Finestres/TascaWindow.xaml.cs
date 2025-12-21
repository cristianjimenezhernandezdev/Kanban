using Kanban.Programs.cs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace Kanban
{
    // Finestra per crear o editar una tasca.
    // Aquesta finestra:
    // - rep la llista de participants del projecte (per assignar Responsable)
    // - pot obrir-se en mode crear (tasca null) o en mode editar (tasca existent)
    // - quan l'usuari accepta, guarda a la BDD i retorna la TascaResultant al MainWindow
    public partial class TascaWindow : Window
    {
        // Propietats amb les dades que l'usuari omple al formulari.
        public string Descripcio { get; private set; }
        public int Prioritat { get; private set; }
        public DateTime? DataVenciment { get; private set; }
        public string Responsable { get; private set; }
        public string Notes { get; private set; }

        // Tasca creada o modificada que retornarem al MainWindow.
        public Tasques TascaResultant { get; private set; }

        // Indica si estem editant una tasca existent o creant-ne una de nova.
        private readonly bool _isEditMode;

        // Referència a la tasca original quan estem editant (la mateixa instància que hi ha a la llista del MainWindow).
        private readonly Tasques _tascaOriginal;

        // Columna del kanban on s'afegirà la tasca nova per defecte.
        private readonly byte _columnaPerDefecte;

        // Id del projecte on s'afegirà/editarà la tasca.
        private readonly int _idProjecte;

        // Servei per fer INSERT/UPDATE de tasques a la BDD.
        private readonly ConsultesTasquesService _tasquesService;

        // Data de venciment per defecte si l'usuari no selecciona res o selecciona una data antiga.
        private DateTime dataVencimentPerDefecte = DateTime.Today;

        // Constructor principal.
        // Rep:
        // - participants: noms dels participants del projecte
        // - tasca: tasca existent (mode editar) o null (mode crear)
        // - columnaPerDefecte: columna on s'ha de crear la tasca si és nova
        // - idProjecte: projecte al qual pertany la tasca
        public TascaWindow(List<string> participants, Tasques tasca, byte columnaPerDefecte, int idProjecte)
        {
            InitializeComponent();

            _tasquesService = new ConsultesTasquesService();
            _idProjecte = idProjecte;

            // Omplim els desplegables amb participants i nivells de prioritat.
            cmbParticipants.ItemsSource = participants;
            cmbPrioritat.ItemsSource = new[] { "Alta", "Mitja", "Baixa" };

            _tascaOriginal = tasca;
            _isEditMode = tasca != null; // True si rebem una tasca (mode editar)
            _columnaPerDefecte = columnaPerDefecte;

            // Si estem editant, carreguem les dades de la tasca als controls.
            if (_isEditMode)
            {
                txtDescripcio.Text = tasca.Descripcio;

                // Prioritat a la UI: índex 0/1/2 => prioritat 1/2/3.
                var indexPrioritat = Math.Max(0, tasca.Prioritat - 1);
                if (indexPrioritat < 3)
                    cmbPrioritat.SelectedIndex = indexPrioritat;

                // Seleccionem responsable si en té.
                if (!string.IsNullOrEmpty(tasca.Responsable))
                    cmbParticipants.SelectedItem = tasca.Responsable;

                // Si la data guardada és anterior a avui, la corregim a avui.
                // Si no, la mostrem.
                if (dpDataVenciment.SelectedDate < DateTime.Now)
                    tasca.DataVenciment = DateTime.Today;
                else
                    dpDataVenciment.SelectedDate = tasca.DataVenciment;

                // Carreguem notes.
                txtNotes.Text = tasca.Notes;
            }
        }

        // Constructor buit per compatibilitat XAML així no falla.
        
        public TascaWindow() : this(new List<string>(), null, 1, 0) { }

        // Botó Acceptar:
        // - valida dades
        // - crea una tasca nova o actualitza l'existent
        // - retorna DialogResult=true per indicar al MainWindow que s'ha guardat bé
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

            DialogResult = true;
            Close();
        }

        // Llegeix i comprova les dades del formulari.
        private bool ValidarFormulari()
        {
            Descripcio = txtDescripcio.Text;

            if (string.IsNullOrWhiteSpace(Descripcio))
            {
                MessageBox.Show("Introdueix una descripció.");
                return false;
            }

            // Prioritat obligatòria.
            if (cmbPrioritat.SelectedIndex < 0)
            {
                MessageBox.Show("Selecciona una prioritat.");
                return false;
            }

            // Prioritat: índex 0 = Alta (1), índex 1 = Mitja (2), índex 2 = Baixa (3).
            Prioritat = cmbPrioritat.SelectedIndex + 1;

            // Data i responsable poden ser opcionals (segons l'ús).
            DataVenciment = dpDataVenciment.SelectedDate;
            Responsable = cmbParticipants.SelectedItem?.ToString();
            Notes = txtNotes.Text;

            return true;
        }

        // Mode crear:
        // - crea un objecte Tasques
        // - fa INSERT a la BDD
        // - guarda l'IdTasca retornat
        private bool CrearNovaTasca()
        {
            // Si l'usuari ha triat una data vàlida (avui o futur), la fem servir.
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
                // Inserim la tasca a la BDD amb l'idProjecte correcte.
                // InserirTasca retorna l'id generat a la BDD.
                nova.IdTasca = _tasquesService.InserirTasca(nova, DataBase.grupActiu, _idProjecte);
                TascaResultant = nova;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la tasca: Gamba!" + ex.Message);
                return false;
            }
        }

        // Mode editar:
        // - modifica la tasca original
        // - fa UPDATE a la BDD
        private bool ActualitzarTascaExistent()
        {
            // Si l'usuari ha triat una data vàlida (avui o futur), la fem servir.
            if (DataVenciment.HasValue && DataVenciment.Value >= DateTime.Today)
                dataVencimentPerDefecte = DataVenciment.Value;

            // Actualitzem l'objecte en memòria (és el mateix que hi ha a la llista del MainWindow).
            _tascaOriginal.Descripcio = Descripcio;
            _tascaOriginal.Prioritat = Prioritat;
            _tascaOriginal.Responsable = Responsable;
            _tascaOriginal.DataVenciment = dataVencimentPerDefecte;
            _tascaOriginal.Notes = Notes;

            try
            {
                // Guardem canvis a la BDD.
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
