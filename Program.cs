using FitnessCenter.Data;
using FitnessCenter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Entity Framework ve SQL Server yapılandırması
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity yapılandırması
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre gereksinimleri (ödev gereği "sau" şifresi için gevşetildi)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;

    // Kullanıcı gereksinimleri
    options.User.RequireUniqueEmail = true;

    // Kilitleme ayarları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Veritabanı ve başlangıç verilerini oluştur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Veritabanını oluştur
        context.Database.EnsureCreated();

        // Rolleri oluştur
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("Member"))
        {
            await roleManager.CreateAsync(new IdentityRole("Member"));
        }

        // Admin kullanıcısını oluştur
        var adminEmail = "g221210058@sakarya.edu.tr";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(adminUser, "sau");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Örnek verileri oluştur
        if (!context.Gyms.Any())
        {
            var gym = new Gym
            {
                Name = "FitLife Spor Salonu",
                Address = "Sakarya Üniversitesi Kampüsü, Esentepe Mah.",
                PhoneNumber = "0264 295 50 00",
                Email = "info@fitlife.com",
                Description = "Modern ekipmanlar ve uzman kadro ile hizmetinizdeyiz.",
                OpeningTime = new TimeSpan(6, 0, 0),
                ClosingTime = new TimeSpan(23, 0, 0),
                IsActive = true
            };
            context.Gyms.Add(gym);
            await context.SaveChangesAsync();

            // Hizmetleri ekle
            var services_list = new List<Service>
            {
                new Service { Name = "Kişisel Antrenman", Description = "Birebir antrenör eşliğinde kişiselleştirilmiş antrenman", DurationMinutes = 60, Price = 500, Category = ServiceCategory.PersonalTraining, GymId = gym.Id },
                new Service { Name = "Yoga Dersi", Description = "Profesyonel yoga eğitmeni ile grup dersi", DurationMinutes = 60, Price = 200, Category = ServiceCategory.Yoga, GymId = gym.Id },
                new Service { Name = "Pilates", Description = "Mat pilates ve reformer pilates dersleri", DurationMinutes = 55, Price = 250, Category = ServiceCategory.Pilates, GymId = gym.Id },
                new Service { Name = "Kardio Antrenman", Description = "Yüksek tempolu kardio egzersizleri", DurationMinutes = 45, Price = 150, Category = ServiceCategory.Cardio, GymId = gym.Id },
                new Service { Name = "Kickbox", Description = "Kickbox ve dövüş sanatları dersi", DurationMinutes = 60, Price = 300, Category = ServiceCategory.Kickbox, GymId = gym.Id },
                new Service { Name = "Grup Fitness", Description = "Eğlenceli grup fitness dersleri", DurationMinutes = 50, Price = 100, Category = ServiceCategory.GroupClass, GymId = gym.Id }
            };
            context.Services.AddRange(services_list);
            await context.SaveChangesAsync();

            // Antrenörleri ekle
            var trainers = new List<Trainer>
            {
                new Trainer { FirstName = "Ahmet", LastName = "Yılmaz", Email = "ahmet@fitlife.com", PhoneNumber = "0532 111 22 33", Specializations = "Kas Geliştirme, Kuvvet Antrenmanı", Biography = "10 yıllık deneyimli fitness antrenörü", ExperienceYears = 10, GymId = gym.Id },
                new Trainer { FirstName = "Ayşe", LastName = "Kaya", Email = "ayse@fitlife.com", PhoneNumber = "0533 222 33 44", Specializations = "Yoga, Pilates, Esneklik", Biography = "Sertifikalı yoga ve pilates eğitmeni", ExperienceYears = 8, GymId = gym.Id },
                new Trainer { FirstName = "Mehmet", LastName = "Demir", Email = "mehmet@fitlife.com", PhoneNumber = "0534 333 44 55", Specializations = "Kickbox, Kardio, Kilo Verme", Biography = "Ulusal şampiyon kickbox antrenörü", ExperienceYears = 12, GymId = gym.Id }
            };
            context.Trainers.AddRange(trainers);
            await context.SaveChangesAsync();

            // Antrenör-Hizmet ilişkilerini ekle
            var trainerServices = new List<TrainerService>
            {
                new TrainerService { TrainerId = trainers[0].Id, ServiceId = services_list[0].Id },
                new TrainerService { TrainerId = trainers[0].Id, ServiceId = services_list[3].Id },
                new TrainerService { TrainerId = trainers[1].Id, ServiceId = services_list[1].Id },
                new TrainerService { TrainerId = trainers[1].Id, ServiceId = services_list[2].Id },
                new TrainerService { TrainerId = trainers[2].Id, ServiceId = services_list[4].Id },
                new TrainerService { TrainerId = trainers[2].Id, ServiceId = services_list[5].Id }
            };
            context.TrainerServices.AddRange(trainerServices);

            // Antrenör müsaitlik saatlerini ekle
            foreach (var trainer in trainers)
            {
                for (int day = 1; day <= 6; day++) // Pazartesi - Cumartesi
                {
                    context.TrainerAvailabilities.Add(new TrainerAvailability
                    {
                        TrainerId = trainer.Id,
                        DayOfWeek = (DayOfWeek)day,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0),
                        IsActive = true
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata oluştu.");
    }
}

app.Run();
