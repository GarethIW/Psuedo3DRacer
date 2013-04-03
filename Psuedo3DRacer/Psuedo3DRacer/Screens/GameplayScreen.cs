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
using Microsoft.Xna.Framework.Audio;
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

        //ContentManager content;
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

        bool standingsShown = false;

        AudioListener listener = new AudioListener();

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
            playerColor = SelectionScreen.carColor;
            currentTrack = 0;

            
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            //if (content == null)
            //    content = new ContentManager(ScreenManager.Game.Services, "Psuedo3DRacer.Content");

            //AudioController.LoadContent(content);

            parallaxManager = new ParallaxManager(ScreenManager.Viewport);

            gameHud = new HUD(ScreenManager.Viewport);
            gameHud.LoadContent(ScreenManager.Game.Content);

            gameCamera = new Camera(ScreenManager.GraphicsDevice, ScreenManager.Viewport);
            gameCamera.AttachedToCar = true;

            gameTrack = Track.Load("track" + ((Cup * 3) + currentTrack).ToString("000"), ScreenManager.Game.Content, parallaxManager, false);

            gameFont = ScreenManager.Game.Content.Load<SpriteFont>("font");
            texBlank = ScreenManager.Game.Content.Load<Texture2D>("blank");

            if (gameCars.Count == 0)
            {
                gameCars.Add(new Car(gameTrack.Length - 10, -0.2f, gameTrack, Color.Red));
                gameCars.Add(new Car(gameTrack.Length - 20, 0.2f, gameTrack, Color.Blue));
                gameCars.Add(new Car(gameTrack.Length - 30, -0.2f, gameTrack, Color.Green));
                gameCars.Add(new Car(gameTrack.Length - 40, 0.2f, gameTrack, Color.Gold));
                gameCars.Add(new Car(gameTrack.Length - 50, -0.2f, gameTrack, Color.Pink));
                gameCars.Add(new Car(gameTrack.Length - 60, 0.2f, gameTrack, Color.Purple));
                gameCars.Add(new Car(gameTrack.Length - 70, -0.2f, gameTrack, Color.Orange));
                gameCars.Add(new Car(gameTrack.Length - 80, 0.2f, gameTrack, playerColor));

                // Select "awesome" driver
                int sel = Psuedo3DRacer.rand.Next(7);
                gameCars[sel].SpeedWhenTurning = 0.051f + (0.002f * Cup);
                gameCars[sel].ConcentrationLevel = 1500 + (200 * Cup);
                gameCars[sel].CorrectionTime = 900 - (200 * Cup);
                gameCars[sel].ReactionTime = 100;

                // and "bad" driver
                int sel2 = sel;
                while (sel2 == sel)
                {
                    sel2 = Psuedo3DRacer.rand.Next(7);
                }
                gameCars[sel2].SpeedWhenTurning = 0.04f + (0.001f * Cup);
                gameCars[sel2].ConcentrationLevel = 50 + (200 * Cup);
                gameCars[sel2].CorrectionTime = 5000 - (200 * Cup);
                gameCars[sel2].ReactionTime = 2000 - (100 * Cup);
            }
            else
            {
                gameCars[0].Reset(gameTrack.Length - 10, -0.2f, gameTrack);
                gameCars[1].Reset(gameTrack.Length - 20, 0.2f, gameTrack);
                gameCars[2].Reset(gameTrack.Length - 30, -0.2f, gameTrack);
                gameCars[3].Reset(gameTrack.Length - 40, 0.2f, gameTrack);
                gameCars[4].Reset(gameTrack.Length - 50, -0.2f, gameTrack);
                gameCars[5].Reset(gameTrack.Length - 60, 0.2f, gameTrack);
                gameCars[6].Reset(gameTrack.Length - 70, -0.2f, gameTrack);
                gameCars[7].Reset(gameTrack.Length - 80, 0.2f, gameTrack);
            }

            gameCars[7].IsPlayerControlled = true;
            gameCars[7].camAttached = true;

            foreach (Car c in gameCars)
            {
                c.LoadContent(ScreenManager.Game.Content, 0);
                c.engineSound.Volume = 0.75f * AudioController.sfxvolume;
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

            ScreenManager.Game.ResetElapsedTime();
            trackFade = 0f;
            fadeTime = 0;
            startDelay = 6000;
            finishDelay = 3000;

            for (int i = 0; i < 8; i++) finishingPositions[i] = -1;

            mapRenderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice, 300, 300, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            RenderMap();

            steeringAmount = 0f;

            standingsShown = false;
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
           

            foreach (Car c in gameCars)
            {
                c.Dispose();
            }

            //content.Unload();
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
                    if (!AudioController.isPlaying && !gameCars[7].Finished) AudioController.PlayMusic(gameTrack.Horizon.ToString().ToLower());
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
                        gameCars[7].camAttached = false;
                        finishDelay -= gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (finishDelay <= 0)
                        {
                            if (!standingsShown)
                            {
                                ScreenManager.AddScreen(new StandingsScreen(gameCars, currentTrack), null);
                                standingsShown = true;
                            }
                            else
                            {
                                bool found=false;
                                foreach (GameScreen s in ScreenManager.GetScreens()) if (s.GetType() == typeof(StandingsScreen)) found = true;

                                if (!found)
                                {
                                    trackFade -= 0.01f;

                                    if (trackFade <= 0f)
                                    {
                                        currentTrack++;
                                        if (currentTrack == 3)
                                        {
                                            AudioController.PlayMusic("title");
                                            int pos = 1;
                                            foreach (Car c in gameCars.OrderByDescending(car => car.CupPoints))
                                            {
                                                c.engineSound.Stop();
                                                c.tyreSound.Stop();
                                                if (gameCars.IndexOf(c) == 7 && pos < SelectionScreen.trophies[Cup]) SelectionScreen.trophies[Cup] = pos;
                                                pos++;
                                            }
                                            LoadingScreen.Load(ScreenManager, false, null, new SelectionScreen(1));
                                        }
                                        else
                                        {
                                            foreach (Car c in gameCars)
                                            {
                                                c.engineSound.Stop();
                                            }
                                            LoadContent();
                                        }
                                    }
                                }
                                else if (trackFade > 0.5f) trackFade -= 0.01f;
                            }
                        }
                    }
                }

                if (gameCamera.AttachedToCar)
                {
                    gameCamera.Position = gameCars[7].CameraPosition;
                    gameCamera.LookAt(gameCars[7].CameraLookat, (gameCars[7].steeringAmount * 0.5f));
                }

                listener.Position = gameCamera.Position;
                listener.Forward = Vector3.TransformNormal(Vector3.Forward, Matrix.CreateRotationY(gameCamera.Yaw));
                listener.Up = Vector3.Up;

                foreach (Car c in gameCars)
                {
                    //if (c.IsPlayerControlled)
                    //{
                        float dist = (gameCamera.Position - c.Position).Length();
                        //float angle = Helper.WrapAngle((float)Math.Atan2((gameCamera.Position - c.Position).X, (gameCamera.Position - c.Position).Z));
                        //angle -= gameCamera.Yaw;

                        float distMod = MathHelper.Clamp(1f - ((1f / 6f) * dist), 0f, 1f);

                        c.engineSound.Volume = distMod;
                        c.crashSound.Volume = 0.4f * distMod;

                        if (Math.Abs(c.steeringAmount) > 0.2f || (c.applyingBrake && c.Speed>0f))
                        {
                            c.tyreSound.Play();
                            c.tyreSound.Pitch = ((float)Psuedo3DRacer.rand.NextDouble() * 0.2f) - 0.1f;
                            c.tyreSound.Volume = MathHelper.Lerp(c.tyreSound.Volume, 0.3f * distMod, 0.1f);
                        }
                        else
                        {
                            c.tyreSound.Volume = MathHelper.Lerp(c.tyreSound.Volume, 0f, 0.1f);
                            if (c.tyreSound.Volume <= 0.01f)
                                c.tyreSound.Stop();
                        }

                    c.emitter.Position = c.Position;
                    //c.emitter.Forward = c.Normal;
                    //c.engineSound.Apply3D(listener, c.emitter);

                    c.engineSound.Pitch = -1f + (c.Speed * 30f);
                    //c.engineSound.Volume = 0.75f * AudioController.sfxvolume;
                        
                    //}
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
                    if (steeringAmount > 0f) steeringAmount -= 0.015f;
                    steeringAmount -= 0.01f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f);
                }
                else if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.D) ||
                        input.CurrentKeyboardStates[0].IsKeyDown(Keys.Right))
                {
                    if (steeringAmount < 0f) steeringAmount += 0.015f;
                    steeringAmount += 0.01f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f);
                }
                else if (input.CurrentGamePadStates[0].ThumbSticks.Left.X < -0.05f)
                {
                    if (steeringAmount > 0f) steeringAmount -= 0.015f;
                    steeringAmount -= 0.01f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f * ((float)Math.Abs(input.CurrentGamePadStates[0].ThumbSticks.Left.X)), 0.5f);
                }
                else if (input.CurrentGamePadStates[0].ThumbSticks.Left.X > 0.05f)
                {
                    if (steeringAmount < 0f) steeringAmount += 0.015f;
                    steeringAmount += 0.01f;
                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f * ((float)Math.Abs(input.CurrentGamePadStates[0].ThumbSticks.Left.X)));
                }
                else steeringAmount = MathHelper.Lerp(steeringAmount, 0f, 0.15f);

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
        /// 
        float tiltAmount;

        public override void Draw(GameTime gameTime)
        {

            if (gameCars[7].camAttached)
                tiltAmount = gameCars[7].steeringAmount;

            if (gameTrack.HasLoaded)
            {
                ScreenManager.GraphicsDevice.Clear(gameTrack.SkyColor);

                Vector3 horizV = new Vector3(0, 0f, -100);
                Vector3 horiz = ScreenManager.Viewport.Project(horizV, gameCamera.projectionMatrix, gameCamera.ViewMatrixUpDownOnly(), gameCamera.worldMatrix);
                horizHeight = horiz.Y - (25f * parallaxManager.Scale);
                ScreenManager.SpriteBatch.Begin();
                ScreenManager.SpriteBatch.Draw(texBlank, new Rectangle(ScreenManager.Viewport.Width / 2, (int)horizHeight, ScreenManager.Viewport.Width * 2, (ScreenManager.Viewport.Height - (int)horizHeight) + 400), null, gameTrack.GroundColor, (tiltAmount * 0.5f), new Vector2(0.5f, 0), SpriteEffects.None, 1);
                ScreenManager.SpriteBatch.End();
                ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(new Vector3(parallaxManager.Scale, parallaxManager.Scale, 1f)) * Matrix.CreateRotationZ(tiltAmount * 0.5f) * Matrix.CreateTranslation(new Vector3(ScreenManager.Viewport.Width / 2, horizHeight, 0f)));
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
            gameHud.Draw(ScreenManager.SpriteBatch, gameCars[7]);
            if (gameCars[7].debug != null) ScreenManager.SpriteBatch.DrawString(gameFont, gameCars[7].debug, Vector2.One * 10f, Color.White);
            ScreenManager.SpriteBatch.End();



            if (!gameCars[7].Finished)
            {
                ScreenManager.SpriteBatch.Begin();
                ScreenManager.SpriteBatch.Draw(mapRenderTarget, new Vector2(ScreenManager.Viewport.Width - 320f, 20f), Color.White * 0.75f);
                ScreenManager.SpriteBatch.End();
                gameTrack.DrawMapCars(ScreenManager.SpriteBatch, new Vector2(ScreenManager.Viewport.Width - 320f, 20f), gameCars);
            }
            


            // If the game is transitioning on or off, fade it out to black.
            ScreenManager.FadeBackBufferToBlack(1f - trackFade);
        }

        void RenderMap()
        {
            ScreenManager.GraphicsDevice.SetRenderTarget(mapRenderTarget);
            gameTrack.DrawMap(ScreenManager.GraphicsDevice, ScreenManager.SpriteBatch, mapRenderTarget);
#if !WINDOWS_PHONE
             ScreenManager.GraphicsDevice.SetRenderTarget(null);
#else
                ScreenManager.GraphicsDevice.SetRenderTarget(Psuedo3DRacer.renderTarget);
                
#endif
        }


        #endregion
    }
}
