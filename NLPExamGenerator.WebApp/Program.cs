using Microsoft.EntityFrameworkCore;
using NLPExamGenerator.Entidades;
using NLPExamGenerator.Entidades.EF;
using NLPExamGenerator.Logica;
var builder = WebApplication.CreateBuilder(args);

// --------------------
// Configurar servicios
// --------------------

// MVC
builder.Services.AddControllersWithViews();

// DbContext con SQL Server
builder.Services.AddDbContext<NLPExamGeneratorContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está definida en appsettings.json");
    }
    options.UseSqlServer(connectionString);
});

// Registrar repositorio y servicio de usuarios
/*builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();*/

builder.Services.AddScoped<IUsuarioLogica, UsuarioLogica>();

// Sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
app.UseAuthorization();

// Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
