

using Base_Asp_Core_MVC_with_Identity.CommonFile.IServiceCommon;
using Base_Asp_Core_MVC_with_Identity.CommonFile.ServiceCommon;
using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnectionMySQL") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnectionMySQL' not found.");

builder.Services.AddDbContext<Base_Asp_Core_MVC_with_IdentityContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    //options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
    //options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), b =>
    //{
    //    b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
    //});
});

//builder.Services.AddScoped(typeof(ICommonService<>), typeof(CommonService<>));
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddDefaultIdentity<UserSystemIdentity>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
           .AddEntityFrameworkStores<Base_Asp_Core_MVC_with_IdentityContext>()
            .AddDefaultUI()
           .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<Base_Asp_Core_MVC_with_IdentityContext>();
        var userManager = services.GetRequiredService<UserManager<UserSystemIdentity>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await ContextSeed.SeedRolesAsync(userManager, roleManager);
        await ContextSeed.SeedSuperAdminAsync(userManager, roleManager);

    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
