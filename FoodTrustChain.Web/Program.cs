using FoodTrustChain.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.EnsureCreated();
        await DbSeeder.SeedDataAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabani migrasyonu veya seed data yüklenirken hata olustu.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/PageNotFound");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.Use(async (context, next) =>
{
    await next();

    if (!context.Response.HasStarted && !context.Request.Path.StartsWithSegments("/Error"))
    {
        if (context.Response.StatusCode == 404)
        {
            context.Response.Redirect("/Error/PageNotFound");
        }
        else if (context.Response.StatusCode == 403)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.Redirect("/Identity/Account/Login");
            }
            else
            {
                context.Response.Redirect("/Error/PageUnauthorized");
            }
        }
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "yonetici",
    pattern: "Yonetici/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "error404",
    pattern: "Error/PageNotFound",
    defaults: new { controller = "Error", action = "PageNotFound" });

app.MapControllerRoute(
    name: "error403",
    pattern: "Error/PageUnauthorized",
    defaults: new { controller = "Error", action = "PageUnauthorized" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AnaSayfa}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();