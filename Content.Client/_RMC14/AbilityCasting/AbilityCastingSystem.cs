using Content.Shared._RMC14.AbilityCasting;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Client._RMC14.AbilityCasting;

/// <summary>
/// Idea of this is to have lots of functions that can be used as a base for abilities
/// </summary>
public sealed class AbilityCastingSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IOverlayManager _overlays = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        _overlays.AddOverlay(new AreaOverlays());
        SubscribeLocalEvent<CastingDebugComponent,DrawEvent>(OnDraw);
    }

    private void OnDraw(Entity<CastingDebugComponent> ent, ref DrawEvent args)
    {
        Logger.Debug("OnDraw");
        if (!TryComp<CastingDebugComponent>(args.Uid , out var component))
            return;
        Logger.Debug($"Changing on uid: {args.Uid}");
        component.CastedShape = CastingDebugComponent.ShapeCasted.Circle;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlays.RemoveOverlay<AreaOverlays>();
    }


}
