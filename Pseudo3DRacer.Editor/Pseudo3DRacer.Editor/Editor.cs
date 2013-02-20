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
using Psuedo3DRacer.Common;

namespace Pseudo3DRacer
{
    public enum EditorMode
    {
        Construction,
        Painting,
        Testing
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Editor : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        EditorMode Mode = EditorMode.Construction;
        bool lockCameraToCar = true;
        Camera Camera = new Camera();
        Track Track;

        BasicEffect drawEffect;
        AlphaTestEffect drawAlphaEffect;

        List<Vector3> ControlPoints = new List<Vector3> { new Vector3(0, 0, 20), new Vector3(-20, 0, 0), new Vector3(0, 0, -20), new Vector3(20, 0, 0) };

        KeyboardState lks;
        MouseState lms;

        SpriteFont spriteFont;
        Texture2D texGrid;
        Texture2D texBlank;
        Model handleSphere;

        int selectedPoint = 0;
        int currentTrackPos = 0;
        int paintPos = 0;

        Matrix[] sphereTransforms;

        SceneryBrush LeftBrush = SceneryBrush.None;
        SceneryBrush RightBrush = SceneryBrush.None;
        RoadBrush RoadBrush = RoadBrush.Road;
        AboveBrush AboveBrush = AboveBrush.None;

        Car car;

        public Editor()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
            Content.RootDirectory = "Psuedo3DRacer.Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Mode = EditorMode.Construction;

            lks = Keyboard.GetState();
            lms = Mouse.GetState();

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

            texGrid = Content.Load<Texture2D>("grid");
            texBlank = Content.Load<Texture2D>("blank");
            handleSphere = Content.Load<Model>("spherelowpoly");
            spriteFont = Content.Load<SpriteFont>("gamefont");

            sphereTransforms = new Matrix[handleSphere.Bones.Count];
            handleSphere.CopyAbsoluteBoneTransformsTo(sphereTransforms);

            drawEffect = new BasicEffect(GraphicsDevice)
            {
                World = Camera.worldMatrix,
                View = Camera.viewMatrix,
                Projection = Camera.projectionMatrix,
                TextureEnabled = true
            };

            drawAlphaEffect = new AlphaTestEffect(GraphicsDevice)
            {
                World = Camera.worldMatrix,
                View = Camera.viewMatrix,
                Projection = Camera.projectionMatrix,
                 ReferenceAlpha = 254,
                   AlphaFunction = CompareFunction.Greater
                 
            };

            Track = Track.BuildFromControlPoints(ControlPoints);
            Track.LoadContent(Content);

            car = new Car(0, Track);
            car.LoadContent(Content, 1);
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState cks = Keyboard.GetState();
            MouseState cms = Mouse.GetState();
            Vector2 mousePos = new Vector2(cms.X, cms.Y);
            Vector2 mousedelta = mousePos - new Vector2(lms.X, lms.Y);
            int wheelDelta = cms.ScrollWheelValue - lms.ScrollWheelValue;

            //Mode select
            if (cks.IsKeyDown(Keys.F1) && !lks.IsKeyDown(Keys.F1))
            {
                Mode = EditorMode.Construction;
            }
            if (cks.IsKeyDown(Keys.F2) && !lks.IsKeyDown(Keys.F2))
            {
                Mode = EditorMode.Painting;
                currentTrackPos = 0;
                paintPos = 0;
            }
            if (cks.IsKeyDown(Keys.F3) && !lks.IsKeyDown(Keys.F3))
            {
                Mode = EditorMode.Testing;
                //car = new car(0, Track);
            }

