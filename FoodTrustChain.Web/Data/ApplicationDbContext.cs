using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodTrustChain.Web.Models.Entities;
using System;

namespace FoodTrustChain.Web.Data
{
    /// <summary>
    /// Uygulama Veritabanı Bağlamı - EF Core ile veritabanı işlemleri
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Uyeler> Uyeler { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Uretim> Uretimler { get; set; }
        public DbSet<Depolama> DepolamaIslemleri { get; set; }
        public DbSet<Tasit> Tasitler { get; set; }
        public DbSet<Nakliye> Nakliyeler { get; set; }
        public DbSet<BlokZincirKayit> BlokZincirKayitlari { get; set; }
        public DbSet<UrunHareketi> UrunHareketleri { get; set; }
        public DbSet<Ayar> Ayar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Uyeler>()
                .HasIndex(u => u.Eposta)
                .IsUnique();

            modelBuilder.Entity<Urun>()
                .HasIndex(u => u.Guid)
                .IsUnique();

            modelBuilder.Entity<Tasit>()
                .HasIndex(t => t.PlakaNo)
                .IsUnique();

            modelBuilder.Entity<BlokZincirKayit>()
                .HasIndex(b => b.MevcutHash)
                .IsUnique();

            modelBuilder.Entity<Urun>()
                .HasOne(u => u.Uretici)
                .WithMany(y => y.Urunler)
                .HasForeignKey(u => u.UreticiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Uretim>()
                .HasOne(u => u.Urun)
                .WithMany(u => u.Uretimler)
                .HasForeignKey(u => u.UrunId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Uretim>()
                .HasOne(u => u.Uretici)
                .WithMany()
                .HasForeignKey(u => u.UreticiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Depolama>()
                .HasOne(d => d.Urun)
                .WithMany(u => u.DepolamaIslemleri)
                .HasForeignKey(d => d.UrunId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Nakliye>()
                .HasOne(n => n.Urun)
                .WithMany(u => u.Nakliyeler)
                .HasForeignKey(n => n.UrunId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Nakliye>()
                .HasOne(n => n.Tasit)
                .WithMany(t => t.Nakliyeler)
                .HasForeignKey(n => n.TasitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UrunHareketi>()
                .HasOne(u => u.Urun)
                .WithMany()
                .HasForeignKey(u => u.UrunId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}