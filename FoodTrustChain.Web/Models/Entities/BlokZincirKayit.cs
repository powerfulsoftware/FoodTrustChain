using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Blok zincir kayıtları - Değiştirilemez işlem kayıtları
    /// </summary>
    public class BlokZincirKayit
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string OncekiHash { get; set; }

        [MaxLength(256)]
        public string MevcutHash { get; set; }

        [MaxLength(50)]
        public string IslemTipi { get; set; }

        [MaxLength(4000)]
        public string IslemIcerigi { get; set; }

        public DateTime ZamanDamgasi { get; set; } = DateTime.Now;

        public int Nonce { get; set; } = 0;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public virtual ICollection<Uretim> Uretimler { get; set; }
        public virtual ICollection<Depolama> DepolamaIslemleri { get; set; }
        public virtual ICollection<Nakliye> Nakliyeler { get; set; }
    }
}