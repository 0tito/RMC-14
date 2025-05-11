using System.Numerics;
using Robust.Shared.Map;

namespace Content.Shared._RMC14.AbilityCasting;

public sealed class RectangleArea
{
    public float Height;
    public float Width;
    public MapCoordinates Position;
    public Angle Angle = Angle.Zero;


    public void SetMapCoordsToRectangleCenter(ref RectangleArea rectangle)
    {
        MapCoordinates RectangleCenter = rectangle.Position;
        RectangleCenter.Offset(rectangle.Width/2, rectangle.Height/2);
    }

    public void RotateRectangle(ref RectangleArea rectangle, Angle angle)
    {
        rectangle.Angle += angle;
    }
}
