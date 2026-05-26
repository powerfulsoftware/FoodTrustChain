using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Sistem kullanıcıları - Çiftçi, Lojistik, Depo, Market, Yönetici, Tüketici
    /// </summary>
    public class Uyeler
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ad { get; set; }

        [Required]
        [MaxLength(50)]
        public string Soyad { get; set; }

        [Required]
        [MaxLength(100)]
        public string Eposta { get; set; }

        [Required]
        public string Sifre { get; set; }

        [MaxLength(20)]
        public string Telefon { get; set; }

        [Required]
        [MaxLength(20)]
        public string Rol { get; set; }

        public bool AktifMi { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public virtual ICollection<Urun> Urunler { get; set; }

        public string TamAd => $"{Ad} {Soyad}";
    }
}