
#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Psuedo3DRacer.Common;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
#if WINRT || WINDOWS_PHONE || TOUCH
using Microsoft.Xna.Framework.Input.Touch;
#endif
#endregion

namespace Psuedo3DRacer
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary> 

    public class SelectionScreen : GameScreen
    {
        #region Fields

        Random rand = new Random();

        ContentManager content;

        Dictionary<string, Texture2D> texList = new Dictionary<string, Texture2D>();

        Track Track;
        ParallaxManager Parallax;
        Camera Camera;

        BasicEffect drawEffect;
        AlphaTestEffect drawAlphaEffect;

        Vector3 lookAtOffset = Vector3.Zero;
        Vector3 lookAtTarget;

        int selectionMode = 0;

        Vector2 bannerTop1Pos;
        Vector2 bannerTop2Pos;
        Vector2 bannerBottom1Pos;
        Vector2 bannerBottom2Pos;

        Vector2 PaintPos;
        Vector2 CarPos;
        Vector2 SpotPos;

        float carScale = 1f;

        float paintCarAlpha = 1f;
        float selectCupAlpha = 1f;

        Rectangle redRect;
        Rectangle greenRect;
        Rectangle blueRect;

        Rectangle leftRect;
        Rectangle rightRect;
        Rectangle leftCupRect;
        Rectangle rightCupRect;

        Color carColor = new Color(255, 255, 255);

        Texture2D[] texCarDirections;

        int carFrame = 0;
        double carSpinTime = 0;

        int selectedColor = 0;

        Vector2 cupPosition;

        bool loadedGameplay = false;


        class Cup
        {
            public Vector2 Position;
            public float Scale;

            
        }

        List<Cup> Cups = new List<Cup>();

        Track[] cupTracks = new Track[3];
        ParallaxManager[] cupTrackPM = new ParallaxManager[3];
        RenderTarget2D[] cupTrackRT = new RenderTarget2D[3];
        int[] trackPos = new int[3];
        

        int selectedCup = 0;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectionScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1);
            TransitionOffTime = TimeSpan.FromSeconds(1);

#if WINRT || WINDOWS_PHONE || TOUCH
            EnabledGestures = GestureType.Tap | GestureType.HorizontalDrag |GestureType.Flick;
