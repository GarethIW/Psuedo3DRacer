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
#endregion

namespace Psuedo3DRacer
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class PauseMenuScreen : MenuScreen
    {
        #region Initialization

        PauseBackgroundScreen BGScreen;
        MenuEntry sfxMenuEntry;
        MenuEntry musicMenuEntry;
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public PauseMenuScreen(PauseBackgroundScreen pauseBG)
            : base("Pause", 0)
        {
            BGScreen = pauseBG;
            IsPopup = true;
        }

        public override void LoadContent()
        {
            MenuEntry resumeGameMenuEntry;
            resumeGameMenuEntry = new MenuEntry("Resume");

            MenuEntry optionsMenuEntry = new MenuEntry("Options");
            sfxMenuEntry = new MenuEntry("SFX: "+ (AudioController.sfx ? "On" : "Off"));
            musicMenuEntry = new MenuEntry("Music: "+ (AudioController.music ? "On" : "Off"));
            MenuEntry exitMenuEntry = new MenuEntry("Quit Race");

            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += ResumeGameMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            sfxMenuEntry.Selected += SfxMenuEntrySelected;
            musicMenuEntry.Selected += MusicMenuEntrySelected;
            exitMenuEntry.Selected += ExitMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(sfxMenuEntry);
            MenuEntries.Add(musicMenuEntry);
            MenuEntries.Add(exitMenuEntry);

            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void ResumeGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            BGScreen.ExitScreen();
            ExitScreen();
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        void SfxMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
            AudioController.sfx = !AudioController.sfx;
            if (AudioController.sfx) AudioController.sfxvolume = 1f;
            else AudioController.sfxvolume = 0f;
            sfxMenuEntry.Text = "SFX: " + (AudioController.sfx ? "On" : "Off");
        }

        void MusicMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
            AudioController.music = !AudioController.music;
            if (AudioController.music) AudioController.musicvolume = 0.3f;
            else AudioController.musicvolume = 0f;
            musicMenuEntry.Text = "Music: " + (AudioController.music ? "On" : "Off");

        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void ExitMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            AudioController.StopMusic();
           LoadingScreen.Load(ScreenManager, false, e.PlayerIndex, new SelectionScreen(1));
        }


        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            BGScreen.ExitScreen();
            ExitScreen();
        }


        #endregion
    }
}
