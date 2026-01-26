using Dork.Web.Components;
using Dork.Engine.Commands;
using Dork.Engine.Game;
using Dork.Engine.Model;
using Dork.Engine.Systems;
using Dork.Engine.World;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// World + state
builder.Services.AddScoped<Func<World>>(_ => () => WorldFactory.CreateDemoWorld());
builder.Services.AddScoped<Func<GameState>>(_ => () => new GameState(startingRoomId: 1));

// Options
builder.Services.AddScoped<GameOptions>(_ => new GameOptions { ShowExits = true });

// Core services
builder.Services.AddScoped<MovementService>();

// Command handlers
builder.Services.AddScoped<ICommandHandler, IntroCommands>();
builder.Services.AddScoped<ICommandHandler, ClassGateCommands>();
builder.Services.AddScoped<ICommandHandler, MovementCommands>();
builder.Services.AddScoped<ICommandHandler, InventoryCommands>();
builder.Services.AddScoped<ICommandHandler, ExamineCommands>();
builder.Services.AddScoped<ICommandHandler, HelpCommands>();
builder.Services.AddScoped<ICommandHandler, FallbackCommands>();
builder.Services.AddScoped<ICommandHandler, PhoneCommands>();
builder.Services.AddScoped<ICommandHandler, SecurityCommands>();
builder.Services.AddScoped<ICommandHandler, PushCommands>();
builder.Services.AddScoped<ICommandHandler, ListenCommands>();
//builder.Services.AddScoped<ICommandHandler, WaitCommands>();
//builder.Services.AddScoped<ICommandHandler, PokeCommands>();
//builder.Services.AddScoped<ICommandHandler, EatCommands>();
//builder.Services.AddScoped<ICommandHandler, HideCommands>();
builder.Services.AddScoped<ICommandHandler, SaveCommands>();
builder.Services.AddScoped<ICommandHandler, NewGameCommands>();

builder.Services.AddSingleton<ISaveService>(sp => new FileSaveService("save.json"));

// Router
builder.Services.AddScoped<ICommandRouter>(sp =>
{
    var handlers = sp.GetServices<ICommandHandler>();
    return new CommandRouter(handlers);
});

// Turn systems
builder.Services.AddScoped<ITurnSystem, IdSystem>();
builder.Services.AddScoped<ITurnSystem, NoiseSystem>();
builder.Services.AddScoped<ITurnSystem, GuardSystem>();
builder.Services.AddScoped<ITurnSystem, BatterySystem>();

// Turn pipeline
builder.Services.AddScoped(sp =>
{
    var systems = sp.GetServices<ITurnSystem>();
    return new TurnPipeline(systems);
});

// Game
builder.Services.AddScoped<DorkGame>(sp =>
{
    var worldFactory = sp.GetRequiredService<Func<World>>();
    var stateFactory = sp.GetRequiredService<Func<GameState>>();
    var options = sp.GetRequiredService<GameOptions>();
    var router = sp.GetRequiredService<ICommandRouter>();
    var pipeline = sp.GetRequiredService<TurnPipeline>();
    var savegame = sp.GetRequiredService<ISaveService>();

    return new DorkGame(
                        worldFactory,
                        stateFactory,
                        options,
                        router,
                        pipeline,
                        savegame
                        );
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
