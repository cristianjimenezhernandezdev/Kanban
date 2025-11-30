
using System;
using System.Collections.Generic;
using System.Windows;

namespace Kanban
{
    public partial class MainWindow : Window
    {
        // Llistes de cada columna
        public List<Tasques> Backlog { get; set; }
        public List<Tasques> Todo { get; set; }
        public List<Tasques> Doing { get; set; }
        public List<Tasques> Done { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Inicialitzar columnes
            Backlog = new List<Tasques>();
            Todo = new List<Tasques>();
            Doing = new List<Tasques>();
            Done = new List<Tasques>();

            // Exemple de dades inicials
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

            // Enllaçar llistes amb ListBox
            listBacklog.ItemsSource = Backlog;
            listTodo.ItemsSource = Todo;
            listDoing.ItemsSource = Doing;
            listDone.ItemsSource = Done;
        }

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

            public override string ToString()
            {
                return Titol;
            }
        }

        private void btnAddBacklog_Click(object sender, RoutedEventArgs e)
        {
            TascaWindow tascaWindow = new TascaWindow();

            if (tascaWindow.ShowDialog() == false)
            {
                Backlog.Add(new Tasques()
                {
                    Titol = tascaWindow.Titol,
                    Descripcio = tascaWindow.Descripcio,
                    Estat = "Backlog",
                    Responsable = tascaWindow.Responsable,
                    Prioritat = tascaWindow.Prioritat,
                    DataVenciment = tascaWindow.DataVenciment ?? DateTime.Now,
                    Notes = tascaWindow.Notes,
                    DataCreacio = DateTime.Now
                });
                // Actualitzar la vista
                listBacklog.Items.Refresh();
            }
        }

        private void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            TascaWindow tascaWindow = new TascaWindow();

            tascaWindow.Show();

            listTodo.Items.Refresh();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Aplicació Kanban creada per Amine.\nVersió 1.0\nGestiona tasques i projectes de forma visual.",
                "Informació",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private string ConsutlaSelect()
        {
            return "";
        }
    }
}
