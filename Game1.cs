using System.IO;
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
    private Texture2D Background;
    private Texture2D SubBackground;
    private RenderTarget2D rt;
    private bool pressed;
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
        renderer = new LightingRenderer(GraphicsDevice, rt, new Point(320, 180));
        renderer.EdgeLight = true;
        renderer.EnvironmentColor = Color.White * 0.2f;
        base.Initialize();

        renderer.Lights.Add(light = new Light(20, 20));
        light.Color = Color.White;

        renderer.Walls.Add(new Wall(100, 100, 10, 30) { Color = Color.LightGreen });
        // renderer.Walls.Add(new Wall(220, 80, 20, 20) { Color = Color.Blue });
        // renderer.Walls.Add(new Wall(220, 20, 90, 30) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(220, 20, 8, 8) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(228, 20, 8, 8) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(228 + 8, 20, 8, 8) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(228 + 16, 20, 8, 8) { Color = Color.Blue });
        renderer.Walls.Add(new Wall(228 + 24, 20, 8, 8) { Color = Color.Blue });
    }

    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);
        renderer.LoadContent(Content);

        Background = Content.Load<Texture2D>("background.png");
        SubBackground = Content.Load<Texture2D>("subbackground.png");

        PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        PixelTexture.SetData(new Color[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        light.Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

        if (Mouse.GetState().LeftButton == ButtonState.Pressed) 
        {
            if (pressed) 
            {
                return;
            }
            pressed = true;
            renderer.Lights.Add(new Light(Mouse.GetState().X, Mouse.GetState().Y));
            return;
        }
        
        pressed = false;
    }

    protected override void Draw(GameTime gameTime)
    {
        Vector2 center = new Vector2(320 * 0.5f, 180 * 0.5f);
        var view = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));
        var projection = Matrix.CreateOrthographic(center.X * 2, center.Y * 2, -0.5f, 1);
        renderer.BeforeRender(spriteBatch, view, projection);

        GraphicsDevice.SetRenderTarget(rt);
        GraphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin();
        spriteBatch.Draw(Background, Vector2.Zero, Color.White * 0.95f);
        spriteBatch.End();

        spriteBatch.Begin();
        foreach (var wall in renderer.Walls) 
        {
            spriteBatch.Draw(Game1.PixelTexture, new Vector2(wall.X, wall.Y), 
            new Rectangle(0, 0, wall.Width, wall.Height), wall.Color);
        }
        spriteBatch.End();


        renderer.Render(spriteBatch);

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.White);
        spriteBatch.Begin();
        spriteBatch.Draw(rt, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 3, SpriteEffects.None, 1f);
        spriteBatch.End();
    }
}