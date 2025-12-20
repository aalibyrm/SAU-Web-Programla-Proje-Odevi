using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonu.Models;

namespace SporSalonu.Data
{
    public class UygulamaDbContext : IdentityDbContext<Uye>
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options)
        {
        }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<AntrenorHizmet> AntrenorHizmetleri { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Hizmet>()
                .HasOne(h => h.Salon)
                .WithMany(s => s.Hizmetler)
                .HasForeignKey(h => h.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Antrenor>()
                .HasOne(a => a.Salon)
                .WithMany(s => s.Antrenorler)
                .HasForeignKey(a => a.SalonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Antrenor)
                .WithMany(a => a.AntrenorHizmetleri)
                .HasForeignKey(ah => ah.AntrenorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Hizmet)
                .WithMany(h => h.AntrenorHizmetleri)
                .HasForeignKey(ah => ah.HizmetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Randevu>()
                .HasOne(r => r.Uye)
                .WithMany(u => u.Randevular)
                .HasForeignKey(r => r.UyeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Antrenor)
                .WithMany(a => a.Randevular)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            TohumVerileriEkle(builder);
        }

        private void TohumVerileriEkle(ModelBuilder builder)
        {
            builder.Entity<Salon>().HasData(
                new Salon
                {
                    Id = 1,
                    Ad = "Gold Fitness",
                    Adres = "Sakarya Üniversitesi Kampüsü, Esentepe",
                    Telefon = "0264 123 45 67",
                    AcilisSaati = new TimeSpan(7, 0, 0),
                    KapanisSaati = new TimeSpan(23, 0, 0),
                    AktifMi = true
                }
            );

            builder.Entity<Hizmet>().HasData(
                new Hizmet { Id = 1, Ad = "Fitness", Aciklama = "Kişiye özel fitness antrenmanı", SureDakika = 60, Ucret = 250, SalonId = 1, AktifMi = true },
                new Hizmet { Id = 2, Ad = "Yoga", Aciklama = "Rahatlama ve esneklik için yoga dersi", SureDakika = 45, Ucret = 200, SalonId = 1, AktifMi = true },
                new Hizmet { Id = 3, Ad = "Pilates", Aciklama = "Core kasları güçlendirme", SureDakika = 50, Ucret = 220, SalonId = 1, AktifMi = true },
                new Hizmet { Id = 4, Ad = "Kickboks", Aciklama = "Yüksek tempolu dövüş sporu antrenmanı", SureDakika = 60, Ucret = 280, SalonId = 1, AktifMi = true },
                new Hizmet { Id = 5, Ad = "Kilo Verme Programı", Aciklama = "Özel diyet ve egzersiz kombinasyonu", SureDakika = 90, Ucret = 350, SalonId = 1, AktifMi = true }
            );

            builder.Entity<Antrenor>().HasData(
                new Antrenor
                {
                    Id = 1,
                    Ad = "Ahmet",
                    Soyad = "Yılmaz",
                    Eposta = "ahmet@goldfitness.com",
                    Telefon = "0532 111 22 33",
                    UzmanlikAlanlari = "Fitness, Kilo Verme",
                    MesaiBaslangic = new TimeSpan(9, 0, 0),
                    MesaiBitis = new TimeSpan(18, 0, 0),
                    SalonId = 1,
                    AktifMi = true
                },
                new Antrenor
                {
                    Id = 2,
                    Ad = "Zeynep",
                    Soyad = "Kaya",
                    Eposta = "zeynep@goldfitness.com",
                    Telefon = "0533 222 33 44",
                    UzmanlikAlanlari = "Yoga, Pilates",
                    MesaiBaslangic = new TimeSpan(10, 0, 0),
                    MesaiBitis = new TimeSpan(19, 0, 0),
                    SalonId = 1,
                    AktifMi = true
                },
                new Antrenor
                {
                    Id = 3,
                    Ad = "Murat",
                    Soyad = "Demir",
                    Eposta = "murat@goldfitness.com",
                    Telefon = "0534 333 44 55",
                    UzmanlikAlanlari = "Kickboks, Fitness",
                    MesaiBaslangic = new TimeSpan(12, 0, 0),
                    MesaiBitis = new TimeSpan(21, 0, 0),
                    SalonId = 1,
                    AktifMi = true
                }
            );

            builder.Entity<AntrenorHizmet>().HasData(
                new AntrenorHizmet { Id = 1, AntrenorId = 1, HizmetId = 1 },
                new AntrenorHizmet { Id = 2, AntrenorId = 1, HizmetId = 5 },
                new AntrenorHizmet { Id = 3, AntrenorId = 2, HizmetId = 2 },
                new AntrenorHizmet { Id = 4, AntrenorId = 2, HizmetId = 3 },
                new AntrenorHizmet { Id = 5, AntrenorId = 3, HizmetId = 4 },
                new AntrenorHizmet { Id = 6, AntrenorId = 3, HizmetId = 1 }
            );
        }
    }
}
