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
    private readonly World.World _world;
    private readonly GameState _state;
    private readonly GameOptions _options;
    private readonly ICommandRouter _router;
    private readonly TurnPipeline _turnPipeline;
    private readonly Random _rng = new();
    private readonly MovementService _movement = new();

    public DorkGame(World.World world,
                    GameState state,
                    GameOptions options,
                    ICommandRouter router,
                    TurnPipeline turnPipeline)
    {
        _world = world;
        _state = state;
        _options = options;
        _router = router;
        _turnPipeline = turnPipeline;
    }

    public GameOutput Execute(string input)
    {
        var raw = input ?? "";
        var normalized = InputNormalizer.Normalize(raw);

        var ctx = new GameContext(_world, _state, _options, _rng, _movement);
        ctx.Turn.RawInput = raw;
        ctx.Turn.NormalizedInput = normalized;

        // 2) Route command
        var result = _router.Route(normalized, ctx);

        // 3) Apply systems
        result = _turnPipeline.Run(ctx, result);

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
