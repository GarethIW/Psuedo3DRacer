#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Collections.Generic;
using Psuedo3DRacer.Common;
using System.Linq;
#if WINRT
using Windows.System.Threading;
#endif
#endregion

namespace Psuedo3DRacer
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class StandingsScreen : GameScreen
    {
        #region Fields

        ContentManager content;

        double delayTime;

        List<Car> gameCars;

        int currentTrack;

        int currentScreen = 0;
        bool swappingScreens = false;
        float currentScreenTransition = 1f;

        Texture2D texBlank;
        Texture2D texBanner;
        Texture2D texRace;
        Texture2D texCup;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public StandingsScreen(List<Car> cars, int tracknum)
        {
            

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(1);

            IsPopup = true;
            IsNonBlocking = true;

#if WINDOWS_PHONE || WINRT || TOUCH
            EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.Tap;
#endif

            currentTrack = tracknum;
            gameCars = cars;

            currentScreen = 0;
            currentScreenTransition = 1f;
            
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

            //if(isComplete)
            //    texBG = content.Load<Texture2D>("complete");
            //else
            //    texBG = content.Load<Texture2D>("gameover");


            texBlank = content.Load<Texture2D>("blank");
            texBanner = content.Load<Texture2D>("standings-banner");
            texRace = content.Load<Texture2D>("standings-race");
            texCup = content.Load<Texture2D>("standings-cup");
            
           
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
            if (IsActive)
            {
                delayTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (delayTime > 5000)
                {
                    delayTime = 0;
                    //LoadingScreen.Load(ScreenManager, false, null, new GameplayScreen(), new MainMenuScreen());
                }

                if (!IsExiting)
                {
                    if (currentScreen == 0)
                    {
                        if (swappingScreens)
                        {
                            currentScreenTransition = MathHelper.Lerp(currentScreenTransition, 1f, 0.1f);
                            if (currentScreenTransition >= 0.99f) currentScreen = 1;
                        }
                        else
                        {
                            currentScreenTransition = MathHelper.Lerp(currentScreenTransition, 0f, 0.1f);
                        }
                    }
                    else
                    {
                        currentScreenTransition = MathHelper.Lerp(currentScreenTransition, 0f, 0.1f);
                    }
                }
                else currentScreenTransition = MathHelper.Lerp(currentScreenTransition, 1f, 0.1f);
            }
            else currentScreenTransition = MathHelper.Lerp(currentScreenTransition, 1f, 0.1f);
            

           
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void HandleInput(InputState input)
        {
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex) || input.IsMenuCancel(ControllingPlayer, out playerIndex) || input.TapPosition.HasValue || input.MouseLeftClick)
            {
              //  LoadingScreen.Load(ScreenManager, false, null, new GameplayScreen(), new MainMenuScreen());
                if (currentScreen == 0)
                    swappingScreens = true;
                else
                    ExitScreen();
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

            //ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 0.2f);

            spriteBatch.Begin();

            //spriteBatch.Draw(texBG, fullscreen,
              //               Color.White * TransitionAlpha * (0.5f + (0.5f * TransitionPosition)));

            //ScreenManager.SpriteBatch.Draw(texBG, new Vector2(ScreenManager.Viewport.Width / 2, ScreenManager.Viewport.Height / 2), null, Color.White * TransitionAlpha, 0f, new Vector2(texBG.Width, texBG.Height) / 2, 1f + (5f * (!IsExiting?TransitionPosition:0f)), SpriteEffects.None, 1);

            //ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "lol", Vector2.One * 20, Color.White);

            float y = 180f;
            if (currentScreen == 0)
            {
                foreach (Car c in gameCars.OrderBy(car => car.RacePosition))
                {
                    string posText = c.RacePosition.ToString();
                    if(c.RacePosition==1) posText+="st";
                    else if (c.RacePosition==2) posText+="nd";
                    else if (c.RacePosition==3) posText+="rd";
                    else posText+="th";

                    Rectangle bgRect = new Rectangle((int)((viewport.Width / 2) - 500 + ((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition), (int)y - 28, 800, 50);

                    spriteBatch.Draw(texBanner, new Vector2((viewport.Width / 2) - 100, 90) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), null, Color.LightGray, 0f, new Vector2(texBanner.Width, texBanner.Height) / 2, 1f, SpriteEffects.None, 1);
                    spriteBatch.Draw(texRace, new Vector2((viewport.Width / 2) - 100, 90) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), null, Color.White, 0f, new Vector2(texBanner.Width, texBanner.Height) / 2, 1f, SpriteEffects.None, 1);

                    if(gameCars.IndexOf(c)==7)
                        spriteBatch.Draw(texBlank, bgRect, null, Color.DarkGray * 0.5f);                        
                    ShadowText(spriteBatch, posText, new Vector2((viewport.Width / 2) - 200, y) + new Vector2(((-viewport.Width ) - (50 * c.RacePosition)) * currentScreenTransition, 0f), Color.White, ScreenManager.Font.MeasureString(posText) / 2, 1f);
                    ShadowText(spriteBatch, "+" + (9 - c.RacePosition).ToString() + "pts", new Vector2((viewport.Width/2) + 200, y) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), Color.White, ScreenManager.Font.MeasureString("+" + (9 - c.RacePosition).ToString() + "pts") / 2, 1f);
                    spriteBatch.Draw(c.texDirections1[2], new Vector2((viewport.Width / 2) - 0, y) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), null, c.Tint, 0f, new Vector2(c.texDirections1[2].Width, c.texDirections1[2].Height) / 2, 0.3f, SpriteEffects.None, 1);
                    y+=55;
                }
            }
            else
            {
                int pos = 1;
                foreach (Car c in gameCars.OrderByDescending(car => car.CupPoints))
                {
                    string posText = pos.ToString();
                    if (pos == 1) posText += "st";
                    else if (pos == 2) posText += "nd";
                    else if (pos == 3) posText += "rd";
                    else posText += "th";

                    Rectangle bgRect = new Rectangle((int)((viewport.Width / 2) - 500 + ((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition), (int)y - 28, 800, 50);

                    spriteBatch.Draw(texBanner, new Vector2((viewport.Width / 2) - 100, 90) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), null, Color.LightGray, 0f, new Vector2(texBanner.Width, texBanner.Height) / 2, 1f, SpriteEffects.None, 1);
                    spriteBatch.Draw(texCup, new Vector2((viewport.Width / 2) - 100, 90) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), null, Color.White, 0f, new Vector2(texBanner.Width, texBanner.Height) / 2, 1f, SpriteEffects.None, 1);

                    if (gameCars.IndexOf(c) == 7)
                        spriteBatch.Draw(texBlank, bgRect, null, Color.DarkGray * 0.5f);
                    ShadowText(spriteBatch, posText, new Vector2((viewport.Width / 2) - 200, y) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), Color.White, ScreenManager.Font.MeasureString(posText) / 2, 1f);
                    ShadowText(spriteBatch, c.CupPoints.ToString() + "pts", new Vector2((viewport.Width / 2) + 200, y) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), Color.White, ScreenManager.Font.MeasureString("+" + (9 - c.RacePosition).ToString() + "pts") / 2, 1f);
                    spriteBatch.Draw(c.texDirections1[2], new Vector2((viewport.Width / 2) - 0, y) + new Vector2(((-viewport.Width) - (50 * c.RacePosition)) * currentScreenTransition, 0f), null, c.Tint, 0f, new Vector2(c.texDirections1[2].Width, c.texDirections1[2].Height) / 2, 0.3f, SpriteEffects.None, 1);
                    y += 55;
                    pos++;
                }
            }

            spriteBatch.End();

            //ScreenManager.FadeBackBufferToBlack(1f-TransitionAlpha);
        }

        void ShadowText(SpriteBatch sb, string text, Vector2 pos, Color col, Vector2 off, float scale)
        {
            sb.DrawString(ScreenManager.Font, text, pos + (Vector2.One * 2f), new Color(0, 0, 0, col.A), 0f, off, scale, SpriteEffects.None, 1);
            sb.DrawString(ScreenManager.Font, text, pos, col, 0f, off, scale, SpriteEffects.None, 1);
        }

        #endregion
    }
}
