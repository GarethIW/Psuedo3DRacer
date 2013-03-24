#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Psuedo3DRacer.Common;
using System.Collections.Generic;
#if WINRT || WINDOWS_PHONE || TOUCH
using Microsoft.Xna.Framework.Input.Touch;
#endif
#endregion

namespace Psuedo3DRacer
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class MainMenuScreen : GameScreen
    {
        #region Initialization

        ContentManager content;

        Camera gameCamera;
        Track gameTrack;

        List<Car> gameCars = new List<Car>();

        ParallaxManager parallaxManager;

        Random rand = new Random();

        BasicEffect drawEffect;
        AlphaTestEffect drawAlphaEffect;

        int followCar = 7;
        float camAngle = 0f;
        double camTime = 0;

        float whiteFlash = 0f;
        double flashTime = 0;
        bool hasFlashed = false;

        bool loadedSelect = false;

        Texture2D texLogo;
        Texture2D texBlank;
        SpriteFont gameFont;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
        {
            this.TransitionOnTime = TimeSpan.FromMilliseconds(2000);
            this.TransitionOffTime = TimeSpan.FromMilliseconds(1000);
#if WINRT || WINDOWS_PHONE || TOUCH
            EnabledGestures = GestureType.Tap;
#endif

        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Psuedo3DRacer.Content");

            texLogo = content.Load<Texture2D>("title");
            texBlank = content.Load<Texture2D>("blank");
            gameFont = content.Load<SpriteFont>("font");

            parallaxManager = new ParallaxManager(ScreenManager.Viewport);

            gameCamera = new Camera(ScreenManager.GraphicsDevice, ScreenManager.Viewport);
            gameCamera.AttachedToCar = true;

            // + ((Cup * 3) + currentTrack).ToString("000")
            gameTrack = Track.Load("track" + (rand.Next(4)).ToString("000"), content, parallaxManager, false);

          
            gameCars.Add(new Car(gameTrack.Length - 10, -0.2f, gameTrack, Color.Red));
            gameCars.Add(new Car(gameTrack.Length - 20, 0.2f, gameTrack, Color.Blue));
            gameCars.Add(new Car(gameTrack.Length - 30, -0.2f, gameTrack, Color.Green));
            gameCars.Add(new Car(gameTrack.Length - 40, 0.2f, gameTrack, Color.Gold));
            gameCars.Add(new Car(gameTrack.Length - 50, -0.2f, gameTrack, Color.Pink));
            gameCars.Add(new Car(gameTrack.Length - 60, 0.2f, gameTrack, Color.Purple));
            gameCars.Add(new Car(gameTrack.Length - 70, -0.2f, gameTrack, Color.Orange));
            gameCars.Add(new Car(gameTrack.Length - 80, 0.2f, gameTrack, Color.White));

            gameCamera.AttachedToCar = true;

            foreach (Car c in gameCars)
            {
                c.LoadContent(content, 0);
                c.Update(new GameTime(), gameTrack, gameCars);
            }
            foreach (Car c in gameCars) c.Update(new GameTime(), gameTrack, gameCars);


            drawEffect = new BasicEffect(ScreenManager.GraphicsDevice)
            {
                World = gameCamera.worldMatrix,
                View = gameCamera.viewMatrix,
                Projection = gameCamera.projectionMatrix,
                TextureEnabled = true
            };

            drawAlphaEffect = new AlphaTestEffect(ScreenManager.GraphicsDevice)
            {
                World = gameCamera.worldMatrix,
                View = gameCamera.viewMatrix,
                Projection = gameCamera.projectionMatrix,
                ReferenceAlpha = 254,
                AlphaFunction = CompareFunction.Greater

            };

            base.LoadContent();
        }


        #endregion

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            
            camTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            flashTime += gameTime.ElapsedGameTime.TotalMilliseconds;
         
            if (camTime >= 5000)
            {
                followCar = rand.Next(8);
                camTime = 0;
            }

            if (flashTime > 5000)
            {
                if (!hasFlashed)
                {
                    if (whiteFlash < 1f) whiteFlash += 0.1f;
                    else hasFlashed = true;
                }
                else
                {
                    if (whiteFlash > 0f) whiteFlash -= 0.1f;
                }
            }

            foreach (Car c in gameCars)
            {
                c.Update(gameTime, gameTrack, gameCars);
            }
            gameCamera.Position = gameCars[followCar].CameraPosition;
            gameCamera.LookAt(gameCars[followCar].CameraLookat, (gameCars[7].steeringAmount * 0.5f));
            

            drawEffect.View = gameCamera.viewMatrix;
            drawAlphaEffect.View = gameCamera.viewMatrix;

            parallaxManager.Update(gameTime, new Vector2(((1280 * 4) / MathHelper.TwoPi) * gameCamera.Yaw, 0f));

            if (IsExiting)
            {
                if (TransitionPosition >= 0.95f)
                {
                    if (!loadedSelect)
                    {
                        LoadingScreen.Load(ScreenManager, false, null, new SelectionScreen());
                        loadedSelect = true;
                    }
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        public override void HandleInput(InputState input)
        {
            PlayerIndex pi;

            if (IsActive)
            {
                if (input.IsMenuSelect(null, out pi) || input.MouseLeftClick) ExitScreen();
               
#if WINRT || WINDOWS_PHONE || TOUCH
                foreach (GestureSample gest in input.Gestures)
                {
                    if (gest.GestureType == GestureType.Tap)
                    {
                        ExitScreen();
                    }
                }
#endif
            }

            

            base.HandleInput(input);
        }

        float horizHeight;       

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(gameTrack.SkyColor);

            Vector3 horizV = new Vector3(0, 0f, -200);
            Vector3 horiz = ScreenManager.Viewport.Project(horizV, gameCamera.projectionMatrix, gameCamera.ViewMatrixUpDownOnly(), gameCamera.worldMatrix);
            horizHeight = horiz.Y - (25f * parallaxManager.Scale);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(texBlank, new Rectangle(ScreenManager.Viewport.Width / 2, (int)horizHeight, ScreenManager.Viewport.Width * 2, (ScreenManager.Viewport.Height - (int)horizHeight) + 400), null, gameTrack.GroundColor, (gameCars[7].steeringAmount * 0.5f), new Vector2(0.5f, 0), SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.End();
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(new Vector3(parallaxManager.Scale, parallaxManager.Scale, 1f)) * Matrix.CreateRotationZ(gameCars[7].steeringAmount * 0.5f) * Matrix.CreateTranslation(new Vector3(ScreenManager.Viewport.Width / 2, horizHeight, 0f)));
            parallaxManager.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.End();

            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            gameTrack.DrawBatches(ScreenManager.GraphicsDevice, drawEffect, drawAlphaEffect);
            //gameTrack.DrawRoad(ScreenManager.GraphicsDevice, drawEffect, gameCars[7].currentTrackPos, gameTrack.Length);
            foreach (Car c in gameCars) c.Draw(ScreenManager.GraphicsDevice, drawAlphaEffect, gameCamera);

            ScreenManager.SpriteBatch.Begin();
            if(!hasFlashed)
                ScreenManager.GraphicsDevice.Clear(Color.Black);

            if (whiteFlash > 0f)
                ScreenManager.SpriteBatch.Draw(texBlank, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White * whiteFlash);

            ScreenManager.SpriteBatch.End();


            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(texLogo, new Vector2(ScreenManager.Viewport.Width/2, ScreenManager.Viewport.Height/3) , null, Color.White, 0f, new Vector2(texLogo.Width, texLogo.Height) / 2, 1f, SpriteEffects.None, 1);
            //ScreenManager.SpriteBatch.DrawString(gameFont, "One-level Prototype for #1GAM 2013", new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height / 3) + new Vector2(0, 150), Color.White * TransitionAlpha, 0f, gameFont.MeasureString("One-level Prototype for #1GAM 2013") / 2, 1f, SpriteEffects.None, 1);
            //ScreenManager.SpriteBatch.DrawString(gameFont, "onegameamonth.com/garethiw", new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height / 3) + new Vector2(0, 250), Color.White * TransitionAlpha, 0f, gameFont.MeasureString("onegameamonth.com/garethiw") / 2, 0.75f, SpriteEffects.None, 1);
            //ScreenManager.SpriteBatch.DrawString(gameFont, "Control with WASD/Cursor, Xbox Pad or Touch/Tilt", new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height / 3) + new Vector2(0, 300), Color.White * TransitionAlpha, 0f, gameFont.MeasureString("Control with WASD/Cursor, Xbox Pad or Touch/Tilt") / 2, 0.75f, SpriteEffects.None, 1);

            ShadowText(ScreenManager.SpriteBatch, "onegameamonth.com/garethiw", new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height / 3) + new Vector2(0, 250), Color.White, gameFont.MeasureString("onegameamonth.com/garethiw") / 2, 0.75f);
            ShadowText(ScreenManager.SpriteBatch, "Control with WASD/Cursor, Xbox Pad or Touch/Tilt", new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height / 3) + new Vector2(0, 300), Color.LightGray, gameFont.MeasureString("Control with WASD/Cursor, Xbox Pad or Touch/Tilt") / 2, 0.6f);


            ScreenManager.SpriteBatch.End();

            ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);

            base.Draw(gameTime);
        }

        void ShadowText(SpriteBatch sb, string text, Vector2 pos, Color col, Vector2 off, float scale)
        {
            sb.DrawString(gameFont, text, pos + (Vector2.One * 2f), new Color(0, 0, 0, col.A), 0f, off, scale, SpriteEffects.None, 1);
            sb.DrawString(gameFont, text, pos, col, 0f, off, scale, SpriteEffects.None, 1);
        }
    }
}
