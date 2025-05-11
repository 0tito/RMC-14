using Robust.Shared.Map;

namespace Content.Shared._RMC14.AbilityCasting;

public sealed class DrawEvent(EntityUid uid, MapCoordinates center, float radius) : EntityEventArgs
{
    public EntityUid Uid = uid;
    public MapCoordinates Position = center;
    public float Radius = radius;
}
