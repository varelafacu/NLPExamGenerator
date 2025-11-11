using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NLPExamGenerator.Entidades;
using NLPExamGenerator.Entidades.EF;
using NLPExamGenerator.Logica;
using NLPExamGenerator.Logica.Models;
using NLPExamGenerator.Logica.Services;
using QuestPDF;
using QuestPDF.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

// --------------------
// Configurar servicios
// --------------------

// MVC
builder.Services.AddControllersWithViews();

// DbContext con SQL Server
// Detectar el sistema operativo
var connectionString = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : builder.Configuration.GetConnectionString("SQLiteConnection");

builder.Services.AddDbContext<NLPExamGeneratorContext>(options =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Registrar repositorio y servicio de usuarios
/*builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();*/

builder.Services.AddScoped<IUsuarioLogica, UsuarioLogica>();
builder.Services.AddScoped<IExamLogica, ExamLogica>();

// OpenAI
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();

// PDF
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
Settings.License = LicenseType.Community;

// Sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index"; // o la ruta que elijas
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
});


var app = builder.Build();

// --------------------
// Middleware
// --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
