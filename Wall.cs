using Microsoft.Xna.Framework;

namespace VertexTest;

public class Wall 
{
    public int X, Y, Width, Height;
    public Color Color = Color.White;
    public Wall(int x, int y, int width, int height) 
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }
}