#endif
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Psuedo3DRacer.Content");

            LoadTex("arrow");
            LoadTex("blank");
            LoadTex("blank-track");
            LoadTex("banner-bottom");
            LoadTex("banner-top");
            LoadTex("car-spot");
            LoadTex("colors");
            LoadTex("paintcar");
            LoadTex("selectcup");
            LoadTex("triangles");
            LoadTex("cup-holder");
            LoadTex("cuparrow");

            for (int i = 1; i <= 5; i++)
            {
                LoadTex("cuptitle" + i.ToString());
            }

            texCarDirections = new Texture2D[8];
            for (int dir = 0; dir < 8; dir++)
            {
                texCarDirections[dir] = content.Load<Texture2D>("cars/0-" + dir + "-0");
            }

            Parallax = new ParallaxManager(ScreenManager.Viewport);
            Camera = new Camera(ScreenManager.GraphicsDevice, ScreenManager.Viewport);
            Track = Track.Load("track000", content, Parallax, false);

            drawEffect = new BasicEffect(ScreenManager.GraphicsDevice)
            {
                World = Camera.worldMatrix,
                View = Camera.viewMatrix,
                Projection = Camera.projectionMatrix,
                TextureEnabled = true
            };

            drawAlphaEffect = new AlphaTestEffect(ScreenManager.GraphicsDevice)
            {
                World = Camera.worldMatrix,
                View = Camera.viewMatrix,
                Projection = Camera.projectionMatrix,
                ReferenceAlpha = 254,
                AlphaFunction = CompareFunction.Greater
            };

            // Initial positions
            bannerTop1Pos = new Vector2(150, -230);
            bannerTop2Pos = new Vector2(400, -230);

            CarPos = new Vector2(ScreenManager.Viewport.Width / 2, -250f);
            SpotPos = new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height + 500f);
            PaintPos = new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height + 500f);

            Cups.Add(new Cup());
            Cups.Add(new Cup());
            Cups.Add(new Cup());
            //Cups.Add(new Cup());
            //Cups.Add(new Cup());

            cupPosition = new Vector2(ScreenManager.Viewport.Width + 500, ScreenManager.Viewport.Height / 2);
            for (int i = selectedCup - 2; i <= selectedCup + 2; i++)
            {
                if (i >= 0 && i < Cups.Count)
                {
                    Cups[i].Position = cupPosition + new Vector2(i * 150, 0);
                    Cups[i].Scale = 1f - (Math.Abs(i) * 0.25f);
                }
            }

            cupTrackRT[0] = new RenderTarget2D(ScreenManager.GraphicsDevice, 193, 108, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            cupTrackRT[1] = new RenderTarget2D(ScreenManager.GraphicsDevice, 193, 108, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            cupTrackRT[2] = new RenderTarget2D(ScreenManager.GraphicsDevice, 193, 108, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            leftRect = new Rectangle(0, ScreenManager.Viewport.Height - 75, 150, 50);
            rightRect = new Rectangle(ScreenManager.Viewport.Width-150, ScreenManager.Viewport.Height - 75, 150, 50);

            leftCupRect = new Rectangle((int)cupPosition.X - 300, (int)cupPosition.Y - 100, 50, 200);
            rightCupRect = new Rectangle((int)cupPosition.X + 250, (int)cupPosition.Y - 100, 50, 200);

            ScreenManager.Game.ResetElapsedTime();
        }

        void LoadTex(string name)
        {
            texList.Add(name, content.Load<Texture2D>("selectionscreen/" + name));            
        }

        void LoadTracks(int cup)
        {
            for (int i = 0; i < 3; i++)
            {
                cupTrackPM[i] = new ParallaxManager(new Viewport(cupTrackRT[i].Bounds));
                //cupTrackPM[i].Scale = 108f / 720f;
                //cupTracks[i] = Track.Load("track" +  i.ToString("000"), content, cupTrackPM[i], true);
                cupTracks[i] = Track.Load("track" + ((selectedCup * 3) + i).ToString("000"), content, cupTrackPM[i], true);

                trackPos[i] = cupTracks[i].Length - 50;
            }
            ScreenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            if ((lookAtOffset - lookAtTarget).Length() < 0.01f)
            {
                if(rand.Next(100)==1)
                    lookAtTarget = new Vector3(((float)rand.NextDouble() * 0.5f) - 0.25f, ((float)rand.NextDouble() * 0.5f) - 0.25f, ((float)rand.NextDouble() * 0.5f) - 0.25f);
            }
            lookAtOffset = Vector3.Lerp(lookAtOffset, lookAtTarget, 0.02f);

            Camera.Position = new Vector3(3f, 0.2f, 16f);
            Camera.LookAt(new Vector3(0f, 0.5f, 25f) + lookAtOffset, 0f);

            drawEffect.View =Camera.viewMatrix;
            drawAlphaEffect.View = Camera.viewMatrix;

            Parallax.Update(gameTime, new Vector2(((1280 * 4) / MathHelper.TwoPi) * Camera.Yaw, 0f));

            if (selectionMode == 0)
            {
                bannerTop1Pos = Vector2.Lerp(bannerTop1Pos, new Vector2(150f, 0f), 0.1f);
                bannerBottom1Pos = bannerTop1Pos + new Vector2(0, 210);
                bannerTop2Pos = Vector2.Lerp(bannerTop2Pos, new Vector2(400, -230), 0.1f);
                bannerBottom2Pos = bannerTop2Pos + new Vector2(0, 210);
                CarPos = Vector2.Lerp(CarPos, new Vector2(ScreenManager.Viewport.Width, ScreenManager.Viewport.Height-50)/2, 0.1f);
                carScale = MathHelper.Lerp(carScale, 1f, 0.1f);
                SpotPos = Vector2.Lerp(SpotPos, new Vector2(ScreenManager.Viewport.Width, ScreenManager.Viewport.Height+100)/2, 0.1f);
                PaintPos = Vector2.Lerp(PaintPos, new Vector2(ScreenManager.Viewport.Width/2, ScreenManager.Viewport.Height - 150) , 0.1f);
                paintCarAlpha = MathHelper.Lerp(paintCarAlpha, 1f, 0.1f);

                cupPosition = Vector2.Lerp(cupPosition, new Vector2(ScreenManager.Viewport.Width + 500, ScreenManager.Viewport.Height / 2), 0.1f);
                leftCupRect = new Rectangle((int)cupPosition.X - 350, (int)cupPosition.Y - 100, 50, 200);
                rightCupRect = new Rectangle((int)cupPosition.X + 300, (int)cupPosition.Y - 100, 50, 200);
            }
            else
            {
                bannerTop1Pos = Vector2.Lerp(bannerTop1Pos, new Vector2(150f, -50f), 0.1f);
                bannerBottom1Pos = bannerTop1Pos + new Vector2(0, 210);
                bannerTop2Pos = Vector2.Lerp(bannerTop2Pos, new Vector2(400f, 0f), 0.1f);
                bannerBottom2Pos = bannerTop2Pos + new Vector2(0, 210);
                paintCarAlpha = MathHelper.Lerp(paintCarAlpha, 0f, 0.1f);

                carScale = MathHelper.Lerp(carScale, 0.5f, 0.1f);
                CarPos = Vector2.Lerp(CarPos, bannerBottom1Pos - new Vector2(0, ((bannerBottom1Pos.Y - (bannerTop1Pos.Y + texList["banner-top"].Height)) / 2) - 1f), 0.1f);

                SpotPos = Vector2.Lerp(SpotPos, new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height + 500f), 0.1f);
                PaintPos = Vector2.Lerp(PaintPos, new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height + 500f), 0.1f);


                cupPosition = Vector2.Lerp(cupPosition, new Vector2(500 + ((ScreenManager.Viewport.Width - 500)/2), ScreenManager.Viewport.Height / 2), 0.1f);

                leftCupRect = new Rectangle((int)cupPosition.X - 350, (int)cupPosition.Y - 100, 50, 200);
                rightCupRect = new Rectangle((int)cupPosition.X + 300, (int)cupPosition.Y - 100, 50, 200);

                for (int i = 0; i < 3; i++)
                {
                    if (cupTracks[i].HasLoaded)
                    {
                        trackPos[i]++;
                        if (trackPos[i] == cupTracks[i].Length) trackPos[i] = 0;
                    }
                }

            }

            for (int i = 0; i < Cups.Count; i++)
            {

                Cups[i].Position = Vector2.Lerp(Cups[i].Position, cupPosition + new Vector2((i - selectedCup) * (200 * (1f - (Math.Abs(i - selectedCup) * 0.20f))), 0), 0.1f);
                //Cups[i].Position = Vector2.Lerp(Cups[i].Position, cupPosition + new Vector2((i - selectedCup) * (200 * (1f - (Math.Abs(i - selectedCup) * 0.25f))), 0), 0.1f);
                Cups[i].Scale = MathHelper.Lerp(Cups[i].Scale, 1f - (Math.Abs(i - selectedCup) * 0.25f), 0.1f);

            }

            redRect = new Rectangle((int)PaintPos.X - (texList["colors"].Width / 2), (int)PaintPos.Y - (texList["colors"].Height / 2), texList["colors"].Width, 40);
            greenRect = redRect;
            greenRect.Offset(0, 60);
            blueRect = greenRect;
            blueRect.Offset(0, 60);

            carSpinTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (carSpinTime > 200)
            {
                carSpinTime = 0;
                carFrame--;
                if (carFrame == -1) carFrame = 7;
            }


            if (IsExiting && selectionMode==1)
            {
                if (TransitionPosition >= 0.95f)
                {
                    if (!loadedGameplay)
                    {
                        LoadingScreen.Load(ScreenManager, false, null, new GameplayScreen(selectedCup, carColor));
                        loadedGameplay = true;
                    }
                }
            }

            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void HandleInput(InputState input)
        {
            PlayerIndex player;

            if (selectionMode == 0)
            {
                if (input.CurrentKeyboardStates[0].IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) ||
                   input.CurrentKeyboardStates[0].IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) ||
                    input.CurrentGamePadStates[0].ThumbSticks.Left.X<-0.2f ||
                    input.CurrentGamePadStates[0].DPad.Left == ButtonState.Pressed)
                {
                    switch (selectedColor)
                    {
                        case 0:
                            if (carColor.R > 25) carColor.R -= 2;
                            break;
                        case 1:
                            if (carColor.G > 25) carColor.G -= 2;
                            break;
                        case 2:
                            if (carColor.B > 25) carColor.B -= 2;
                            break;
                    }

                }
                if (input.CurrentKeyboardStates[0].IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) ||
                   input.CurrentKeyboardStates[0].IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) ||
                    input.CurrentGamePadStates[0].ThumbSticks.Left.X > 0.2f ||
                    input.CurrentGamePadStates[0].DPad.Right == ButtonState.Pressed)
                {
                    switch (selectedColor)
                    {
                        case 0:
                            if (carColor.R < 255) carColor.R += 2;
                            break;
                        case 1:
                            if (carColor.G < 255) carColor.G += 2;
                            break;
                        case 2:
                            if (carColor.B < 255) carColor.B += 2;
                            break;
                    }
                }

                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Up, null, out player) ||
                   input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.W, null, out player) ||
                    input.IsMenuUp(null))
                {
                    selectedColor--;
                    if (selectedColor == -1) selectedColor = 2;
                }
                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Down, null, out player) ||
                   input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.S, null, out player) ||
                    input.IsMenuDown(null))
                {
                    selectedColor++;
                    if (selectedColor == 3) selectedColor = 0;
                }

                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Enter, null, out player) ||
                   input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Space, null, out player) ||
                    input.IsMenuSelect(null, out player))
                {
                    selectionMode = 1;
                    LoadTracks(selectedCup);
                }

                if (input.MouseLeftClick)
                {
                    if(rightRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        selectionMode = 1;
                        LoadTracks(selectedCup);
                    }

                    if (leftRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        LoadingScreen.Load(ScreenManager, false, null, new MainMenuScreen());
                    }

                    
                }

                if (input.MouseDragging)
                {
                    if (redRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        carColor.R = (byte)MathHelper.Clamp((25f + (230f / 400f) * ((input.CurrentMouseState.X - (float)redRect.Left))), 25f, 255f);
                        selectedColor = 0;
                    }
                    if (greenRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        carColor.G = (byte)MathHelper.Clamp((25f + (230f / 400f) * ((input.CurrentMouseState.X - (float)greenRect.Left))), 25f, 255f);
                        selectedColor = 1;
                    }
                    if (blueRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        carColor.B = (byte)MathHelper.Clamp((25f + (230f / 400f) * ((input.CurrentMouseState.X - (float)blueRect.Left))), 25f, 255f);
                        selectedColor = 2;
                    }
                }

