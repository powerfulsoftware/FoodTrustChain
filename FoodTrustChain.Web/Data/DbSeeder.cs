using FoodTrustChain.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FoodTrustChain.Web.Data
{
    /// <summary>
    /// Veritabanı Seed Data - İlk verilerin yüklenmesi
    /// </summary>
    public static class DbSeeder
    {
        public static string HashSifre(string sifre)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sifre));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.Migrate();

            if (!await roleManager.RoleExistsAsync("Yonetici"))
            {
                await roleManager.CreateAsync(new IdentityRole("Yonetici"));
            }
            if (!await roleManager.RoleExistsAsync("Uretici"))
            {
                await roleManager.CreateAsync(new IdentityRole("Uretici"));
            }
            if (!await roleManager.RoleExistsAsync("Depo"))
            {
                await roleManager.CreateAsync(new IdentityRole("Depo"));
            }
            if (!await roleManager.RoleExistsAsync("Lojistik"))
            {
                await roleManager.CreateAsync(new IdentityRole("Lojistik"));
            }
            if (!await roleManager.RoleExistsAsync("Market"))
            {
                await roleManager.CreateAsync(new IdentityRole("Market"));
            }
            if (!await roleManager.RoleExistsAsync("Tuketici"))
            {
                await roleManager.CreateAsync(new IdentityRole("Tuketici"));
            }

            if (!context.Uyeler.Any())
            {
                var yonetici = new Uyeler
                {
                    Ad = "Selman",
                    Soyad = "Yakut",
                    Eposta = "yonetici@foodtrust.com",
                    Sifre = HashSifre("Yonetici123!"),
                    Telefon = "0532 111 1111",
                    Rol = "Yonetici",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Uyeler.Add(yonetici);

                var uretici = new Uyeler
                {
                    Ad = "Mustafa",
                    Soyad = "Celenk",
                    Eposta = "uretici@foodtrust.com",
                    Sifre = HashSifre("Uretici123!"),
                    Telefon = "0532 222 2222",
                    Rol = "Uretici",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Uyeler.Add(uretici);

                var depo = new Uyeler
                {
                    Ad = "Selcuk",
                    Soyad = "Yakut",
                    Eposta = "depo@foodtrust.com",
                    Sifre = HashSifre("Depo123!"),
                    Telefon = "0532 333 3333",
                    Rol = "Depo",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Uyeler.Add(depo);

                var lojistik = new Uyeler
                {
                    Ad = "Ebu",
                    Soyad = "Bekir",
                    Eposta = "lojistik@foodtrust.com",
                    Sifre = HashSifre("Lojistik123!"),
                    Telefon = "0532 444 4444",
                    Rol = "Lojistik",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Uyeler.Add(lojistik);

                var market = new Uyeler
                {
                    Ad = "Ahmet",
                    Soyad = "Yilmaz",
                    Eposta = "market@foodtrust.com",
                    Sifre = HashSifre("Market123!"),
                    Telefon = "0532 555 5555",
                    Rol = "Market",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Uyeler.Add(market);

                var tuketici = new Uyeler
                {
                    Ad = "Ayse",
                    Soyad = "Demir",
                    Eposta = "tuketici@foodtrust.com",
                    Sifre = HashSifre("Tuketici123!"),
                    Telefon = "0532 666 6666",
                    Rol = "Tuketici",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Uyeler.Add(tuketici);

                await context.SaveChangesAsync();

                var users = new List<IdentityUser>
                {
                    new IdentityUser { UserName = yonetici.Eposta, Email = yonetici.Eposta },
                    new IdentityUser { UserName = uretici.Eposta, Email = uretici.Eposta },
                    new IdentityUser { UserName = depo.Eposta, Email = depo.Eposta },
                    new IdentityUser { UserName = lojistik.Eposta, Email = lojistik.Eposta },
                    new IdentityUser { UserName = market.Eposta, Email = market.Eposta },
                    new IdentityUser { UserName = tuketici.Eposta, Email = tuketici.Eposta }
                };

                var passwords = new List<string>
                {
                    "Yonetici123!", "Uretici123!", "Depo123!",
                    "Lojistik123!", "Market123!", "Tuketici123!"
                };

                var roles = new List<string>
                {
                    "Yonetici", "Uretici", "Depo",
                    "Lojistik", "Market", "Tuketici"
                };

                for (int i = 0; i < users.Count; i++)
                {
                    var result = await userManager.CreateAsync(users[i], passwords[i]);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(users[i], roles[i]);
                    }
                }

                var tasit1 = new Tasit
                {
                    PlakaNo = "01 ABC 123",
                    SoforAdi = "Mehmet Kaya",
                    SoforTelefon = "0533 777 7777",
                    AracTipi = "Kamyon",
                    Kapasite = 5000,
                    SigortaNo = "SIG-001",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Tasitler.Add(tasit1);

                var tasit2 = new Tasit
                {
                    PlakaNo = "34 XYZ 567",
                    SoforAdi = "Ali Yildiz",
                    SoforTelefon = "0534 888 8888",
                    AracTipi = "Tir",
                    Kapasite = 20000,
                    SigortaNo = "SIG-002",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Tasitler.Add(tasit2);

                var tasit3 = new Tasit
                {
                    PlakaNo = "16 ABC 789",
                    SoforAdi = "Huseyin Oz",
                    SoforTelefon = "0535 999 9999",
                    AracTipi = "Minibus",
                    Kapasite = 1500,
                    SigortaNo = "SIG-003",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };
                context.Tasitler.Add(tasit3);

                await context.SaveChangesAsync();

                if (!context.Urunler.Any())
                {
                    var urunler = new List<Urun>
                    {
                        new Urun { Ad = "Domates", Kategori = "Sebze", Aciklama = "Taze domates", UreticiId = 2, Durum = "Uretimde", OlusturmaTarihi = DateTime.Now },
                        new Urun { Ad = "Salatalık", Kategori = "Sebze", Aciklama = "Taze salatalık", UreticiId = 2, Durum = "Uretimde", OlusturmaTarihi = DateTime.Now },
                        new Urun { Ad = "Elma", Kategori = "Meyve", Aciklama = "Kırmızı elma", UreticiId = 2, Durum = "Uretimde", OlusturmaTarihi = DateTime.Now },
                        new Urun { Ad = "Süt", Kategori = "Süt", Aciklama = "Taze inek sütü", UreticiId = 2, Durum = "Uretimde", OlusturmaTarihi = DateTime.Now },
                        new Urun { Ad = "Yumurta", Kategori = "Yumurta", Aciklama = "Köy yumurtası", UreticiId = 2, Durum = "Uretimde", OlusturmaTarihi = DateTime.Now },
                        new Urun { Ad = "Buğday", Kategori = "Tahıl", Aciklama = "Ekmeklik buğday", UreticiId = 2, Durum = "Uretimde", OlusturmaTarihi = DateTime.Now }
                    };
                    context.Urunler.AddRange(urunler);
                    await context.SaveChangesAsync();
                }

                if (!context.BlokZincirKayitlari.Any())
                {
                    var ilkKayit = new BlokZincirKayit
                    {
                        OncekiHash = "0000000000000000000000000000000000000000000000000000000000000000",
                        MevcutHash = HashSifre("ILKKAYIT" + DateTime.Now.Ticks.ToString()),
                        IslemTipi = "Genesis",
                        IslemIcerigi = "Blok zincir sistemi baslatildi",
                        ZamanDamgasi = DateTime.Now,
                        Nonce = 0,
                        OlusturmaTarihi = DateTime.Now
                    };
                    context.BlokZincirKayitlari.Add(ilkKayit);
                    await context.SaveChangesAsync();
                }

                if (!context.Ayar.Any())
                {
                    var ayarlar = new List<Ayar>
                    {
                        new Ayar { Anahtar = "SirketAdi", Deger = "FoodTrustChain", Aciklama = "Sistem adi", AktifMi = true, OlusturmaTarihi = DateTime.Now },
                        new Ayar { Anahtar = "BlokZincirZorluk", Deger = "4", Aciklama = "Madenci zorluk derecesi", AktifMi = true, OlusturmaTarihi = DateTime.Now },
                        new Ayar { Anahtar = "UrunKategorileri", Deger = "Et,Sut,Sebze,Meyve,Tahil,Yumurta", Aciklama = "Ürün kategorileri", AktifMi = true, OlusturmaTarihi = DateTime.Now }
                    };
                    context.Ayar.AddRange(ayarlar);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}