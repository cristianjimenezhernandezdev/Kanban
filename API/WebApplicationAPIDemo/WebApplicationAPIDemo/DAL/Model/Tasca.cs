using System;

namespace WebApplicationAPIDemo.Model
{
    public class Tasca
    {
        public int IdTasca { get; set; }
        public int IdProjecte { get; set; }
        public int IdColumna { get; set; }
        public int? IdUsuariResponsable { get; set; }
        public string Descripcio { get; set; }
        public byte? Prioritat { get; set; }
        public DateTime DataCreacio { get; set; }
        public DateTime? DataVenciment { get; set; }
    }
}
