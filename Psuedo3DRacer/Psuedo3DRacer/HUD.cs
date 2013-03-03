using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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


        public void Draw(SpriteBatch spriteBatch)
        {
            Viewport vp = spriteBatch.GraphicsDevice.Viewport;
            
            if(countdownNumber>0)
                spriteBatch.Draw(texList["countdown"], new Vector2(vp.Width / 2, (vp.Height / 2) - 200f), new Rectangle((65 * (countdownNumber - 1)), 0, 64, 64), Color.White * countdownFade, 0f, new Vector2(32, 32), countdownScale, SpriteEffects.None, 1);

            if(countdownNumber==0)
                spriteBatch.Draw(texList["countdown"], new Vector2(vp.Width / 2, (vp.Height / 2) - 200f), new Rectangle(194, 0, 168, 64), Color.White * countdownFade, 0f, new Vector2(168/2, 32), countdownScale, SpriteEffects.None, 1);

        }

     

        void ShadowText(SpriteBatch sb, string text, Vector2 pos, Color col, Vector2 off, float scale)
        {
            sb.DrawString(fontHUD, text, pos + (Vector2.One * 2f), new Color(0,0,0,col.A), 0f, off, scale, SpriteEffects.None, 1);
            sb.DrawString(fontHUD, text, pos, col, 0f, off, scale, SpriteEffects.None, 1);
        }
    }
}
