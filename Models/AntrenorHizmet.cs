namespace SporSalonu.Models
{

    public class AntrenorHizmet
    {
        public int Id { get; set; }

        public int AntrenorId { get; set; }
        public virtual Antrenor? Antrenor { get; set; }

        public int HizmetId { get; set; }
        public virtual Hizmet? Hizmet { get; set; }
    }
}


