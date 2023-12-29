using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VertexTest;

public class LightingRenderer 
{
    public List<Light> Lights = new();
    public List<Wall> Walls = new();
    private GraphicsDevice graphicsDevice;
    private Effect lightEffect;
    private Effect shadowEffect;

    private QuadRenderComponent component;
    private RenderTarget2D surface;
    public RenderTarget2D lightTarget;

    public LightingRenderer(GraphicsDevice graphicsDevice, RenderTarget2D surface) 
    {
        this.graphicsDevice = graphicsDevice;
        // Global light is here, just to fix a weird shadow bug..
        Lights.Add(new GlobalLight());
        this.surface = surface;

        component = new QuadRenderComponent(graphicsDevice);

        lightTarget = new RenderTarget2D(graphicsDevice, 320, 180);
    }

    public void LoadContent(ContentManager content) 
    {
        lightEffect = content.Load<Effect>("light.fxb");
        shadowEffect = content.Load<Effect>("shadow.fxb");
        component.SetEffect(shadowEffect);

        Vector2 center = new Vector2(320 * 0.5f, 180 * 0.5f);
        var view = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));
        var projection = Matrix.CreateOrthographic(center.X * 2, center.Y * 2, -0.5f, 1);

        lightEffect.Parameters["view"].SetValue(view);
        lightEffect.Parameters["projection"].SetValue(projection);
    }

    private bool pressed;

    public void Update() 
    {
        if (Mouse.GetState().LeftButton == ButtonState.Pressed) 
        {
            if (pressed) 
            {
                return;
            }
            pressed = true;
            Lights.Add(new Light(Mouse.GetState().X, Mouse.GetState().Y));
            return;
        }
        pressed = false;
    }

    public void Render(SpriteBatch spriteBatch) 
    {
        component.Begin();
        for (int j = 0; j < Walls.Count; j++) 
        {
            var wall = Walls[j];
            var offsetX = wall.X;
            var offsetY = wall.Y;
            var offsetW = wall.Width;
            var offsetH = wall.Height;
            component.DrawVertexQuad(offsetX, offsetY, offsetX + offsetW, offsetY + offsetH, Color.White);
            component.DrawVertexQuad(offsetX + offsetW, offsetY, offsetX, offsetY + offsetH, Color.White);
        }

        var lightBlend = new BlendState();
        lightBlend.ColorSourceBlend = Blend.InverseDestinationAlpha;
        lightBlend.ColorDestinationBlend = Blend.One;
        lightBlend.AlphaSourceBlend = Blend.Zero;
        lightBlend.AlphaDestinationBlend = Blend.Zero;

        var shadowBlend = new BlendState();
        shadowBlend.ColorSourceBlend = Blend.Zero;
        shadowBlend.ColorDestinationBlend = Blend.One;
        shadowBlend.AlphaSourceBlend = Blend.One;
        shadowBlend.AlphaDestinationBlend = Blend.One;

        graphicsDevice.SetRenderTarget(lightTarget);
        graphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin();
        spriteBatch.Draw(surface, Vector2.Zero, new Rectangle(0, 0, 320, 180), Color.White * 0.2f);
        spriteBatch.End();


        for (int i = 0; i < Lights.Count; i++) 
        {
            if (i == 0) 
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, lightBlend,
                SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

                lightEffect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.End();
                continue;
            }


            var light = Lights[i];
            lightEffect.Parameters["ps_pos"].SetValue(light.Position);
            lightEffect.Parameters["size"].SetValue(light.Size);
            lightEffect.Parameters["dir"].SetValue(light.Direction);
            lightEffect.Parameters["fov"].SetValue(light.Fov);
            lightEffect.Parameters["ps_str"].SetValue(light.Strength);
            shadowEffect.Parameters["vs_pos"].SetValue(light.Position);
            graphicsDevice.BlendState = shadowBlend;

            component.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, lightBlend,
            SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            lightEffect.CurrentTechnique.Passes[0].Apply();

            // spriteBatch.Draw(surface, Vector2.Zero, new Rectangle(0, 0, 320, 180), light.Color);
            spriteBatch.Draw(Game1.PixelTexture, Vector2.Zero, new Rectangle(0, 0, 320, 180), light.Color);
            spriteBatch.End();
        }
        component.Reset();
        graphicsDevice.BlendState = BlendState.AlphaBlend;
    }
}