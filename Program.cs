using Microsoft.EntityFrameworkCore;
using RegistroLibro.Context;
using RegistroLibro.Services;
using RegistroLibro.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextFactory<RegistroContext>(options =>
    options.UseSqlite("Data Source=RegistroLibros.db"));

builder.Services.AddScoped<LibroService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
