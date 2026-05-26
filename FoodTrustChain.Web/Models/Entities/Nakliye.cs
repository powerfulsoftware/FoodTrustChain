using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Nakliye kayıtları - Ürün taşıma bilgileri
    /// </summary>
    public class Nakliye
    {
        [Key]
        public int Id { get; set; }

        public int UrunId { get; set; }

        public int TasitId { get; set; }

        [MaxLength(100)]
        public string SoforAdi { get; set; }

        [MaxLength(500)]
        public string BaslangicAdresi { get; set; }

        [MaxLength(500)]
        public string BitisAdresi { get; set; }

        public DateTime? TasimaBaslangicTarihi { get; set; }

        public DateTime? TasimaBitisTarihi { get; set; }

        public double? Sicaklik { get; set; }

        [MaxLength(256)]
        public string HashDegeri { get; set; }

        public int? BlokZincirKayitId { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public virtual Urun Urun { get; set; }
        public virtual Tasit Tasit { get; set; }
        public virtual BlokZincirKayit? BlokZincirKayit { get; set; }
    }
}