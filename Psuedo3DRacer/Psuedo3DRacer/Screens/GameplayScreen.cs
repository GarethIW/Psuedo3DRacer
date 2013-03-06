#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Psuedo3DRacer.Common;
using System.Collections.Generic;
#if WINRT || WINDOWS_PHONE || TOUCH
using Microsoft.Xna.Framework.Input.Touch;
#endif
#endregion

namespace Psuedo3DRacer
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        Camera gameCamera;
        Track gameTrack;

        Color playerColor;
        int Cup;
        int currentTrack;

        List<Car> gameCars = new List<Car>();

        int[] finishingPositions = new int[8];

        ParallaxManager parallaxManager;

        HUD gameHud;

        BasicEffect drawEffect;
        AlphaTestEffect drawAlphaEffect;

        Texture2D texBlank;

        float steeringAmount = 0f;

        float horizHeight;

        float trackFade = 0f;
        double fadeTime = 0;

        double startDelay = 6000;
        double finishDelay = 5000;

        RenderTarget2D mapRenderTarget;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(int cup, Color color)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            IsStubbourn = true;

#if WINRT || WINDOWS_PHONE || TOUCH
            EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.Hold;
#endif
            Cup = cup;
            playerColor = color;
            currentTrack = 0;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Psuedo3DRacer.Content");

            //AudioController.LoadContent(content);

            parallaxManager = new ParallaxManager(ScreenManager.Viewport);

            gameHud = new HUD(ScreenManager.Viewport);
            gameHud.LoadContent(content);

            gameCamera = new Camera(ScreenManager.GraphicsDevice);
            gameCamera.AttachedToCar = true;

            gameTrack = Track.Load("track" + ((Cup * 3) + currentTrack).ToString("000"), content, parallaxManager, false);

            gameFont = content.Load<SpriteFont>("font");
            texBlank = content.Load<Texture2D>("blank");

            gameCars.Clear();
            gameCars.Add(new Car(gameTrack.Length - 10, -0.2f, gameTrack, Color.Red));
            gameCars.Add(new Car(gameTrack.Length - 20, 0.2f, gameTrack, Color.Blue));
            gameCars.Add(new Car(gameTrack.Length - 30, -0.2f, gameTrack, Color.Green));
            gameCars.Add(new Car(gameTrack.Length - 40, 0.2f, gameTrack, Color.Gold));
            gameCars.Add(new Car(gameTrack.Length - 50, -0.2f, gameTrack, Color.Pink));
            gameCars.Add(new Car(gameTrack.Length - 60, 0.2f, gameTrack, Color.Purple));
            gameCars.Add(new Car(gameTrack.Length - 70, -0.2f, gameTrack, Color.Orange));
            gameCars.Add(new Car(gameTrack.Length - 80, 0.2f, gameTrack, playerColor));
            foreach (Car c in gameCars) c.LoadContent(content, 0);

            gameCars[7].IsPlayerControlled = true;

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

            ScreenManager.Game.ResetElapsedTime();
            trackFade = 0f;
            fadeTime = 0;
            startDelay = 6000;
            finishDelay = 3000;

            for (int i = 0; i < 8; i++) finishingPositions[i] = -1;

            mapRenderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice, 300, 300, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                //parallaxManager.Update(gameTime);
                //AudioController.Update(gameTime);

                gameHud.Update(gameTime, startDelay);

                fadeTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (fadeTime > 1000 && !gameCars[7].Finished)
                {
                    if (trackFade < 1f) trackFade += 0.01f;
                }

                startDelay -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (startDelay <= 0)
                {
                    foreach (Car c in gameCars)
                    {
                        c.Update(gameTime, gameTrack, gameCars);

                        if (c.Finished)
                        {
                            bool found = false;
                            for (int i = 0; i < 8; i++) if (finishingPositions[i] == gameCars.IndexOf(c)) found = true;
                            if (!found)
                            {
                                for (int i = 0; i < 8; i++) 
                                    if (finishingPositions[i] == -1)
                                    {
                                        finishingPositions[i] = gameCars.IndexOf(c);
                                        break;
                                    }
                            }
                        }
                    }

                    if (gameCars[7].Finished)
                    {
                        gameCamera.AttachedToCar = false;
                        finishDelay -= gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (finishDelay <= 0)
                        {
                            trackFade -= 0.01f;

                            if (trackFade <= 0f)
                            {
                                currentTrack++;
                                if (currentTrack == 3)
                                    LoadingScreen.Load(ScreenManager, false, null, new SelectionScreen());
                                else
                                    LoadContent();
                            }
                        }
                    }
                }

                if (gameCamera.AttachedToCar)
                {
                    gameCamera.Position = gameCars[7].CameraPosition;
                    gameCamera.LookAt(gameCars[7].CameraLookat, (gameCars[7].steeringAmount * 0.5f));
                }
            }

            drawEffect.View = gameCamera.viewMatrix;
            drawAlphaEffect.View = gameCamera.viewMatrix;

            parallaxManager.Update(gameTime, new Vector2(((1280*4) / MathHelper.TwoPi) * gameCamera.Yaw, 0f));
        }




        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = 0;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            PlayerIndex player;
            if (input.IsPauseGame(ControllingPlayer))
            {
                PauseBackgroundScreen pauseBG = new PauseBackgroundScreen();
                ScreenManager.AddScreen(pauseBG, ControllingPlayer);
                ScreenManager.AddScreen(new PauseMenuScreen(pauseBG), ControllingPlayer);
            }
        
            if(IsActive)
            {
                

                gameCars[7].ApplyThrottle(0f);
                gameCars[7].ApplyBrake(false);

                if(input.CurrentKeyboardStates[0].IsKeyDown(Keys.W) ||
                   input.CurrentKeyboardStates[0].IsKeyDown(Keys.Up))
                {
                    gameCars[7].ApplyThrottle(1f);
                }
                else gameCars[7].ApplyThrottle(0f);

                if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.S) ||
                   input.CurrentKeyboardStates[0].IsKeyDown(Keys.Down))
                {
                    gameCars[7].ApplyBrake(true);
                }
                else gameCars[7].ApplyBrake(false);

                if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.A) ||
                   input.CurrentKeyboardStates[0].IsKeyDown(Keys.Left))
                {
                    if (steeringAmount > 0f) steeringAmount -= 0.02f;
                    steeringAmount -= 0.02f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f);
                }
                else if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.D) ||
                        input.CurrentKeyboardStates[0].IsKeyDown(Keys.Right))
                {
                    if (steeringAmount < 0f) steeringAmount += 0.02f;
                    steeringAmount += 0.02f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f);
                }
                else if (input.CurrentGamePadStates[0].ThumbSticks.Left.X < -0.1f)
                {
                    if (steeringAmount > 0f) steeringAmount -= 0.02f;
                    steeringAmount -= 0.02f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f * ((float)Math.Abs(input.CurrentGamePadStates[0].ThumbSticks.Left.X)), 0.5f);
                }
                else if (input.CurrentGamePadStates[0].ThumbSticks.Left.X > 0.1f)
                {
                    if (steeringAmount < 0f) steeringAmount += 0.02f;
                    steeringAmount += 0.02f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f * ((float)Math.Abs(input.CurrentGamePadStates[0].ThumbSticks.Left.X)));
                }
                else steeringAmount = MathHelper.Lerp(steeringAmount, 0f, 0.25f);

                if (input.CurrentGamePadStates[0].Triggers.Right > 0.05f)
                {
                    gameCars[7].ApplyThrottle(input.CurrentGamePadStates[0].Triggers.Right > 0.95f ? 1f : input.CurrentGamePadStates[0].Triggers.Right);
                }
                if (input.CurrentGamePadStates[0].Triggers.Left > 0.05f)
                {
                    gameCars[7].ApplyBrake(true);
                }

