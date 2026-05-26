using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Depolama kayıtları - Ürünlerin depolanma bilgileri
    /// </summary>
    public class Depolama
    {
        [Key]
        public int Id { get; set; }

        public int UrunId { get; set; }

        [MaxLength(200)]
        public string DepoAdi { get; set; }

        [MaxLength(500)]
        public string DepoAdresi { get; set; }

        public double? Sicaklik { get; set; }

        public double? NemOrani { get; set; }

        public DateTime? GirisTarihi { get; set; }

        public DateTime? CikisTarihi { get; set; }

        [MaxLength(256)]
        public string HashDegeri { get; set; }

        public int? BlokZincirKayitId { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public virtual Urun Urun { get; set; }
        public virtual BlokZincirKayit? BlokZincirKayit { get; set; }
    }
}