using System;
using System.Collections.Generic;
using System.Windows;
using Kanban.Programs.cs;

namespace Kanban
{
    public partial class TascaWindow : Window
    {
        public string Descripcio { get; private set; }
        public int Prioritat { get; private set; }
        public DateTime? DataVenciment { get; private set; }
        public string Responsable { get; private set; }
        public string Notes { get; private set; }
        public Tasques TascaResultant { get; private set; }

        private readonly bool _isEditMode;
        private readonly Tasques _tascaOriginal;
        private readonly byte _columnaPerDefecte;
        private readonly KanbanService _kanbanService;

        public TascaWindow(List<string> participants, Tasques tasca = null, byte columnaPerDefecte = 1)
        {
            InitializeComponent();
            _kanbanService = new KanbanService();
            cmbParticipants.ItemsSource = participants;
            cmbPrioritat.ItemsSource = new[] { "Alta", "Mitja", "Baixa" };

            _tascaOriginal = tasca;
            _isEditMode = tasca != null;
            _columnaPerDefecte = columnaPerDefecte;

            if (_isEditMode)
            {
                txtDescripcio.Text = tasca.Descripcio;
                int indexPrioritat = Math.Max(0, tasca.Prioritat - 1);
                if (indexPrioritat < 3)
                    cmbPrioritat.SelectedIndex = indexPrioritat;

                if (!string.IsNullOrEmpty(tasca.Responsable))
                    cmbParticipants.SelectedItem = tasca.Responsable;

                dpDataVenciment.SelectedDate =
                    tasca.DataVenciment == DateTime.MinValue ? (DateTime?)null : tasca.DataVenciment;
                txtNotes.Text = tasca.Notes;
            }
        }

        public TascaWindow() : this(new List<string>()) { }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulari())
                return;

            if (_isEditMode)
            {
                if (!ActualitzarTascaExisting())
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

        private bool ValidarFormulari()
        {
            if (cmbPrioritat.SelectedIndex < 0)
            {
                MessageBox.Show("Selecciona una prioritat.");
                return false;
            }

            Descripcio = txtDescripcio.Text;
            Prioritat = cmbPrioritat.SelectedIndex + 1;
            DataVenciment = dpDataVenciment.SelectedDate;
            Responsable = cmbParticipants.SelectedItem?.ToString();
            Notes = txtNotes.Text;

            if (string.IsNullOrWhiteSpace(Descripcio))
            {
                MessageBox.Show("Introdueix una descripció.");
                return false;
            }

            return true;
        }

        private bool CrearNovaTasca()
        {
            Tasques nova = new Tasques
            {
                Titol = Descripcio,
                Descripcio = Descripcio,
                Estat = KanbanService.GetEstatPerColumna(_columnaPerDefecte),
                Responsable = Responsable,
                Prioritat = Prioritat,
                DataVenciment = DataVenciment ?? DateTime.MinValue,
                Notes = Notes,
                DataCreacio = DateTime.Now,
                IdColumna = _columnaPerDefecte
            };

            try
            {
                nova.IdTasca = _kanbanService.InserirTasca(nova, LoginWindow.grupActiu);
                TascaResultant = nova;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la tasca: " + ex.Message);
                return false;
            }
        }

        private bool ActualitzarTascaExisting()
        {
            _tascaOriginal.Descripcio = Descripcio;
            _tascaOriginal.Titol = Descripcio;
            _tascaOriginal.Prioritat = Prioritat;
            _tascaOriginal.Responsable = Responsable;
            _tascaOriginal.DataVenciment = DataVenciment ?? DateTime.MinValue;
            _tascaOriginal.Notes = Notes;

            try
            {
                _kanbanService.ActualitzarDetallsTasca(_tascaOriginal, LoginWindow.grupActiu);
                TascaResultant = _tascaOriginal;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualitzar la tasca: " + ex.Message);
                return false;
            }
        }
    }
}
