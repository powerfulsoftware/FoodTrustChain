using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Taşıt kayıtları - Lojistik araçları
    /// </summary>
    public class Tasit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string PlakaNo { get; set; }

        [MaxLength(100)]
        public string SoforAdi { get; set; }

        [MaxLength(20)]
        public string SoforTelefon { get; set; }

        [MaxLength(50)]
        public string AracTipi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Kapasite { get; set; }

        [MaxLength(50)]
        public string SigortaNo { get; set; }

        public bool AktifMi { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public virtual ICollection<Nakliye> Nakliyeler { get; set; }
    }
}