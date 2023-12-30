using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VertexTest;

public class LightingRenderer 
{
    public List<Light> Lights = new();
    public List<Wall> Walls = new();
    private GraphicsDevice graphicsDevice;
    private Effect lightEffect;
    private Effect shadowEffect;
    private Effect blurEffect;
    private Effect envEffect;

    private RenderTarget2D surface;
    private RenderTarget2D paperLight;
    public RenderTarget2D LightTarget;

    private BlendState blendLight;
    private BlendState blendShadow;
    private BlendState blendRenderer;
    private VertexPositionColor[] vertexBuffer;
    private short[] indexBuffer;
    private int shapeCount;
    private int vertexCount;
    private int indexCount;

    public bool EdgeLight;

    public Color EnvironmentColor = Color.White * 0.2f;
    public float Alpha = 1f;
    public Point Size;

    public LightingRenderer(GraphicsDevice graphicsDevice, RenderTarget2D surface, Point size) 
    {
        this.graphicsDevice = graphicsDevice;
        const int MaxCount = 2048;
        vertexBuffer = new VertexPositionColor[MaxCount];
        indexBuffer = new short[MaxCount];
        // Global light is here, just to fix a weird shadow bug..
        Lights.Add(new GlobalLight());
        this.surface = surface;

        Size = size;

        LightTarget = new RenderTarget2D(graphicsDevice, size.X, size.Y);
        paperLight = new RenderTarget2D(graphicsDevice, size.X, size.Y);

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

        blendRenderer = new BlendState();
        blendRenderer.ColorSourceBlend = Blend.Zero;
        blendRenderer.ColorDestinationBlend = Blend.SourceColor;
    }

