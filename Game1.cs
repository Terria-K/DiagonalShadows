using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VertexTest;


public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private LightingRenderer renderer;
    private Light light;
    private Texture2D Background, SubBackground;
    private Effect effect;
    private RenderTarget2D rt;
    public static RenderTarget2D blurRt, fBlurRt;
    public static Texture2D PixelTexture;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        this.IsFixedTimeStep = false;
        graphics.SynchronizeWithVerticalRetrace = false;
        graphics.PreferredBackBufferWidth = 960;
        graphics.PreferredBackBufferHeight = 540;
    }

    protected override void Initialize()
    {
        rt = new RenderTarget2D(GraphicsDevice, 320, 180);
        blurRt = new RenderTarget2D(GraphicsDevice, 320, 180);
        fBlurRt = new RenderTarget2D(GraphicsDevice, 320, 180);
        renderer = new LightingRenderer(GraphicsDevice, rt);
        base.Initialize();
        // renderer.Lights.Add(new Light(2000, 2000));
        renderer.Lights.Add(light = new Light(20, 20));
        light.Color = Color.Blue;
        // renderer.Lights.Add(new Light(40, 40));
        renderer.Walls.Add(new Wall(100, 100, 10, 30) { Color = Color.LightGreen });
        renderer.Walls.Add(new Wall(220, 80, 20, 20) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(220, 20, 30, 30) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(250, 20, 30, 30) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(280, 20, 30, 30) { Color = Color.Blue });
    }

    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);
        renderer.LoadContent(Content);

        Background = Content.Load<Texture2D>("background.png");
        SubBackground = Content.Load<Texture2D>("subbackground.png");
        effect = Content.Load<Effect>("environment.fxb");

        PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        PixelTexture.SetData(new Color[] { Color.White });
    }

    protected override void UnloadContent()
    {
        // TODO: Unload any non ContentManager content here
    }

    protected override void Update(GameTime gameTime)
    {
        // Allows the game to exit
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            this.Exit();

        renderer.Update();
        light.Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

        base.Update(gameTime);
    }

    private void PreDraw() 
    {
        GraphicsDevice.SetRenderTarget(rt);
        GraphicsDevice.Clear(Color.Transparent);
        var rendererBlending = new BlendState();
        rendererBlending.ColorSourceBlend = Blend.Zero;
        rendererBlending.ColorDestinationBlend = Blend.SourceColor;

        spriteBatch.Begin();
        spriteBatch.Draw(Background, Vector2.Zero, Color.White);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Immediate, rendererBlending,
        SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);
        spriteBatch.Draw(renderer.lightTarget, Vector2.Zero, Color.White);
        spriteBatch.End();

        // Try moving this into the top of the lightTarget to see why I abandoned this.. 
        // meow! :>
        spriteBatch.Begin();
        foreach (var wall in renderer.Walls) 
        {
            spriteBatch.Draw(Game1.PixelTexture, new Vector2(wall.X, wall.Y), 
            new Rectangle(0, 0, wall.Width, wall.Height), Color.Red);
        }
        spriteBatch.End();


        renderer.Render(spriteBatch);
    }

    protected override void Draw(GameTime gameTime)
    {
        PreDraw();
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin();
        spriteBatch.Draw(rt, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 3, SpriteEffects.None, 1f);
        spriteBatch.End();
        base.Draw(gameTime);
    }
}