﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;


namespace Psuedo3DRacer
{
    public static class AudioController
    {
        public static bool sfx = true;
        public static bool music = true;

        public static float sfxvolume = 1f;
        public static float musicvolume = 0.3f;

        public static Random randomNumber = new Random();

        public static Dictionary<string, SoundEffect> effects;

        public static Dictionary<string, SoundEffectInstance> songs;

        //static SongCollection songs = new SongCollection();

        static string playingTrack = "";
        public static bool isPlaying;

        public static string currentlyPlaying = "";

        public static int currentTrack = 0;

        public static void LoadContent(ContentManager content)
        {
            effects = new Dictionary<string, SoundEffect>();

            effects.Add("countdown", content.Load<SoundEffect>("audio/sfx/countdown"));
            effects.Add("select", content.Load<SoundEffect>("audio/sfx/select"));

            


            songs = new Dictionary<string, SoundEffectInstance>();
            songs.Add("title", content.Load<SoundEffect>("audio/music/title").CreateInstance());
            songs.Add("cityday", content.Load<SoundEffect>("audio/music/cityday").CreateInstance());
            songs.Add("citynight", content.Load<SoundEffect>("audio/music/citynight").CreateInstance());
            songs.Add("island", content.Load<SoundEffect>("audio/music/island").CreateInstance());
            songs.Add("urban", content.Load<SoundEffect>("audio/music/urban").CreateInstance());

            foreach(SoundEffectInstance s in songs.Values)
                s.IsLooped = true;
            //MediaPlayer.IsRepeating = true;
        }

        public static void LoadMusic(string piece, ContentManager content)
        {
            //if (currentlyPlaying.ToLower() == piece.ToLower()) return;
            //currentlyPlaying = piece;

            //if (!MediaPlayer.GameHasControl) return;

            //if (MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();
            ////if (musicInstance != null)
            ////{
            ////    musicInstance.Dispose();
            ////}

            //musicInstance = content.Load<Song>("audio/music/" + piece);
            //MediaPlayer.IsRepeating = true;
            //// MediaPlayer.Volume = musicvolume;
            //MediaPlayer.Play(musicInstance);

            //if (!OptionsMenuScreen.music) MediaPlayer.Pause();
        }

        public static void PlayMusic()
        {
            //PlayMusic(currentTrack.ToString());
            //currentTrack++;
            //if (currentTrack == 5) currentTrack = 0;
        }

        public static void PlayMusic(string track)
        {
            playingTrack = track;
            isPlaying = true;
            
            songs[track].Volume = musicvolume;
            songs[track].Play();
        }

        public static void RestartMusic(string track)
        {
            songs[track].Stop();
            playingTrack = track;
            isPlaying = true;
            songs[track].Volume = musicvolume;
            songs[track].Play();
        }

        public static void StopMusic()
        {
            
            isPlaying = false;
        }

        public static void ToggleMusic()
        {

            //if (OptionsMenuScreen.music)
            //{
            //    MediaPlayer.Resume();
            //}
            //else
            //    MediaPlayer.Pause();
        }

        public static void PlaySFX(string name)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, 0f, 0f);
        }
        public static void PlaySFX(string name, float pitch)
        {
            //if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, pitch, 0f);
        }
        public static void PlaySFX(string name, float volume, float pitch, float pan)
        {
           // if (OptionsMenuScreen.sfx)
            if (pan < -1f || pan > 1f) return;
            volume = MathHelper.Clamp(volume, 0f, 1f);
            effects[name].Play(volume * sfxvolume, pitch, pan);
        }
        public static void PlaySFX(string name, float minpitch, float maxpitch)
        {
           // if (OptionsMenuScreen.sfx)
                effects[name].Play(sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), 0f);
        }

        internal static void PlaySFX(string name, float volume, float minpitch, float maxpitch, Vector2 Position)
        {
            //Vector2 screenPos = Vector2.Transform(Position, GameManager.Camera.CameraMatrix);
            //float pan = MathHelper.Clamp((screenPos.X - (GameManager.Camera.Width / 2)) / (GameManager.Camera.Width/2), -1f,1f);
            //effects[name].Play(volume * sfxvolume, minpitch + ((float)randomNumber.NextDouble() * (maxpitch - minpitch)), pan);
        }


        public static void Update(GameTime gameTime)
        {

            if (playingTrack == "") return;

            if (isPlaying)
            {
                if (songs[playingTrack].Volume < musicvolume) songs[playingTrack].Volume = MathHelper.Clamp(songs[playingTrack].Volume + 0.01f, 0f, 1f);
                if (songs[playingTrack].Volume > musicvolume) songs[playingTrack].Volume = MathHelper.Clamp(songs[playingTrack].Volume - 0.01f,0f,1f);
            }

            if (!isPlaying)
            {
                foreach (SoundEffectInstance s in songs.Values)
                {
                    if (s.Volume > 0.1f) s.Volume -= 0.01f;
                    else s.Stop();
                }
            }

            if (MediaPlayer.Volume > musicvolume) MediaPlayer.Volume = musicvolume;
        }

        public static void Unload()
        {

        }



       
    }
}
