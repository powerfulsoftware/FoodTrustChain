using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Üretim kayıtları - Çiftlikteki üretim/hasat bilgileri
    /// </summary>
    public class Uretim
    {
        [Key]
        public int Id { get; set; }

        public int? UrunId { get; set; }

        public int? UreticiId { get; set; }

        [MaxLength(200)]
        public string? CiftlikAdi { get; set; }

        [MaxLength(500)]
        public string? Konum { get; set; }

        public DateTime? HasatTarihi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Miktar { get; set; }

        [MaxLength(20)]
        public string? Birim { get; set; } = "kg";

        [MaxLength(500)]
        public string? GorselYolu { get; set; }

        [MaxLength(256)]
        public string? HashDegeri { get; set; }

        public int? BlokZincirKayitId { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public virtual Urun? Urun { get; set; }
        public virtual Uyeler? Uretici { get; set; }
        public virtual BlokZincirKayit? BlokZincirKayit { get; set; }
    }
}