            if (Mode == EditorMode.Construction)// || Mode == EditorMode.Testing)
            {
                if (cks.IsKeyDown(Keys.PageUp) && !lks.IsKeyDown(Keys.PageUp))
                {
                    selectedPoint++;
                    if (selectedPoint == ControlPoints.Count) selectedPoint = 0;
                }
                if (cks.IsKeyDown(Keys.PageDown) && !lks.IsKeyDown(Keys.PageDown))
                {
                    selectedPoint--;
                    if (selectedPoint == -1) selectedPoint = ControlPoints.Count - 1;
                }
                if (cks.IsKeyDown(Keys.Space) && !lks.IsKeyDown(Keys.Space))
                {
                    int pos = selectedPoint;
                    int nextpos = selectedPoint + 1;
                    if (pos >= ControlPoints.Count) pos = pos - (ControlPoints.Count);
                    if (nextpos >= ControlPoints.Count) nextpos = nextpos - ControlPoints.Count;
                    Vector3 delta = (ControlPoints[nextpos] - ControlPoints[pos]);
                    delta.X = delta.X / 2;
                    delta.Z = delta.Z / 2;
                    ControlPoints.Insert(selectedPoint + 1, ControlPoints[pos] + delta);
                    selectedPoint++;
                    Track.Rebuild(ControlPoints);
                }

                Vector3 moveVector = new Vector3(0, 0, 0);
                if (cks.IsKeyDown(Keys.Up) || cks.IsKeyDown(Keys.W))
                    moveVector += new Vector3(0, 0, -1);
                if (cks.IsKeyDown(Keys.Down) || cks.IsKeyDown(Keys.S))
                    moveVector += new Vector3(0, 0, 1);
                if (cks.IsKeyDown(Keys.Right) || cks.IsKeyDown(Keys.D))
                    moveVector += new Vector3(1, 0, 0);
                if (cks.IsKeyDown(Keys.Left) || cks.IsKeyDown(Keys.A))
                    moveVector += new Vector3(-1, 0, 0);
                Camera.AddToPosition(moveVector);

                if (cms.RightButton == ButtonState.Pressed)
                {
                    Vector3 pos = ControlPoints[selectedPoint];
                    pos.X += mousedelta.X * 0.05f;
                    pos.Z += mousedelta.Y * 0.05f;
                    ControlPoints[selectedPoint] = pos;

                    if (wheelDelta != 0)
                    {
                        if (wheelDelta > 0)
                            pos.Y += 1;
                        else
                            pos.Y -= 1;
                        ControlPoints[selectedPoint] = pos;
                    }
                    Track.Rebuild(ControlPoints);
                }
                else
                {
                    if (wheelDelta != 0)
                    {
                        if (wheelDelta > 0)
                        {
                            selectedPoint++;
                            if (selectedPoint == ControlPoints.Count) selectedPoint = 0;
                        }
                        else
                        {
                            selectedPoint--;
                            if (selectedPoint == -1) selectedPoint = ControlPoints.Count - 1;
                        }
                    }
                }

                if (cms.LeftButton == ButtonState.Pressed)
                {
                    Camera.Rotate(mousedelta.X, mousedelta.Y);
                }
            }

