namespace Content.Client._RMC14.AbilityCasting;

[RegisterComponent]
public sealed partial class CastingDebugComponent : Component
{
    public enum ShapeCasted
    {
        Circle,
        Rectangle
    }

    [DataField("shape_casted")]
    public ShapeCasted? CastedShape;

    public float Radius = 5;
}
