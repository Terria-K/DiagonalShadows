using Microsoft.Xna.Framework;

namespace VertexTest;

public class Light 
{
    public Vector2 Position;
    public float Size = 0.4f;
    public float Intensity = 32f;
    public Color Color = Color.White;
    public float Direction;
    public float Fov = 360;
    public float Strength = -1;

    public Light(int x, int y) 
    {
        Position = new Vector2(x, y);
    }
}
internal class GlobalLight : Light
{
    public GlobalLight() : base(0, 0)
    {
    }
}
