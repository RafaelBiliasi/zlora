using Microsoft.AspNetCore.Identity;
using Zlora.Data;
using Zlora.Models;
using Zlora.Seeds;
using Zlora.Services;
using Zlora.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddSingleton<ContextMongoDb>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailService>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(30));
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
}).AddMongoDbStores<ApplicationUser, ApplicationRole, string>(
    builder.Configuration["MongoConnection:ConnectionString"], builder.Configuration["MongoConnection:Database"])
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Contas/Login";
    options.AccessDeniedPath = "/Contas/AcessoNegado";
    options.Cookie.Name = "Zlora.CSharp";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});
// Revalida o perfil em cada requisição após uma alteração pelo administrador.
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ContextMongoDb>().CriarIndices();
    await IdentitySeeder.SeedRoles(scope.ServiceProvider);
    await DemoDataSeeder.Seed(scope.ServiceProvider.GetRequiredService<ContextMongoDb>());
}
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Erro");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