            if (Mode == EditorMode.Painting)
            {
                if (cks.IsKeyDown(Keys.Up) || cks.IsKeyDown(Keys.W))
                {
                    currentTrackPos++;
                    paintPos = currentTrackPos;
                }
                if (cks.IsKeyDown(Keys.Down) || cks.IsKeyDown(Keys.S))
                {
                    currentTrackPos--;
                    paintPos = currentTrackPos;
                }
                if (cks.IsKeyDown(Keys.Insert) && !lks.IsKeyDown(Keys.Insert))
                {
                    LeftBrush++;
                    if((int)LeftBrush > Enum.GetValues(typeof(SceneryBrush)).Length-1) LeftBrush = 0;
                }
                if (cks.IsKeyDown(Keys.Delete) && !lks.IsKeyDown(Keys.Delete))
                {
                    LeftBrush--;
                    if ((int)LeftBrush < 0) LeftBrush = (SceneryBrush)Enum.GetValues(typeof(SceneryBrush)).Length - 1;
                }
                if (cks.IsKeyDown(Keys.Home) && !lks.IsKeyDown(Keys.Home))
                {
                    RoadBrush++;
                    if ((int)RoadBrush > Enum.GetValues(typeof(RoadBrush)).Length - 1) RoadBrush = 0;
                }
                if (cks.IsKeyDown(Keys.End) && !lks.IsKeyDown(Keys.End))
                {
                    RoadBrush--;
                    if ((int)RoadBrush < 0) RoadBrush = (RoadBrush)Enum.GetValues(typeof(RoadBrush)).Length - 1;
                }
                if (cks.IsKeyDown(Keys.PageUp) && !lks.IsKeyDown(Keys.PageUp))
                {
                    RightBrush++;
                    if ((int)RightBrush > Enum.GetValues(typeof(SceneryBrush)).Length - 1) RightBrush = 0;
                }
                if (cks.IsKeyDown(Keys.PageDown) && !lks.IsKeyDown(Keys.PageDown))
                {
                    RightBrush--;
                    if ((int)RightBrush < 0) RightBrush = (SceneryBrush)Enum.GetValues(typeof(SceneryBrush)).Length - 1;
                }
                if (cks.IsKeyDown(Keys.NumPad7) && !lks.IsKeyDown(Keys.NumPad7))
                {
                    AboveBrush++;
                    if ((int)AboveBrush > Enum.GetValues(typeof(AboveBrush)).Length - 1) AboveBrush = 0;
                }
                if (cks.IsKeyDown(Keys.NumPad4) && !lks.IsKeyDown(Keys.NumPad4))
                {
                    AboveBrush--;
                    if ((int)AboveBrush < 0) AboveBrush = (AboveBrush)Enum.GetValues(typeof(AboveBrush)).Length - 1;
                }

                if (currentTrackPos == Track.Length) currentTrackPos = 0;
                if (currentTrackPos == -1) currentTrackPos = Track.Length - 1;

                if (cks.IsKeyDown(Keys.Space))
                {
                    Track.TrackSegments[paintPos].Paint(paintPos, RoadBrush, AboveBrush, LeftBrush, RightBrush);
                    paintPos++;
                    if (paintPos > Track.TrackSegments.Count - 1) paintPos = 0;
                }

                int nextpos = (currentTrackPos + 1);
                if (nextpos >= Track.TrackSegments.Count) nextpos = nextpos - Track.TrackSegments.Count;
                //Camera.viewMatrix = Matrix.CreateLookAt(Track.TrackSegments[currentTrackPos].Position + new Vector3(0, 0.5f, 0),
                //                                        Track.TrackSegments[nextpos].Position + new Vector3(0, 0.5f, 0),
                //                                        Vector3.Up);
                Camera.Position = Track.TrackSegments[currentTrackPos].Position + new Vector3(0, 0.5f, 0);
                Camera.LookAt(Track.TrackSegments[nextpos].Position + new Vector3(0, 0.5f, 0));

            }

            if (Mode == EditorMode.Testing)
            {
                if (!lockCameraToCar)
                {
                    Vector3 moveVector = new Vector3(0, 0, 0);
                    if (cks.IsKeyDown(Keys.Up) || cks.IsKeyDown(Keys.W))
                        moveVector += new Vector3(0, 0, -1);
                    if (cks.IsKeyDown(Keys.Down) || cks.IsKeyDown(Keys.S))
                        moveVector += new Vector3(0, 0, 1);
                    if (cks.IsKeyDown(Keys.Right) || cks.IsKeyDown(Keys.D))
                        moveVector += new Vector3(1, 0, 0);
                    if (cks.IsKeyDown(Keys.Left) || cks.IsKeyDown(Keys.A))
                        moveVector += new Vector3(-1, 0, 0);
                    Camera.AddToPosition(moveVector);

                    if (cms.LeftButton == ButtonState.Pressed)
                    {
                        Camera.Rotate(mousedelta.X, mousedelta.Y);
                    }
                }

                if (cks.IsKeyDown(Keys.Space) && !lks.IsKeyDown(Keys.Space))
                {
                    lockCameraToCar = !lockCameraToCar;
                }
            }

            lks = cks;
            lms = cms;

            if (Mode == EditorMode.Testing)
            {
                car.Update(gameTime, Track);
                if (lockCameraToCar)
                {
                    Camera.Position = car.CameraPosition;
                    Camera.LookAt(car.CameraLookat);
                }
            }

