using Dork.Engine.Model;
using Dork.Engine.Systems;
using Dork.Engine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Dork.Engine.Game;

public sealed class DorkGame
{
    private World.World _world;
    private GameState _state;
    private readonly GameOptions _options;
    private readonly ICommandRouter _router;
    private readonly TurnPipeline _turnPipeline;
    private readonly Random _rng = new();
    private readonly MovementService _movement = new();
    private readonly ISaveService _saveService;

    private readonly Func<World.World> _worldFactory;
    private readonly Func<GameState> _stateFactory;

    public DorkGame(
        Func<World.World> worldFactory,
        Func<GameState> stateFactory,
        GameOptions options,
        ICommandRouter router,
        TurnPipeline turnPipeline,
        ISaveService saveService)
    {
        _worldFactory = worldFactory ?? throw new ArgumentNullException(nameof(worldFactory));
        _stateFactory = stateFactory ?? throw new ArgumentNullException(nameof(stateFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _turnPipeline = turnPipeline ?? throw new ArgumentNullException(nameof(turnPipeline));
        _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        _world = _worldFactory() ?? throw new InvalidOperationException("World factory returned null.");
        _state = _stateFactory() ?? throw new InvalidOperationException("GameState factory returned null.");
    }

    public GameOutput Execute(string input)
    {
        var raw = input ?? "";
        var normalized = InputNormalizer.Normalize(raw);

        var ctx = new GameContext(_world, _state, _options, _rng, _movement, _saveService);
        ctx.Turn.RawInput = raw;
        ctx.Turn.NormalizedInput = normalized;

        // 2) Route command
        var result = _router.Route(normalized, ctx);

        // 3) Apply systems
        result = _turnPipeline.Run(ctx, result);

        if (_state.NewGameRequested)
        {
            _world = _worldFactory() ?? throw new InvalidOperationException("World factory returned null.");
            _state = _stateFactory() ?? throw new InvalidOperationException("GameState factory returned null.");

            _state.NewGameRequested = false; // <- clear it

            var newCtx = new GameContext(_world, _state, _options, _rng, _movement, _saveService);
            newCtx.Turn.RawInput = "look";
            newCtx.Turn.NormalizedInput = "look";

            var look = _router.Route("look", newCtx);
            return result.Append("\n").Append(look.Text);
        }

        return result;
    }
}

// quality-of-life: append text without rewriting your whole pipeline
public static class GameOutputExtensions
{
    public static GameOutput Append(this GameOutput output, string extra)
        => new GameOutput(
            output.Text + extra,
            output.Kind,
            output.Code
        );

    public static GameOutput AppendError(this GameOutput output, string extra, string code = "APPEND_ERR")
    => new GameOutput(
        output.Text + extra,
        OutputKind.Error,
        code
    );
}
