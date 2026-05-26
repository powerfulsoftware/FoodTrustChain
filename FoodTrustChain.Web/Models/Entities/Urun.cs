using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Gıda ürünleri - Tarladan sofraya kadar izlenebilir ürünler
    /// </summary>
    public class Urun
    {
        [Key]
        public int Id { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Ad { get; set; }

        [MaxLength(100)]
        public string Kategori { get; set; }

        [MaxLength(1000)]
        public string Aciklama { get; set; }

        public int UreticiId { get; set; }

        [MaxLength(50)]
        public string Durum { get; set; } = "Uretimde";

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public virtual Uyeler Uretici { get; set; }
        public virtual ICollection<Uretim> Uretimler { get; set; }
        public virtual ICollection<Depolama> DepolamaIslemleri { get; set; }
        public virtual ICollection<Nakliye> Nakliyeler { get; set; }
    }
}