#if WINRT || WINDOWS_PHONE
                foreach(TouchLocation tl in TouchPanel.GetState())
                {
                    if (tl.State == TouchLocationState.Pressed || tl.State == TouchLocationState.Moved)
                    {
                        if(tl.Position.X> ScreenManager.Viewport.Width/2)
                            gameCars[7].ApplyThrottle(1f);
                        else
                            gameCars[7].ApplyThrottle(0f);

                        if(tl.Position.X< ScreenManager.Viewport.Width/2)
                            gameCars[7].ApplyBrake(true);
                        else
                            gameCars[7].ApplyBrake(false);
                    }
                }

                if(Math.Abs(input.AccelerometerVect.X)>0.05f)
                {
                    steeringAmount = input.AccelerometerVect.X;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f);
                }
                //if (Math.Abs(input.AccelerometerVect.X) > 0.15f)
                //{
                //    if (input.AccelerometerVect.X > 0) gameCars[7].Steer(input.AccelerometerVect.X);
                //    if (input.AccelerometerVect.X < 0) gameCars[7].Steer(input.AccelerometerVect.X);
                //}
#endif


                gameCars[7].Steer(steeringAmount);
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
           
            if (gameTrack.HasLoaded)
            {
                gameTrack.DrawMap(ScreenManager.GraphicsDevice, ScreenManager.SpriteBatch, mapRenderTarget, gameCars);

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
                //gameTrack.DrawScenery(ScreenManager.GraphicsDevice, drawAlphaEffect, gameCars[7].currentTrackPos, gameTrack.Length);

            }


            ScreenManager.SpriteBatch.Begin();
            gameHud.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.Draw(mapRenderTarget, new Vector2(ScreenManager.Viewport.Width - 320f, 20f), Color.White*0.75f);
            
            if(gameCars[7].debug!=null) ScreenManager.SpriteBatch.DrawString(gameFont, gameCars[7].debug, Vector2.One * 10f, Color.White);
            ScreenManager.SpriteBatch.End();


            // If the game is transitioning on or off, fade it out to black.
            ScreenManager.FadeBackBufferToBlack(1f - trackFade);
        }


        #endregion
    }
}
