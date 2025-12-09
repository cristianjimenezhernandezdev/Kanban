

namespace WebApplicationAPIDemo.Model
{
    public class Usuari
    {

        public long IdUsuari { get; set; }
        public string Nom { get; set; }
        public int IdGrup { get; set; }
        public Grup Grup { get; set; }
    }
}