#if WINRT || WINDOWS_PHONE || TOUCH
                foreach (GestureSample gest in input.Gestures)
                {
                    if (gest.GestureType == GestureType.HorizontalDrag)
                    {
                        if (redRect.Contains(new Point((int)gest.Position.X, (int)gest.Position.Y)))
                        {
                            carColor.R = (byte)MathHelper.Clamp((25f + (230f / 400f) * ((gest.Position.X - (float)redRect.Left))), 25f, 255f);
                            selectedColor = 0;
                        }
                        if (greenRect.Contains(new Point((int)gest.Position.X, (int)gest.Position.Y)))
                        {
                            carColor.G = (byte)MathHelper.Clamp((25f + (230f / 400f) * ((gest.Position.X - (float)greenRect.Left))), 25f, 255f);
                            selectedColor = 1;
                        }
                        if (blueRect.Contains(new Point((int)gest.Position.X, (int)gest.Position.Y)))
                        {
                            carColor.B = (byte)MathHelper.Clamp((25f + (230f / 400f) * ((gest.Position.X - (float)blueRect.Left))), 25f, 255f);
                            selectedColor = 2;
                        }
                    }
                }

                if (input.TapGesture != null)
                {
#if WINDOWS_PHONE
                    if (rightRect.Contains(new Point((int)input.TapGesture.Value.Position.Y, ScreenManager.Viewport.Height - (int)input.TapGesture.Value.Position.X)))
#else
                    if (rightRect.Contains(new Point((int)input.TapGesture.Value.Position.X, (int)input.TapGesture.Value.Position.Y)))
#endif
                    {
                        selectionMode = 1;
                        LoadTracks(selectedCup);
                    }

#if WINDOWS_PHONE
                    if (leftRect.Contains(new Point((int)input.TapGesture.Value.Position.Y, ScreenManager.Viewport.Height - (int)input.TapGesture.Value.Position.X)))
#else
                    if (leftRect.Contains(new Point((int)input.TapGesture.Value.Position.X, (int)input.TapGesture.Value.Position.Y)))
#endif
                    {
                        LoadingScreen.Load(ScreenManager, false, null, new MainMenuScreen());
                    }
                }

