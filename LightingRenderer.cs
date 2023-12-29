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

    private QuadRenderComponent quad;
    private RenderTarget2D surface;
    public RenderTarget2D LightTarget;

    private BlendState blendLight;
    private BlendState blendShadow;

    public LightingRenderer(GraphicsDevice graphicsDevice, RenderTarget2D surface) 
    {
        this.graphicsDevice = graphicsDevice;
        // Global light is here, just to fix a weird shadow bug..
        Lights.Add(new GlobalLight());
        this.surface = surface;

        quad = new QuadRenderComponent(graphicsDevice);

        LightTarget = new RenderTarget2D(graphicsDevice, 320, 180);

        blendLight = new BlendState();
        blendLight.ColorSourceBlend = Blend.InverseDestinationAlpha;
        blendLight.ColorDestinationBlend = Blend.One;
        blendLight.AlphaSourceBlend = Blend.Zero;
        blendLight.AlphaDestinationBlend = Blend.Zero;

        blendShadow = new BlendState();
        blendShadow.ColorSourceBlend = Blend.Zero;
        blendShadow.ColorDestinationBlend = Blend.One;
        blendShadow.AlphaSourceBlend = Blend.One;
        blendShadow.AlphaDestinationBlend = Blend.One;
    }

    public void LoadContent(ContentManager content) 
    {
        lightEffect = content.Load<Effect>("light.fxb");
        shadowEffect = content.Load<Effect>("shadow.fxb");
        quad.SetEffect(shadowEffect);

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
        quad.Begin();
        for (int j = 0; j < Walls.Count; j++) 
        {
            var wall = Walls[j];
            var offsetX = wall.X;
            var offsetY = wall.Y;
            var offsetW = wall.Width;
            var offsetH = wall.Height;
            quad.DrawQuad(offsetX, offsetY, offsetX + offsetW, offsetY + offsetH, Color.White);
            quad.DrawQuad(offsetX + offsetW, offsetY, offsetX, offsetY + offsetH, Color.White);
        }


        graphicsDevice.SetRenderTarget(LightTarget);
        graphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin();
        spriteBatch.Draw(surface, Vector2.Zero, new Rectangle(0, 0, 320, 180), Color.White * 0.2f);
        spriteBatch.End();


        for (int i = 0; i < Lights.Count; i++) 
        {
            if (i == 0) 
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, blendLight,
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
            graphicsDevice.BlendState = blendShadow;

            quad.Flush();

            spriteBatch.Begin(SpriteSortMode.Immediate, blendLight,
            SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            lightEffect.CurrentTechnique.Passes[0].Apply();

            // spriteBatch.Draw(surface, Vector2.Zero, new Rectangle(0, 0, 320, 180), light.Color);
            spriteBatch.Draw(Game1.PixelTexture, Vector2.Zero, new Rectangle(0, 0, 320, 180), light.Color);
            spriteBatch.End();
        }
        quad.End();
        graphicsDevice.BlendState = BlendState.AlphaBlend;
    }
}