using Microsoft.Xna.Framework;

namespace VertexTest;


public class Light 
{
    public Vector2 Position;
    public float Size = 64;
    public Color Color = Color.White;
    public float Direction;
    public float Fov = 360;
    public float Strength = -2;

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
