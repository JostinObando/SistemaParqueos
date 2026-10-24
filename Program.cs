using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar repositorios PRQ
builder.Services.AddDbContext<SistemaParqueos.Data.ParqueosContext>(
    opciones => opciones.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgreSQL")));

var origenDatos = builder.Configuration["OrigenDatos"] ?? "Json";

if (origenDatos.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<
        SistemaParqueos.Repositories.Interfaces.IAutomovilRepository,
        SistemaParqueos.Repositories.BaseDatos.AutomovilRepositoryBD>();

    builder.Services.AddScoped<
        SistemaParqueos.Repositories.Interfaces.IParqueoRepository,
        SistemaParqueos.Repositories.BaseDatos.ParqueoRepositoryBD>();

    builder.Services.AddScoped<
        SistemaParqueos.Repositories.Interfaces.IIngresoRepository,
        SistemaParqueos.Repositories.BaseDatos.IngresoRepositoryBD>();
}
else
{
    builder.Services.AddScoped<
        SistemaParqueos.Repositories.Interfaces.IAutomovilRepository,
        SistemaParqueos.Repositories.Json.AutomovilRepositoryJson>();

    builder.Services.AddScoped<
        SistemaParqueos.Repositories.Interfaces.IParqueoRepository,
        SistemaParqueos.Repositories.Json.ParqueoRepositoryJson>();

    builder.Services.AddScoped<
        SistemaParqueos.Repositories.Interfaces.IIngresoRepository,
        SistemaParqueos.Repositories.Json.IngresoRepositoryJson>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