#endif
                if (input.IsMenuCancel(null, out player)) LoadingScreen.Load(ScreenManager, false, null, new MainMenuScreen());
            }
            else
            {
                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Escape, null, out player) ||
                   input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Back, null, out player) ||
                    input.IsMenuCancel(null, out player))
                {
                    selectionMode = 0;
                }

                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Left, null, out player) ||
                   input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.A, null, out player) ||
                    (input.CurrentGamePadStates[0].ThumbSticks.Left.X<-0.2f && input.LastGamePadStates[0].ThumbSticks.Left.X>=-0.2f) ||
                    input.CurrentGamePadStates[0].DPad.Left == ButtonState.Pressed && input.LastGamePadStates[0].DPad.Left != ButtonState.Pressed)
                {
                    selectedCup--;
                    if (selectedCup == -1) selectedCup = 0;
                    else LoadTracks(selectedCup);
                }
                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Right, null, out player) ||
                   input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.D, null, out player) ||
                    (input.CurrentGamePadStates[0].ThumbSticks.Left.X > 0.2f && input.LastGamePadStates[0].ThumbSticks.Left.X <= 0.2f) ||
                    input.CurrentGamePadStates[0].DPad.Right == ButtonState.Pressed && input.LastGamePadStates[0].DPad.Right != ButtonState.Pressed)
                {
                    selectedCup++;
                    if (selectedCup == Cups.Count) selectedCup = Cups.Count-1;
                    else LoadTracks(selectedCup);
                }

                if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Enter, null, out player) ||
                   input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Space, null, out player) ||
                    input.IsMenuSelect(null, out player))
                {
                    ExitScreen();
                    //
                }

                if (input.MouseLeftClick)
                {
                    if (rightRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        ExitScreen();
                    }

                    if (leftRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        selectionMode = 0;
                    }

                    if (leftCupRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        selectedCup--;
                        if (selectedCup == -1) selectedCup = 0;
                        else LoadTracks(selectedCup);
                    }

                    if (rightCupRect.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                    {
                        selectedCup++;
                        if (selectedCup == Cups.Count) selectedCup = Cups.Count - 1;
                        else LoadTracks(selectedCup);
                    }
                }

#if WINRT || WINDOWS_PHONE || TOUCH

                foreach (GestureSample gest in input.Gestures)
                {
                    if (gest.GestureType == GestureType.Flick)
                    {
                       
                            if (gest.Delta.X > 0f)
                            {
                                selectedCup--;
                                if (selectedCup == -1) selectedCup = 0;
                                else LoadTracks(selectedCup);
                            }

                            if (gest.Delta.X < 0f)
                            {
                                selectedCup++;
                                if (selectedCup == Cups.Count) selectedCup = Cups.Count - 1;
                                else LoadTracks(selectedCup);
                            }
                       
                    }
                }

                if (input.TapGesture != null)
                {
#if WINDOWS_PHONE
                    if (rightRect.Contains(new Point((int)input.TapGesture.Value.Position.Y, ScreenManager.Viewport.Height - (int)input.TapGesture.Value.Position.X)))
#else
                    if (rightRect.Contains(new Point((int)input.TapGesture.Value.Position.X, (int)input.TapGesture.Value.Position.Y)))
#endif
                    {
                        ExitScreen();
                    }

#if WINDOWS_PHONE
                    if (leftRect.Contains(new Point((int)input.TapGesture.Value.Position.Y, ScreenManager.Viewport.Height - (int)input.TapGesture.Value.Position.X)))
#else
                    if (leftRect.Contains(new Point((int)input.TapGesture.Value.Position.X, (int)input.TapGesture.Value.Position.Y)))
#endif
                    {
                        selectionMode = 0;
                    }

#if WINDOWS_PHONE
                    if (rightCupRect.Contains(new Point((int)input.TapGesture.Value.Position.Y, ScreenManager.Viewport.Height - (int)input.TapGesture.Value.Position.X)))
#else
                    if (rightCupRect.Contains(new Point((int)input.TapGesture.Value.Position.X, (int)input.TapGesture.Value.Position.Y)))
#endif
                    {
                        selectedCup++;
                        if (selectedCup == Cups.Count) selectedCup = Cups.Count - 1;
                        else LoadTracks(selectedCup);
                    }

#if WINDOWS_PHONE
                    if (leftCupRect.Contains(new Point((int)input.TapGesture.Value.Position.Y, ScreenManager.Viewport.Height - (int)input.TapGesture.Value.Position.X)))
#else
                    if (leftCupRect.Contains(new Point((int)input.TapGesture.Value.Position.X, (int)input.TapGesture.Value.Position.Y)))
#endif
                    {
                        selectedCup--;
                        if (selectedCup == -1) selectedCup = 0;
                        else LoadTracks(selectedCup);
                    }
                }

#endif
            }


            base.HandleInput(input);
        }

       

        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            Vector3 horizV;
            Vector3 horiz;
            float horizHeight;

            if (selectionMode == 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!cupTracks[i].HasLoaded) continue;

                    ScreenManager.GraphicsDevice.SetRenderTarget(cupTrackRT[i]);

                    ScreenManager.GraphicsDevice.Clear(cupTracks[i].SkyColor);

                    int nextpos = (trackPos[i] + 1);
                    if (nextpos >= cupTracks[i].TrackSegments.Count) nextpos = nextpos - cupTracks[i].TrackSegments.Count;
                    Camera.Position = cupTracks[i].TrackSegments[trackPos[i]].Position + new Vector3(0, 0.5f, 0);
                    Camera.LookAt(cupTracks[i].TrackSegments[nextpos].Position + new Vector3(0, 0.5f, 0), 0f);
                    drawEffect.View = Camera.viewMatrix;
                    drawEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 16f/9f, 0.001f, 100f);
                    drawAlphaEffect.View = Camera.viewMatrix;
                    drawAlphaEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 16f / 9f, 0.001f, 100f);


                    cupTrackPM[i].Update(gameTime, new Vector2(((1280 * 4) / MathHelper.TwoPi) * Camera.Yaw, 0f));

                    horizV = new Vector3(0, 0f, -200);
                    horiz = ScreenManager.Viewport.Project(horizV, Camera.projectionMatrix, Camera.ViewMatrixUpDownOnly(), Camera.worldMatrix);
                    horizHeight = horiz.Y - (25f * cupTrackPM[i].Scale);
                    ScreenManager.SpriteBatch.Begin();
                    ScreenManager.SpriteBatch.Draw(texList["blank"], new Rectangle(cupTrackRT[i].Width / 2, (int)(horizHeight * cupTrackPM[i].Scale), cupTrackRT[i].Width * 2, (cupTrackRT[i].Height - (int)(horizHeight*cupTrackPM[i].Scale)) + 400), null, cupTracks[i].GroundColor, 0f, new Vector2(0.5f, 0), SpriteEffects.None, 1);
                    ScreenManager.SpriteBatch.End();
                    ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(new Vector3(cupTrackPM[i].Scale, cupTrackPM[i].Scale, 1f)) * Matrix.CreateTranslation(new Vector3(cupTrackRT[i].Width / 2, horizHeight * cupTrackPM[i].Scale, 0f)));
                    cupTrackPM[i].Draw(ScreenManager.SpriteBatch);
                    ScreenManager.SpriteBatch.End();

                    ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                    ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
                    ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

                    cupTracks[i].DrawBatches(ScreenManager.GraphicsDevice, drawEffect, drawAlphaEffect);

