using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Psuedo3DRacer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Psuedo3DRacer : Microsoft.Xna.Framework.Game
    {
        public static Psuedo3DRacer Instance;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ScreenManager screenManager;

        RenderTarget2D renderTarget;

        public Psuedo3DRacer()
        {
            graphics = new GraphicsDeviceManager(this);

#if WINDOWS
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
#endif
#if WINRT || WINDOWS_PHONE
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#endif
            graphics.ApplyChanges();

            Content.RootDirectory = "Psuedo3DRacer.Content";

            screenManager = new ScreenManager(this, false);
            Components.Add(screenManager);


        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.ApplyChanges();

            Instance = this;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //screenManager.AddScreen(new GameplayScreen(), null);
            screenManager.AddScreen(new SelectionScreen(), null);

#if WINDOWS_PHONE
            renderTarget = new RenderTarget2D(GraphicsDevice, 1280, 768);
#endif
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //// Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
#if WINDOWS_PHONE
            GraphicsDevice.SetRenderTarget(renderTarget);
#endif

            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            base.Draw(gameTime);

#if WINDOWS_PHONE
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            spriteBatch.Draw(renderTarget, new Vector2(768, 0), null, Color.White, MathHelper.PiOver2, Vector2.Zero, 1f, SpriteEffects.None, 1);
            spriteBatch.End();
#endif
        }
    }
}
