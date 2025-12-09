using System;

namespace WebApplicationAPIDemo.Model
{
    public class Projecte
    {
        public int IdProjecte { get; set; }
        public string Titol { get; set; }
        public DateTime DataInici { get; set; }
        public DateTime? DataFi { get; set; }
        public int IdResponsable { get; set; }
    }
}
