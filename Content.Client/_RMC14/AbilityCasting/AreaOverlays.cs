using System.Numerics;
using Content.Shared._RMC14.AbilityCasting;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.GameObjects;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._RMC14.AbilityCasting;

public sealed class AreaOverlays : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    private readonly TransformSystem _transform;


    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public AreaOverlays()
    {
        IoCManager.InjectDependencies(this);

        _shader = _prototype.Index<ShaderPrototype>("unshaded").Instance();
        _transform = _entity.System<TransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var player = _players.LocalEntity;
        //Logger.Debug($"Local player: {player}");

        if (!_entity.TryGetComponent<CastingDebugComponent>(player, out var castingDebug))
            return;

        //Logger.Debug($"this is running");
        handle.UseShader(_shader);
        DrawShape(in args);
    }

    private void DrawShape(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        var query = _entity.EntityQueryEnumerator<CastingDebugComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            var pos = _transform.GetMapCoordinates(uid);
            Vector2 coords;
            pos.Deconstruct(out var x, out var y);
            coords = new Vector2(x, y);

            switch (component.CastedShape)
            {
                case CastingDebugComponent.ShapeCasted.Circle:
                    handle.DrawCircle(coords, component.Radius, Color.Red, true);
                    //Logger.Debug($"drawing circle at {coords}, radius: {component.Radius}");
                    break;
                case CastingDebugComponent.ShapeCasted.Rectangle:
                    break;
                default:
                    //Logger.Debug("not implemented");
                    break;
            }
        }
    }
}
