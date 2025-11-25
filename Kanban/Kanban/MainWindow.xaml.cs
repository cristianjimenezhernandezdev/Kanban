using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Kanban.MainWindow;

namespace Kanban
{
    public partial class MainWindow : Window
    {
        public List<Tasques> ListaDeTasques { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            ListaDeTasques = new List<Tasques>();
            
            ListaDeTasques.Add(new Tasques()
            {
                Titol = "Dissenyar interfície d'usuari",
                Descripcio = "Crear dissenys per a la nova aplicació mòbil.",
                Estat = "En Progrés",
                Responsable = "Anna",
                DataVenciment = DateTime.Now.AddDays(7),
                Prioritat = 2,
                DataCreacio = DateTime.Now.AddDays(-3),
                Notes = "Revisar amb l'equip de màrqueting."
            });

            ListaDeTasques.Add(new Tasques()
            {
                Titol = "Implementar autenticació",
                Descripcio = "Afegir funcionalitat d'inici de sessió i registre.",
                Estat = "Per Fer",
                Responsable = "Joan",
                DataVenciment = DateTime.Now.AddDays(14),
                Prioritat = 1,
                DataCreacio = DateTime.Now,
                Notes = "Utilitzar OAuth2 per a la seguretat."
            });

            ListaDeTasques.Add(new Tasques()
            {
                Titol = "Provar aplicació",
                Descripcio = "Realitzar proves completes de la nova aplicació.",
                Estat = "Fet",
                Responsable = "Maria",
                DataVenciment = DateTime.Now.AddDays(-1),
                Prioritat = 3,
                DataCreacio = DateTime.Now.AddDays(-10),
                Notes = "Documentar tots els errors trobats."
            });

            llistaTasques.ItemsSource = ListaDeTasques;
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
        }

        private void Button_Click_AfegirTasca(object sender, RoutedEventArgs e)
        {
            Tasques nova = new Tasques()
            {
                Titol = "Titol 1",
                Estat = "Per Fer",
                DataCreacio = DateTime.Now,
                Prioritat = 1
            };

            ListaDeTasques.Add(nova);

            // Refrescar interfície
            llistaTasques.Items.Refresh();
        }
    }
}
