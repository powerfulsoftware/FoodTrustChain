using System;
using System.ComponentModel.DataAnnotations;

namespace FoodTrustChain.Web.Models.Entities
{
    /// <summary>
    /// Ürün hareket takibi - Ürünün tedarik zincirindeki hareketleri
    /// </summary>
    public class UrunHareketi
    {
        [Key]
        public int Id { get; set; }

        public int UrunId { get; set; }

        [MaxLength(50)]
        public string HareketTipi { get; set; }

        [MaxLength(50)]
        public string KaynakTipi { get; set; }

        public int KaynakId { get; set; }

        [MaxLength(50)]
        public string HedefTipi { get; set; }

        public int HedefId { get; set; }

        [MaxLength(500)]
        public string Aciklama { get; set; }

        public DateTime Tarih { get; set; } = DateTime.Now;

        public virtual Urun Urun { get; set; }
    }
}