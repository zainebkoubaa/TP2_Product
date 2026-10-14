using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TP_Product.Models;
using TP_Product.Models.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuration de la base de données avec EF Core
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDBConnection")));

// Configuration d'ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Règles des mots de passe (assouplies)
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
});

// Injection des dépendances des Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategorieRepository, CategoryRepository>();

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

app.UseAuthentication();   // avant UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Création automatique des rôles, du compte Admin et des catégories
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    foreach (var roleName in new[] { "Admin", "Manager", "User" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    var admin = await userManager.FindByEmailAsync("admin1@gmail.com");
    if (admin == null)
    {
        admin = new IdentityUser { UserName = "admin1@gmail.com", Email = "admin1@gmail.com" };
        await userManager.CreateAsync(admin, "Admin123");
    }
    if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Catégories (ajoutées seulement si elles n'existent pas déjà)
    var categories = new[]
    {
        "Alimentaire", "Hygiène", "Crèmerie et Produits Laitiers",
        "Boissons", "Boulangerie et Pâtisserie", "Fruits et Légumes",
        "Viandes et Charcuterie", "Poissons et Conserves", "Épicerie sucrée",
        "Surgelés", "Entretien et Nettoyage", "Bébé et Maternité",
        "Beauté et Parfumerie"
    };
    foreach (var name in categories)
    {
        if (!db.Categories.Any(c => c.CategoryName == name))
            db.Categories.Add(new Category { CategoryName = name });
    }
    db.SaveChanges();
}

app.Run();