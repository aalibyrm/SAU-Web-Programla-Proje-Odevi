using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SporSalonu.Data;
using SporSalonu.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UygulamaDbContext>(secenekler =>
    secenekler.UseSqlServer(builder.Configuration.GetConnectionString("VarsayilanBaglanti")));

builder.Services.AddIdentity<Uye, IdentityRole>(secenekler =>
{
    secenekler.Password.RequireDigit = true;
    secenekler.Password.RequireLowercase = true;
    secenekler.Password.RequireUppercase = false;
    secenekler.Password.RequireNonAlphanumeric = false;
    secenekler.Password.RequiredLength = 6;
    secenekler.User.RequireUniqueEmail = true;
    secenekler.Lockout.MaxFailedAccessAttempts = 5;
    secenekler.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<UygulamaDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(secenekler =>
{
    secenekler.LoginPath = "/Hesap/Giris";
    secenekler.LogoutPath = "/Hesap/Cikis";
    secenekler.AccessDeniedPath = "/Hesap/ErisimEngellendi";
    secenekler.ExpireTimeSpan = TimeSpan.FromDays(7);
    secenekler.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<UygulamaDbContext>();
        var userManager = services.GetRequiredService<UserManager<Uye>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.Migrate();

        await BaslangicVerileriniOlustur(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu.");
    }
}

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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task BaslangicVerileriniOlustur(UserManager<Uye> userManager, RoleManager<IdentityRole> roleManager)
{
    string[] roller = { "Admin", "Uye" };
    foreach (var rol in roller)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    var adminEmail = "b231210068@sakarya.edu.tr";
    var adminSifre = "sau123";

    var mevcutAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (mevcutAdmin == null)
    {
        var adminKullanici = new Uye
        {
            UserName = adminEmail,
            Email = adminEmail,
            Ad = "Admin",
            Soyad = "Kullanıcı",
            EmailConfirmed = true,
            KayitTarihi = DateTime.Now
        };

        var sonuc = await userManager.CreateAsync(adminKullanici, adminSifre);
        if (sonuc.Succeeded)
        {
            await userManager.AddToRoleAsync(adminKullanici, "Admin");
        }
    }
    else
    {
        // Hesap kilitliyse kilidi aç
        if (await userManager.IsLockedOutAsync(mevcutAdmin))
        {
            await userManager.SetLockoutEndDateAsync(mevcutAdmin, null);
        }
        
        // Şifreyi sıfırla
        var token = await userManager.GeneratePasswordResetTokenAsync(mevcutAdmin);
        await userManager.ResetPasswordAsync(mevcutAdmin, token, adminSifre);
    }
}
