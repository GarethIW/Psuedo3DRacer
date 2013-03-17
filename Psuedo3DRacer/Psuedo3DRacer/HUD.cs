using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Psuedo3DRacer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psuedo3DRacer
{
   
   

    public class HUD
    {
        Viewport viewport;

        SpriteFont fontHUD;

        Dictionary<string, Texture2D> texList = new Dictionary<string, Texture2D>();

        public float Alpha = 1f;

        float countdownFade;
        float countdownScale;
        int countdownNumber;
        bool countdownFadingIn = false;

        static Random randomNumber = new Random();

        public HUD(Viewport vp)
        {
            viewport = vp;
        }

        public void LoadContent(ContentManager content)
        {
            LoadTex("countdown", content);
            LoadTex("numbers", content);
            LoadTex("lappos", content);
        }

        void LoadTex(string name, ContentManager content)
        {
            texList.Add(name, content.Load<Texture2D>("hud/" + name));
        }

        public void Update(GameTime gameTime, double countdownTime)
        {
            if (countdownTime > 3400 && countdownTime < 3500)
            {
                countdownFade = 0f;
                countdownScale = 3f;
                countdownNumber = 3;
                countdownFadingIn = true;
            }
            if (countdownNumber == 3 && countdownTime < 3000) countdownFadingIn = false;
            if (countdownTime > 2400 && countdownTime < 2500)
            {
                countdownFade = 0f;
                countdownScale = 3f;
                countdownNumber = 2;
                countdownFadingIn = true;
            }
            if (countdownNumber == 2 && countdownTime < 2000) countdownFadingIn = false;
            if (countdownTime > 1400 && countdownTime < 1500)
            {
                countdownFade = 0f;
                countdownScale = 3f;
                countdownNumber = 1;
                countdownFadingIn = true;
            }
            if (countdownNumber == 1 && countdownTime < 1000) countdownFadingIn = false;
            if (countdownTime > 400 && countdownTime < 500)
            {
                countdownFade = 0f;
                countdownScale = 3f;
                countdownNumber = 0;
                countdownFadingIn = true;
            }
            if (countdownNumber == 0 && countdownTime <= 0) countdownFadingIn = false;

            if (countdownFadingIn && countdownFade < 1f) countdownFade += 0.1f;
            if ((!countdownFadingIn) && countdownFade > 0f) countdownFade -= 0.1f;
            if (countdownScale > 0f) countdownScale -= 0.05f;
        }


        public void Draw(SpriteBatch spriteBatch, Car playerCar)
        {
            Viewport vp = spriteBatch.GraphicsDevice.Viewport;
            
            if(countdownNumber>0)
                spriteBatch.Draw(texList["countdown"], new Vector2(vp.Width / 2, (vp.Height / 2) - 200f), new Rectangle((65 * (countdownNumber - 1)), 0, 64, 64), Color.White * countdownFade, 0f, new Vector2(32, 32), countdownScale, SpriteEffects.None, 1);

            if(countdownNumber==0)
                spriteBatch.Draw(texList["countdown"], new Vector2(vp.Width / 2, (vp.Height / 2) - 200f), new Rectangle(194, 0, 168, 64), Color.White * countdownFade, 0f, new Vector2(168/2, 32), countdownScale, SpriteEffects.None, 1);

            Vector2 hudOffset = new Vector2(vp.Width / 2, 50);

            if (!playerCar.Finished)
            {
                spriteBatch.Draw(texList["lappos"], new Vector2(50, 80), new Rectangle(82, 0, 75, 25), Color.White);
                spriteBatch.Draw(texList["numbers"], new Vector2(130, 40), new Rectangle(64 + (playerCar.RacePosition * 64), 0,64,64), Color.White);
                spriteBatch.Draw(texList["numbers"], new Vector2(180, 80), new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1);
                spriteBatch.Draw(texList["numbers"], new Vector2(210, 110), new Rectangle(64 + (8 * 64), 0, 64, 64), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1);

                int posFix = 0;
                if (playerCar.RacePosition == 1) posFix=0;
                else if (playerCar.RacePosition == 2) posFix=1;
                else if (playerCar.RacePosition == 3) posFix=2;
                else posFix=3;
                spriteBatch.Draw(texList["lappos"], new Vector2(195, 40), new Rectangle(0, 30 + (posFix * 37), 75, 38), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1);


                float lapOffset = vp.Width - 500;
                float lapScale = 0.8f;
                spriteBatch.Draw(texList["lappos"], new Vector2(lapOffset + (50*lapScale), 71), new Rectangle(0, 0, 75, 25), Color.White, 0f, Vector2.Zero, lapScale, SpriteEffects.None, 1);
                spriteBatch.Draw(texList["numbers"], new Vector2(lapOffset + (130*lapScale), 40), new Rectangle(64 + ((4-(playerCar.LapsToGo>0?playerCar.LapsToGo:3)) * 64), 0, 64, 64), Color.White, 0f, Vector2.Zero, 1f * lapScale, SpriteEffects.None, 1);
                spriteBatch.Draw(texList["numbers"], new Vector2(lapOffset + (190*lapScale), 53), new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 0.75f * lapScale, SpriteEffects.None, 1);
                spriteBatch.Draw(texList["numbers"], new Vector2(lapOffset + (240*lapScale), 53), new Rectangle(64 + (3 * 64), 0, 64, 64), Color.White, 0f, Vector2.Zero, 0.75f * lapScale, SpriteEffects.None, 1);

            }
            else
            {

            }

        }

     

        void ShadowText(SpriteBatch sb, string text, Vector2 pos, Color col, Vector2 off, float scale)
        {
            sb.DrawString(fontHUD, text, pos + (Vector2.One * 2f), new Color(0,0,0,col.A), 0f, off, scale, SpriteEffects.None, 1);
            sb.DrawString(fontHUD, text, pos, col, 0f, off, scale, SpriteEffects.None, 1);
        }
    }
}
