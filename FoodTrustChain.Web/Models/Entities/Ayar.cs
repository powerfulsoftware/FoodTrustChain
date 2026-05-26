using System;
using System.ComponentModel.DataAnnotations;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Sistem ayarları - Key-value formatında sistem parametreleri
    /// </summary>
    public class Ayar
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Anahtar { get; set; }

        [MaxLength(500)]
        public string Deger { get; set; }

        [MaxLength(500)]
        public string Aciklama { get; set; }

        public bool AktifMi { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}