#if !WINDOWS_PHONE
                    ScreenManager.GraphicsDevice.SetRenderTarget(null);
#else
                    ScreenManager.GraphicsDevice.SetRenderTarget(Psuedo3DRacer.renderTarget);
               
#endif
                }

                
            }

            Camera.Position = new Vector3(3f, 0.2f, 16f);
            Camera.LookAt(new Vector3(0f, 0.5f, 25f) + lookAtOffset, 0f);

            drawEffect.View = Camera.viewMatrix;
            drawEffect.Projection = Camera.projectionMatrix;
            drawAlphaEffect.View = Camera.viewMatrix;
            drawAlphaEffect.Projection = Camera.projectionMatrix;

            ScreenManager.GraphicsDevice.Clear(Track.SkyColor);
            horizV = new Vector3(0, 0f, -200);
            horiz = ScreenManager.Viewport.Project(horizV, Camera.projectionMatrix, Camera.ViewMatrixUpDownOnly(), Camera.worldMatrix);
            horizHeight = horiz.Y - (25 * Parallax.Scale);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(texList["blank"], new Rectangle(ScreenManager.Viewport.Width / 2, (int)horizHeight, ScreenManager.Viewport.Width * 2, (ScreenManager.Viewport.Height - (int)horizHeight) + 400), null, Track.GroundColor, 0f, new Vector2(0.5f, 0), SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.End();
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(new Vector3(Parallax.Scale, Parallax.Scale,1f)) * Matrix.CreateTranslation(new Vector3(ScreenManager.Viewport.Width / 2, horizHeight, 0f)));
            Parallax.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.End();

            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            

            if(Track.HasLoaded) Track.DrawBatches(ScreenManager.GraphicsDevice, drawEffect, drawAlphaEffect);

            ScreenManager.FadeBackBufferToBlack(0.5f);

            spriteBatch.Begin();
            spriteBatch.Draw(texList["banner-top"], bannerTop1Pos, null, Color.White, 0f, new Vector2(texList["banner-top"].Width / 2, 0f), 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["banner-bottom"], bannerBottom1Pos, null, Color.White, 0f, new Vector2(texList["banner-bottom"].Width / 2, 0f), 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["banner-top"], bannerTop2Pos, null, Color.White, 0f, new Vector2(texList["banner-top"].Width / 2, 0f), 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["banner-bottom"], bannerBottom2Pos, null, Color.White, 0f, new Vector2(texList["banner-bottom"].Width / 2, 0f), 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["paintcar"], bannerBottom1Pos - new Vector2(0, ((bannerBottom1Pos.Y - (bannerTop1Pos.Y + texList["banner-top"].Height)) / 2) - 1f), null, Color.White * paintCarAlpha, 0f, new Vector2(texList["paintcar"].Width, texList["paintcar"].Height)/2, 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["selectcup"], bannerBottom2Pos - new Vector2(0, ((bannerBottom2Pos.Y - (bannerTop2Pos.Y + texList["banner-top"].Height)) / 2) - 1f), null, Color.White * selectCupAlpha, 0f, new Vector2(texList["selectcup"].Width, texList["selectcup"].Height) / 2, 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(texList["car-spot"], SpotPos, null, Color.White, 0f, new Vector2(texList["car-spot"].Width , texList["car-spot"].Height)/2, 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texCarDirections[carFrame], CarPos, null, carColor, 0f, new Vector2(texCarDirections[carFrame].Width, texCarDirections[carFrame].Height) / 2, carScale, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["colors"], PaintPos, null, Color.White, 0f, new Vector2(texList["colors"].Width, texList["colors"].Height) / 2, 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(texList["triangles"], new Vector2(redRect.Left+4, redRect.Top + 24) + new Vector2((400f/230f) * (float)(carColor.R-25),0f), null, selectedColor==0?Color.White:Color.Black, 0f, new Vector2(texList["triangles"].Width, texList["triangles"].Height) / 2, 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["triangles"], new Vector2(greenRect.Left + 4, greenRect.Top + 24) + new Vector2((400f / 230f) * (float)(carColor.G - 25), 0f), null, selectedColor == 1 ? Color.White : Color.Black, 0f, new Vector2(texList["triangles"].Width, texList["triangles"].Height) / 2, 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["triangles"], new Vector2(blueRect.Left + 4, blueRect.Top + 24) + new Vector2((400f / 230f) * (float)(carColor.B - 25), 0f), null, selectedColor == 2 ? Color.White : Color.Black, 0f, new Vector2(texList["triangles"].Width, texList["triangles"].Height) / 2, 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(texList["arrow"], new Vector2(leftRect.Center.X, leftRect.Center.Y), null, Color.LightGray, 0f, new Vector2(texList["arrow"].Width, texList["arrow"].Height) / 2, 1f, SpriteEffects.FlipHorizontally, 1);
            spriteBatch.Draw(texList["arrow"], new Vector2(rightRect.Center.X, rightRect.Center.Y), null, Color.LightGray, 0f, new Vector2(texList["arrow"].Width, texList["arrow"].Height) / 2, 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(texList["cuparrow"], new Vector2(leftCupRect.Center.X, leftCupRect.Center.Y), null, Color.LightGray * (selectedCup>0?1f:0.2f), 0f, new Vector2(texList["cuparrow"].Width, texList["cuparrow"].Height) / 2, 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["cuparrow"], new Vector2(rightCupRect.Center.X, rightCupRect.Center.Y), null, Color.LightGray * (selectedCup < (Cups.Count-1) ? 1f : 0.2f), 0f, new Vector2(texList["cuparrow"].Width, texList["cuparrow"].Height) / 2, 1f, SpriteEffects.FlipHorizontally, 1);


            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.FrontToBack, null);
            foreach(Cup c in Cups)
            {
                spriteBatch.Draw(texList["cup-holder"], c.Position, null, Color.LightGray * c.Scale, 0f, new Vector2(texList["cup-holder"].Width, texList["cup-holder"].Height) / 2, c.Scale, SpriteEffects.None, c.Scale);
                spriteBatch.Draw(texList["cuptitle" + (Cups.IndexOf(c) + 1).ToString()], c.Position + new Vector2(0, -175 * c.Scale), null, Color.LightGray * c.Scale, 0f, new Vector2(texList["cuptitle" + (Cups.IndexOf(c) + 1).ToString()].Width, texList["cuptitle" + (Cups.IndexOf(c) + 1).ToString()].Height) / 2, c.Scale, SpriteEffects.None, c.Scale);

                if (selectionMode == 1 && Cups.IndexOf(c) == selectedCup)
                {
                    if(cupTracks[0].HasLoaded) spriteBatch.Draw(cupTrackRT[0], c.Position + new Vector2(0, -31 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(cupTrackRT[0].Width, cupTrackRT[0].Height) / 2, c.Scale, SpriteEffects.None, c.Scale);
                    else spriteBatch.Draw(texList["blank-track"], c.Position + new Vector2(0, -31 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(193, 108) / 2, c.Scale, SpriteEffects.None, c.Scale);
                    if (cupTracks[1].HasLoaded) spriteBatch.Draw(cupTrackRT[1], c.Position + new Vector2(0, 79 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(cupTrackRT[1].Width, cupTrackRT[1].Height) / 2, c.Scale, SpriteEffects.None, c.Scale);
                    else spriteBatch.Draw(texList["blank-track"], c.Position + new Vector2(0, 79 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(193, 108) / 2, c.Scale, SpriteEffects.None, c.Scale);
                    if (cupTracks[2].HasLoaded) spriteBatch.Draw(cupTrackRT[2], c.Position + new Vector2(0, 189 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(cupTrackRT[2].Width, cupTrackRT[2].Height) / 2, c.Scale, SpriteEffects.None, c.Scale);
                    else spriteBatch.Draw(texList["blank-track"], c.Position + new Vector2(0, 189 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(193, 108) / 2, c.Scale, SpriteEffects.None, c.Scale);
                }
                else
                {
                    spriteBatch.Draw(texList["blank-track"], c.Position + new Vector2(0, -31 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(193, 108) / 2, c.Scale, SpriteEffects.None, c.Scale);
                    spriteBatch.Draw(texList["blank-track"], c.Position + new Vector2(0, 79 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(193, 108) / 2, c.Scale, SpriteEffects.None, c.Scale);
                    spriteBatch.Draw(texList["blank-track"], c.Position + new Vector2(0, 189 * c.Scale), null, Color.White * c.Scale, 0f, new Vector2(193, 108) / 2, c.Scale, SpriteEffects.None, c.Scale);
                }
            }
            spriteBatch.End();

            ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }


        #endregion
    }
}