    public void LoadContent(ContentManager content) 
    {
        lightEffect = content.Load<Effect>("light.fxb");
        shadowEffect = content.Load<Effect>("shadow.fxb");
        blurEffect = content.Load<Effect>("blur.fxb");
        envEffect = content.Load<Effect>("environment.fxb");

        Vector2 center = new Vector2(320 * 0.5f, 180 * 0.5f);
        var view = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));
        var projection = Matrix.CreateOrthographic(center.X * 2, center.Y * 2, -0.5f, 1);

        lightEffect.Parameters["view"].SetValue(view);
        lightEffect.Parameters["projection"].SetValue(projection);
    }

    public void DrawQuad(int x1, int y1, int x2, int y2, Color color) 
    {
        indexBuffer[indexCount]       = (short)(0 + vertexCount);
        indexBuffer[indexCount + 1]   = (short)(1 + vertexCount);
        indexBuffer[indexCount + 2]   = (short)(2 + vertexCount);
        indexBuffer[indexCount + 3]   = (short)(1 + vertexCount);
        indexBuffer[indexCount + 4]   = (short)(2 + vertexCount);
        indexBuffer[indexCount + 5]   = (short)(3 + vertexCount);

        indexCount += 6;

        vertexBuffer[vertexCount]     = new VertexPositionColor(new Vector3(x1, y1, 0), color); 
        vertexBuffer[vertexCount + 1] = new VertexPositionColor(new Vector3(x1, y1, 2), color); 
        vertexBuffer[vertexCount + 2] = new VertexPositionColor(new Vector3(x2, y2, 1), color); 
        vertexBuffer[vertexCount + 3] = new VertexPositionColor(new Vector3(x2, y2, 3), color); 

        vertexCount += 4;
        shapeCount++;
    }


    public void BeforeRender(SpriteBatch spriteBatch, Matrix view, Matrix projection) 
    {
        for (int j = 0; j < Walls.Count; j++) 
        {
            var wall = Walls[j];
            var offsetX = wall.X;
            var offsetY = wall.Y;
            var offsetW = wall.Width;
            var offsetH = wall.Height;
            DrawQuad(offsetX, offsetY, offsetX + offsetW, offsetY + offsetH, Color.White);
            DrawQuad(offsetX + offsetW, offsetY, offsetX, offsetY + offsetH, Color.White);
        }

        graphicsDevice.SetRenderTarget(paperLight);
        graphicsDevice.Clear(Color.Transparent);


        shadowEffect.Parameters["view"].SetValue(view);
        shadowEffect.Parameters["projection"].SetValue(projection);

        spriteBatch.Begin();
        spriteBatch.Draw(surface, Vector2.Zero, new Rectangle(0, 0, Size.X, Size.Y), EnvironmentColor);
        spriteBatch.End();


        for (int i = 0; i < Lights.Count; i++) 
        {
            if (i == 0) 
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, blendLight,
                SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);

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

            if (shapeCount != 0) 
            {
                foreach (var pass in shadowEffect.CurrentTechnique.Passes) 
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, 
                    vertexBuffer, 0, vertexCount, indexBuffer, 0, indexCount / 3);
                }
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, blendLight,
            SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);

            lightEffect.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(Game1.PixelTexture, Vector2.Zero, new Rectangle(0, 0, 320, 180), light.Color);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, 
        DepthStencilState.Default, RasterizerState.CullNone);
        for (int j = 0; j < Walls.Count; j++) 
        {
            var wall = Walls[j];
            spriteBatch.Draw(Game1.PixelTexture, new Vector2(wall.X, wall.Y), 
                new Rectangle(0, 0, wall.Width, wall.Height), Color.Black);
        }
        spriteBatch.End();

        graphicsDevice.BlendState = BlendState.AlphaBlend;

        shapeCount = 0;
        vertexCount = 0;
        indexCount = 0;

        graphicsDevice.SetRenderTarget(LightTarget);
        graphicsDevice.Clear(Color.Black);

        if (EdgeLight) 
        {
            SetBlurEffectParameters(1.0f/960, 1.0f/540);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
            SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, blurEffect);
            spriteBatch.Draw(paperLight, Vector2.Zero, Color.White);
        }
        else 
        {
            spriteBatch.Begin();
            spriteBatch.Draw(paperLight, Vector2.Zero, Color.White);
        }

        spriteBatch.End();
    }

    public void Render(SpriteBatch spriteBatch) 
    {
        spriteBatch.Begin(SpriteSortMode.Immediate, blendRenderer,
        SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, envEffect);
        spriteBatch.Draw(LightTarget, Vector2.Zero, Color.White * Alpha);
        spriteBatch.End();
    }

    // Blur effect based from the Monogame Samples
    // https://github.com/CartBlanche/MonoGame-Samples/blob/f5956f2e8e94c0d0fc5d9889eec3fcb0bcb6e3c2/BloomSample/BloomComponent.cs#L239
    private void SetBlurEffectParameters(float dx, float dy)
    {
        EffectParameter weightsParameter, offsetsParameter;

        weightsParameter = blurEffect.Parameters["SampleWeights"];
        offsetsParameter = blurEffect.Parameters["SampleOffsets"];

        int sampleCount = weightsParameter.Elements.Count;

        float[] sampleWeights = new float[sampleCount];
        Vector2[] sampleOffsets = new Vector2[sampleCount];

        sampleWeights[0] = ComputeGaussian(0);
        sampleOffsets[0] = new Vector2(0);

        float totalWeights = sampleWeights[0];

        for (int i = 0; i < sampleCount / 2; i++)
        {
            float weight = ComputeGaussian(i + 1);

            sampleWeights[i * 2 + 1] = weight;
            sampleWeights[i * 2 + 2] = weight;

            totalWeights += weight * 2;
            float sampleOffset = i * 2 + 1.5f;

            Vector2 delta = new Vector2(dx, dy) * sampleOffset;

            sampleOffsets[i * 2 + 1] = delta;
            sampleOffsets[i * 2 + 2] = -delta;
        }

        for (int i = 0; i < sampleWeights.Length; i++)
        {
            sampleWeights[i] /= totalWeights;
        }

        weightsParameter.SetValue(sampleWeights);
        offsetsParameter.SetValue(sampleOffsets);
    }

    private float ComputeGaussian(float n)
    {
        float theta = 5f;

        return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                        Math.Exp(-(n * n) / (2 * theta * theta)));
    }
}