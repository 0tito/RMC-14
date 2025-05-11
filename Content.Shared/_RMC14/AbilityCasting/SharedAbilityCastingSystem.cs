using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Shared._RMC14.AbilityCasting;

public sealed class SharedAbilityCastingSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SharedDebugCastingComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(Entity<SharedDebugCastingComponent> ent, ref ComponentInit args)
    {
        var ev = new DrawEvent(ent, _transform.GetMapCoordinates(ent), 5);
        RaiseLocalEvent(ent, ev);

    }

    public Filter CastCircle(MapCoordinates center, float radius, bool display = true)
    {
        Filter area = Filter.Empty();
        area.AddInRange(center, radius);

        return area;
    }

    public Filter CastCircle(EntityCoordinates center, float radius, bool display = true)
    {
        return CastCircle (_transform.ToMapCoordinates(center), radius, display);
    }

    public Filter CastRectangle(MapCoordinates center, float height, float width, bool display = true)
    {
        Filter hit = Filter.Empty();
        RectangleArea area = new RectangleArea();
        area.SetMapCoordsToRectangleCenter(ref area);

        return hit;
    }
}