            drawEffect.View = Camera.viewMatrix;
            drawAlphaEffect.View = Camera.viewMatrix;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw horizon, yo
            Vector3 horizV = new Vector3(0, 0f, -200);
            Vector3 horiz = GraphicsDevice.Viewport.Project(horizV, Camera.projectionMatrix, Camera.ViewMatrixUpDownOnly(), Camera.worldMatrix);
            float horizHeight = horiz.Y;
            spriteBatch.Begin();
            spriteBatch.Draw(texBlank, new Rectangle(0, (int)horizHeight, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height - (int)horizHeight), Color.Green);
            spriteBatch.End();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
           
            

            if (Mode == EditorMode.Construction)
            {
                // Draw grid
                drawEffect.Texture = texGrid;
                drawEffect.DiffuseColor = Color.White.ToVector3();
                Quad quad = new Quad(new Vector3(0,-0.1f,0), Vector3.Up, Vector3.Forward, 200f, 200f);
                Drawing.DrawQuad(drawEffect, quad, GraphicsDevice);
                quad = new Quad(Vector3.Zero, Vector3.Down, Vector3.Forward, 200f, 200f);
                Drawing.DrawQuad(drawEffect, quad, GraphicsDevice);
            }

            

            // Draw track
            if (Mode == EditorMode.Construction)
            {
                Track.DrawRoad(GraphicsDevice, drawEffect, 0, Track.Length);



                Track.DrawScenery(GraphicsDevice, drawAlphaEffect, 0, Track.Length);
            }
            if (Mode == EditorMode.Painting || Mode == EditorMode.Testing)
            {
                Track.DrawRoad(GraphicsDevice, drawEffect, currentTrackPos, Track.Length);
                car.Draw(GraphicsDevice, drawAlphaEffect, Camera);
                Track.DrawScenery(GraphicsDevice, drawAlphaEffect, currentTrackPos, Track.Length);

            }

            if (Mode == EditorMode.Testing)
            {
               
            }

            if (Mode == EditorMode.Construction)
            {
                // Control point Handles
                foreach (Vector3 p in ControlPoints)
                {
                    foreach (ModelMesh mesh in handleSphere.Meshes)
                    {
                        foreach (BasicEffect eff in mesh.Effects)
                        {
                            eff.EnableDefaultLighting();

                            if (ControlPoints.IndexOf(p) == selectedPoint)
                                eff.DiffuseColor = new Vector3(0, 255, 0);
                            else
                                eff.DiffuseColor = new Vector3(255, 0, 0);

                            eff.View = Camera.viewMatrix;
                            eff.Projection = Camera.projectionMatrix;
                            eff.World = Camera.worldMatrix *
                                sphereTransforms[mesh.ParentBone.Index] *
                                Matrix.CreateScale(0.1f) *
                                Matrix.CreateTranslation(p + new Vector3(0, 0.75f, 0));
                        }
                        mesh.Draw();
                    }
                }
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, Enum.GetName(typeof(EditorMode), Mode), new Vector2(10, 5), Color.White);

            if(Mode == EditorMode.Construction)
                spriteBatch.DrawString(spriteFont, ControlPoints[selectedPoint].ToString() + " | " + car.debug, new Vector2(10,25), Color.White);

            if (Mode == EditorMode.Painting)
            {
                spriteBatch.DrawString(spriteFont, Enum.GetName(typeof(SceneryBrush), LeftBrush) + " | " + Enum.GetName(typeof(RoadBrush), RoadBrush) + " | " + Enum.GetName(typeof(AboveBrush), AboveBrush) + " | " + Enum.GetName(typeof(SceneryBrush), RightBrush), new Vector2(10, 25), Color.White);
            }

            if(Mode== EditorMode.Testing)
                spriteBatch.DrawString(spriteFont, car.Yaw.ToString() + "," + car.Pitch.ToString() + " | " + car.courseTrackPos + "/" + Track.Length, new Vector2(10, 25), Color.White);


            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
