using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psuedo3DRacer.Common
{
    public class ParallaxLayer
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float ScrollSpeed;
        public bool PositionFromBottom;
        public bool OneOnly;

        public ParallaxLayer(Texture2D tex, Vector2 pos, float speed, bool bottom, bool oneonly)
        {
            Texture = tex;
            Position = pos;
            ScrollSpeed = speed;
            PositionFromBottom = bottom;
            OneOnly = oneonly;
        }
    }

    public class ParallaxManager
    {
        public List<ParallaxLayer> Layers = new List<ParallaxLayer>();

        Viewport viewport;

        Vector2 scrollPosition;

        float rotation;

        public ParallaxManager(Viewport vp)
        {
            viewport = vp;
        }

        public void Update(GameTime gameTime, Vector2 scrollPos)
        {
            scrollPosition = scrollPos;// -new Vector2(GameManager.Camera.Width, GameManager.Camera.Height) / 2;
            //rotation = rot;

            foreach (ParallaxLayer l in Layers)
            {
                //l.Position.Y = scrollPosition.Y;
                //l.Position.X = scrollPosition.X * l.ScrollSpeed;
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {

            foreach (ParallaxLayer l in Layers)
            {
                if (!l.OneOnly)
                {
                    for (float x = (scrollPosition.X * l.ScrollSpeed) - (spriteBatch.GraphicsDevice.Viewport.Width*3); x < (scrollPosition.X * l.ScrollSpeed) + (spriteBatch.GraphicsDevice.Viewport.Width*3); x += l.Texture.Width)
                    {
                        //if (l.Position.X + x > -l.Texture.Width)
                        //{
                        spriteBatch.Draw(l.Texture, new Vector2(l.Position.X + x, l.Position.Y), null, Color.White, 0f, new Vector2(l.Texture.Width / 2, l.Texture.Height / 2), 1f, SpriteEffects.None, 1);
                        //}
                    }
                }
                else
                {
                    spriteBatch.Draw(l.Texture, new Vector2(l.Position.X + (scrollPosition.X*l.ScrollSpeed), l.Position.Y), null, Color.White, 0f, new Vector2(l.Texture.Width / 2, l.Texture.Height / 2), 1f, SpriteEffects.None, 1);
                }


                  
            }
        }
    }
}
