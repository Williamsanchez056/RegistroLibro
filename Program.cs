using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Services;
using RegistroLibro.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<RegistroContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConStr")
        ?? throw new InvalidOperationException(
            "Falta configurar la conexion ConStr.")));

builder.Services.AddScoped<LibroService>();
builder.Services.AddScoped<EstudianteService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();