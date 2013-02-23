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
    public class GameOverScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D texBG;

        double delayTime;

        bool isComplete;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameOverScreen(bool win)
        {
            isComplete = win;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

#if WINDOWS_PHONE || WINRT || TOUCH
            EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.Tap;
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

            if(isComplete)
                texBG = content.Load<Texture2D>("complete");
            else
                texBG = content.Load<Texture2D>("gameover");
            

           
            
           
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
                    LoadingScreen.Load(ScreenManager, false, null, new GameplayScreen(), new MainMenuScreen());
                }
            }

            

           
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void HandleInput(InputState input)
        {
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex) || input.IsMenuCancel(ControllingPlayer, out playerIndex) || input.TapPosition.HasValue || input.MouseLeftClick)
            {
                LoadingScreen.Load(ScreenManager, false, null, new GameplayScreen(), new MainMenuScreen());
            }
           

            base.HandleInput(input);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 0.2f);

            spriteBatch.Begin();

            //spriteBatch.Draw(texBG, fullscreen,
              //               Color.White * TransitionAlpha * (0.5f + (0.5f * TransitionPosition)));

            ScreenManager.SpriteBatch.Draw(texBG, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2), null, Color.White * TransitionAlpha, 0f, new Vector2(texBG.Width, texBG.Height) / 2, 1f + (5f * (!IsExiting?TransitionPosition:0f)), SpriteEffects.None, 1);
           

            spriteBatch.End();

            //ScreenManager.FadeBackBufferToBlack(1f-TransitionAlpha);
        }


        #endregion
    }
}
