using Dork.Web.Components;
using Dork.Engine.Game;
using Dork.Engine.Model;
using Dork.Engine.World;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(sp =>
{
    var world = WorldFactory.CreateDemoWorld();
    var state = new GameState(startingRoomId: 1);
    return new DorkGame(world, state);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
