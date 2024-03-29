﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

#if !WINRT
using System.ComponentModel;
#else
using System.Threading.Tasks;
using Windows.System.Threading;
#endif

namespace Psuedo3DRacer.Common
{
    public enum Horizon
    {
        CityDay,
        CityNight,
        Island,
        Urban,
        Arctic
    }

    public enum BatchEffectType
    {
        Basic,
        Alpha
    }

    public class Batch
    {
        public BatchEffectType effectType;
        public string textureName;
        public Vector3 diffuseColor;

        public List<Quad> quads = new List<Quad>();

        public VertexPositionNormalTexture[] vpnts;
        public short[] indexes;
    }

    public class Track
    {
        const int SEGMENTS_PER100M = 1000;

        public int Length = 0;

        public string FileName = "";

        public string TrackName = "";

        public Color GroundColor = Color.Green;

        public Color SkyColor = Color.CornflowerBlue;

        public Horizon Horizon = Horizon.CityDay;

        [XmlIgnore]
        public Texture2D mapSegment;

        [XmlIgnore]
        public List<Segment> TrackSegments = new List<Segment>();

        public List<string> PackedSegments = new List<string>();

        public List<Vector3> ControlPoints = new List<Vector3>();

        [XmlIgnore]
        Dictionary<string, Texture2D> textDict = new Dictionary<string, Texture2D>();

        [XmlIgnore]
        public List<Batch> Batches = new List<Batch>();

        [XmlIgnore]
        public bool HasLoaded;

        [XmlIgnore]
        float mapscale = 7f;
        [XmlIgnore]
        Vector2 mapoffset = Vector2.Zero;

#if !WINRT
        BackgroundWorker bgW = new BackgroundWorker();
#endif

        public void LoadContent(ContentManager content)
        {
            textDict.Add("road-normal", content.Load<Texture2D>("scenery/road-normal"));
            textDict.Add("road-wood", content.Load<Texture2D>("scenery/road-wood"));
            textDict.Add("tunnel", content.Load<Texture2D>("scenery/tunnel"));
            textDict.Add("tree", content.Load<Texture2D>("scenery/tree"));
            textDict.Add("palmtree", content.Load<Texture2D>("scenery/palmtree"));
            textDict.Add("girder", content.Load<Texture2D>("scenery/girder"));
            textDict.Add("sign-left", content.Load<Texture2D>("scenery/sign-left"));
            textDict.Add("sign-right", content.Load<Texture2D>("scenery/sign-right"));
            textDict.Add("start", content.Load<Texture2D>("scenery/start"));
            textDict.Add("wall", content.Load<Texture2D>("scenery/wall"));
            textDict.Add("tunnel-upper", content.Load<Texture2D>("scenery/tunnel-upper"));
            textDict.Add("ground", content.Load<Texture2D>("scenery/blank-ground"));
            textDict.Add("building", content.Load<Texture2D>("scenery/building"));
            textDict.Add("buildingcorner", content.Load<Texture2D>("scenery/building"));
            textDict.Add("crossroad", content.Load<Texture2D>("scenery/crossroad"));
            textDict.Add("lamppost-left", content.Load<Texture2D>("scenery/lamppost-left"));
            textDict.Add("lamppost-right", content.Load<Texture2D>("scenery/lamppost-right"));


            mapSegment = content.Load<Texture2D>("map-segment");
        }

        public void LoadHorizon(ContentManager content, ParallaxManager parallaxManager)
        {
            parallaxManager.Layers.Clear();

            switch (Horizon)
            {
                case Horizon.CityDay:
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/ground"), new Vector2(0,50f), 1f, false, false));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/sky"), new Vector2(0, -100f), 1f, false, false));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/mountains"), new Vector2(0f, -40f), 1f, false, false));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds1"), new Vector2(-1280f, -290f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds2"), new Vector2(2560f, -200f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds2"), new Vector2(-2560f, -200f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds3"), new Vector2(0f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/city"), new Vector2(200f, -80f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/city"), new Vector2(520f, -80f), 1f, false, true));
                    GroundColor = Color.Green;
                    SkyColor = Color.CornflowerBlue;
                    break;
                case Horizon.CityNight:
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/ground"), new Vector2(0, 25f), 1f, false, false));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/mountains"), new Vector2(0f, -40f), 1f, false, false));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/stars2"), new Vector2(0f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/stars"), new Vector2(1280f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/stars2"), new Vector2(2560f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/stars"), new Vector2(-1280f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/stars2"), new Vector2(-2560f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/citynight/city"), new Vector2(0f, -40f), 1f, false, true));
                    GroundColor = new Color(10,10,50);
                    SkyColor = new Color(10,10,20);
                    break;
                case Horizon.Island:
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/island/sky"), new Vector2(0f, -150f), 1f, false, false));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds1"), new Vector2(-1280f, -290f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds2"), new Vector2(2560f, -200f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds2"), new Vector2(-2560f, -200f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/cityday/clouds3"), new Vector2(0f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/island/boat"), new Vector2(0f, -85f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/island/beach"), new Vector2(0f, 0f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/island/beach"), new Vector2(1280f, 0f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/island/beach"), new Vector2(2560f, 0f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/island/beach"), new Vector2(-1280f, 0f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/island/beach"), new Vector2(-2560f, 0f), 1f, false, true));
                    
                    GroundColor = new Color(183, 159, 0);
                    SkyColor = new Color(0, 175, 219);
                    break;
                case Horizon.Urban:
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/urban/sky"), new Vector2(0, -100f), 1f, false, false));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/urban/clouds1"), new Vector2(-1280f, -290f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/urban/clouds2"), new Vector2(2560f, -200f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/urban/clouds2"), new Vector2(-2560f, -200f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/urban/clouds3"), new Vector2(0f, -250f), 1f, false, true));
                    parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("horizons/urban/city"), new Vector2(0f, -80f), 1f, false, false));
                    GroundColor = Color.Gray;
                    SkyColor = Color.CornflowerBlue;
                    break;
            }
        }

        public void DrawRoad(GraphicsDevice gd, BasicEffect effect, int startPos, int distance)
        {
            //gd.BlendState = BlendState.AlphaBlend;
            //gd.DepthStencilState = DepthStencilState.Default;
            //gd.SamplerStates[0] = SamplerState.LinearClamp;

            Quad quad;

            for (int i = startPos + distance; i >= startPos; i--)
            {
                int pos = i;
                int prevpos = (i - 1);
                if (pos >= TrackSegments.Count) pos = pos - (TrackSegments.Count);
                if (prevpos >= TrackSegments.Count) prevpos = prevpos - TrackSegments.Count;
                if (prevpos < 0) prevpos = 0;

                effect.Texture = textDict[TrackSegments[pos].TextureName];
                effect.DiffuseColor = TrackSegments[pos].Tint;
                quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].Offset, TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].Size.X, TrackSegments[pos].Size.Y);
                Drawing.DrawQuad(effect, quad, gd);
                //quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].Offset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].Size.X, TrackSegments[pos].Size.Y);
                //Drawing.DrawQuad(effect, quad, gd);
            }
            //for (int i = startPos; i < distance; i++)
            //{
            //    int pos = i;
            //    int prevpos = (i - 1);
            //    if (pos >= TrackSegments.Count) pos = pos - (TrackSegments.Count);
            //    if (prevpos >= TrackSegments.Count) prevpos = prevpos - TrackSegments.Count;
            //    if (prevpos < 0) prevpos = 0;

            //    effect.Texture = textDict[TrackSegments[pos].TextureName];
            //    effect.DiffuseColor = TrackSegments[pos].Tint;
            //    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].Offset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].Size.X, TrackSegments[pos].Size.Y);
            //    Drawing.DrawQuad(effect, quad, gd);
            //}
        }

        public void DrawScenery(GraphicsDevice gd, AlphaTestEffect effect, int startPos, int distance)
        {
            //gd.BlendState = BlendState.AlphaBlend;
            
            //gd.DepthStencilState = DepthStencilState.Default;
            //gd.SamplerStates[0] = SamplerState.LinearClamp;

            Quad quad;

            // Draw grid
            //effect.Texture = textDict["road-normal"];
            //effect.DiffuseColor = Color.DarkGray.ToVector3();
            //quad = new Quad(Vector3.Zero, Vector3.Up, Vector3.Forward, 200f, 200f);
            //Drawing.DrawQuad(effect, quad, gd);
            //gd.DepthStencilState = DepthStencilState.DepthRead;

            //gd.DepthStencilState = DepthStencilState.DepthRead;
            for (int i = startPos+distance; i>=startPos; i--)
            {
                int pos = i;
                int prevpos = (i - 1);
                if (pos >= TrackSegments.Count) pos = pos - (TrackSegments.Count);
                if (prevpos >= TrackSegments.Count) prevpos = prevpos - TrackSegments.Count;
                if (prevpos < 0) prevpos = 0;

                Vector3 norm = TrackSegments[pos].Normal;
                Vector3 up = Vector3.Up;

                if (!string.IsNullOrEmpty(TrackSegments[pos].LeftTextureName))
                {
                    norm = TrackSegments[pos].Normal;
                    up = Vector3.Up;

                    effect.Texture = textDict[TrackSegments[pos].LeftTextureName];
                    effect.DiffuseColor = TrackSegments[pos].LeftTint;

                    if (Horizon == Common.Horizon.Island)
                    {
                        if (TrackSegments[pos].LeftTextureName == "tree") TrackSegments[pos].LeftTextureName = "palmtree";
                        if (TrackSegments[pos].RightTextureName == "tree") TrackSegments[pos].RightTextureName = "palmtree";
                    }

                    if (TrackSegments[pos].LeftTextureName == "ground")
                    {
                        effect.DiffuseColor = GroundColor.ToVector3();
                        //norm = Vector3.Transform(TrackSegments[pos].Normal, Matrix.CreateRotationX(MathHelper.PiOver4));
                        //up = TrackSegments[pos].Normal;
                    }
                    if (TrackSegments[pos].LeftTextureName == "building")
                        norm = Vector3.Transform(norm, Matrix.CreateRotationY(MathHelper.PiOver2));

                    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].LeftOffset, norm, up, TrackSegments[pos].LeftSize.X, TrackSegments[pos].LeftSize.Y);
                    Drawing.DrawQuad(effect, quad, gd);
                    //quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].LeftOffset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].LeftSize.X, TrackSegments[pos].LeftSize.Y);
                    //Drawing.DrawQuad(effect, quad, gd);
                }

                if (!string.IsNullOrEmpty(TrackSegments[pos].AboveTextureName))
                {
                    norm = TrackSegments[pos].Normal;
                    up = Vector3.Up;

                    effect.Texture = textDict[TrackSegments[pos].AboveTextureName];
                    effect.DiffuseColor = TrackSegments[pos].AboveTint;
                    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].AboveOffset, norm, up, TrackSegments[pos].AboveSize.X, TrackSegments[pos].AboveSize.Y);
                    Drawing.DrawQuad(effect, quad, gd);
                    //quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].AboveOffset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].AboveSize.X, TrackSegments[pos].AboveSize.Y);
                    //Drawing.DrawQuad(effect, quad, gd);
                }

                if (!string.IsNullOrEmpty(TrackSegments[pos].RightTextureName))
                {
                    norm = TrackSegments[pos].Normal;
                    up = Vector3.Up;

                    effect.Texture = textDict[TrackSegments[pos].RightTextureName];
                    effect.DiffuseColor = TrackSegments[pos].RightTint;

                    if (TrackSegments[pos].RightTextureName == "ground")
                    {
                        effect.DiffuseColor = GroundColor.ToVector3();
                        //norm = Vector3.Transform(TrackSegments[pos].Normal, Matrix.CreateRotationX(MathHelper.PiOver4));
                        //up = TrackSegments[pos].Normal;
                    }

                    if (TrackSegments[pos].RightTextureName == "building")
                        norm = Vector3.Transform(norm, Matrix.CreateRotationY(MathHelper.PiOver2));

                    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].RightOffset, norm, up, TrackSegments[pos].RightSize.X, TrackSegments[pos].RightSize.Y);
                    Drawing.DrawQuad(effect, quad, gd);
                    //quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].RightOffset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].RightSize.X, TrackSegments[pos].RightSize.Y);
                    //Drawing.DrawQuad(effect, quad, gd);
                }
            }
            
           
        }

        public void DrawMap(GraphicsDevice gd, SpriteBatch sb, RenderTarget2D rt)
        {
            mapscale = 7f;
            Vector2 center = new Vector2(rt.Width, rt.Height) / 2;

            mapoffset = Vector2.Zero;

            float top = 1000f, left = 1000f;
            float bottom = -1000f, right = -1000f;
            foreach (Segment seg in TrackSegments)
            {
                if (seg.Position.X < left) left = seg.Position.X;
                if (seg.Position.X > right) right = seg.Position.X;
                if (seg.Position.Z < top) top = seg.Position.Z;
                if (seg.Position.Z > bottom) bottom = seg.Position.Z;
            }

            bool fits = false;
            while (!fits)
            {
                //offset = ((new Vector2(right - left, bottom - top) * scale) / 2);
                mapoffset = new Vector2(150, 150) - ((new Vector2((right + left) / 2, (bottom + top) / 2)) * mapscale);

                if (mapoffset.Y + (top * mapscale) < 7 || mapoffset.X + (left * mapscale) < 7 || mapoffset.Y + (bottom * mapscale) > 293 || mapoffset.X + (right * mapscale) > 293)
                {
                    mapscale -= 0.2f;
                }
                else fits = true;
            }

            

            //gd.SetRenderTarget(rt);

            gd.Clear(Color.Black * 0f);

            sb.Begin(SpriteSortMode.BackToFront, null);
            foreach (Segment seg in TrackSegments)
            {
                sb.Draw(mapSegment, mapoffset + (new Vector2(seg.Position.X, seg.Position.Z) * mapscale), null, Color.White, 0f, new Vector2(7, 7), 1.0f, SpriteEffects.None, 0.9f);
                sb.Draw(mapSegment, mapoffset + (new Vector2(seg.Position.X, seg.Position.Z) * mapscale), null, Color.Black, 0f, new Vector2(7, 7), 0.8f, SpriteEffects.None, 0.1f);
                if (TrackSegments.IndexOf(seg) == 0)
                {
                    float angle = Helper.WrapAngle((float)Math.Atan2(-seg.Normal.X, seg.Normal.Z));
                    sb.Draw(mapSegment, mapoffset + (new Vector2(seg.Position.X, seg.Position.Z) * mapscale), new Rectangle(0, 6, 15, 3), Color.White, angle, new Vector2(8, 1), 1.1f, SpriteEffects.None, 0f);
                }
            }
            sb.End();

            

            //gd.SetRenderTarget(null);
        }

        public void DrawMapCars(SpriteBatch sb, Vector2 mapposition, List<Car> cars)
        {
            sb.Begin(SpriteSortMode.BackToFront, null);
            foreach (Car c in cars.OrderByDescending(car => car.RacePosition))
            {
                if (!c.IsPlayerControlled)
                    sb.Draw(mapSegment, mapposition + mapoffset + (new Vector2(c.Position.X, c.Position.Z) * mapscale), null, c.Tint, 0f, new Vector2(7, 7), 0.4f, SpriteEffects.None, 1);
                else
                    sb.Draw(mapSegment, mapposition + mapoffset + (new Vector2(c.Position.X, c.Position.Z) * mapscale), null, c.Tint, 0f, new Vector2(7, 7), 0.6f, SpriteEffects.None, 0);
            }
            sb.End();
        }

        public void DrawBatches(GraphicsDevice gd, BasicEffect basicEffect, AlphaTestEffect alphaEffect)
        {
            foreach (Batch b in Batches)
            {
                if (b.effectType == BatchEffectType.Basic)
                {
                    basicEffect.Texture = textDict[b.textureName];
                    basicEffect.DiffuseColor = b.diffuseColor;
                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        gd.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, b.vpnts, 0, b.vpnts.Length, b.indexes, 0, b.vpnts.Length/2);
                    }
                }
                else
                {
                    alphaEffect.Texture = textDict[b.textureName];
                    alphaEffect.DiffuseColor = b.diffuseColor;
                    foreach (EffectPass pass in alphaEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        gd.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, b.vpnts, 0, b.vpnts.Length, b.indexes, 0, b.vpnts.Length / 2);
                    }
                }
            }
        }

        void PrepareBatches()
        {
            Quad quad;

            foreach (Segment seg in TrackSegments)
            {
                if (!string.IsNullOrEmpty(seg.TextureName))
                {
                    quad = new Quad(seg.Position + seg.Offset, seg.Normal, Vector3.Up, seg.Size.X, seg.Size.Y);
                    AddToBatch(BatchEffectType.Basic, seg.Tint, seg.TextureName, quad);
                }

                if (!string.IsNullOrEmpty(seg.LeftTextureName))
                {
                    Vector3 norm = seg.Normal;
                    if (seg.LeftTextureName == "building")
                        norm = Vector3.Transform(norm, Matrix.CreateRotationY(MathHelper.PiOver2));

                    if (Horizon == Common.Horizon.Island)
                    {
                        if (seg.LeftTextureName == "tree") seg.LeftTextureName = "palmtree";
                    }

                    quad = new Quad(seg.Position + seg.LeftOffset, norm, Vector3.Up, seg.LeftSize.X, seg.LeftSize.Y);
                    AddToBatch(BatchEffectType.Alpha, seg.LeftTextureName!="ground"?seg.LeftTint:GroundColor.ToVector3(), seg.LeftTextureName, quad);
                }

                if (!string.IsNullOrEmpty(seg.AboveTextureName))
                {
                    quad = new Quad(seg.Position + seg.AboveOffset, seg.Normal, Vector3.Up, seg.AboveSize.X, seg.AboveSize.Y);
                    AddToBatch(BatchEffectType.Alpha, seg.AboveTint, seg.AboveTextureName, quad);
                }

                if (!string.IsNullOrEmpty(seg.RightTextureName))
                {
                    Vector3 norm = seg.Normal;
                    if (seg.RightTextureName == "building")
                        norm = Vector3.Transform(norm, Matrix.CreateRotationY(MathHelper.PiOver2));

                    if (Horizon == Common.Horizon.Island)
                    {
                        if (seg.RightTextureName == "tree") seg.RightTextureName = "palmtree";
                    }

                    quad = new Quad(seg.Position + seg.RightOffset, norm, Vector3.Up, seg.RightSize.X, seg.RightSize.Y);
                    AddToBatch(BatchEffectType.Alpha, seg.RightTextureName != "ground" ? seg.RightTint : GroundColor.ToVector3(), seg.RightTextureName, quad);
                }
            }

            foreach (Batch b in Batches)
            {
                b.vpnts = new VertexPositionNormalTexture[b.quads.Count * 4];
                b.indexes = new short[b.quads.Count * 6];

                int vi = 0;
                int ii = 0;

                for (int q=0;q<b.quads.Count;q++)
                {
                    for (int v = 0; v < b.quads[q].Vertices.Length;v++ )
                    {
                        b.vpnts[vi] = b.quads[q].Vertices[v];
                        vi++;
                    }
                    for (int i = 0; i < b.quads[q].Indexes.Length;i++)
                    {
                        b.indexes[ii] = (short)(b.quads[q].Indexes[i] + (4 * q));
                        ii++;
                    }
                }
            }
        }

        void AddToBatch(BatchEffectType type, Vector3 color, string texName, Quad quad)
        {
            bool found = false;

            foreach (Batch b in Batches)
            {
                if (b.effectType == type && b.diffuseColor == color && b.textureName == texName)
                {
                    b.quads.Add(quad);
                    found = true;
                }
            }

            if (!found)
            {
                Batch newBatch = new Batch();
                newBatch.effectType = type;
                newBatch.diffuseColor = color;
                newBatch.textureName = texName;
                Batches.Add(newBatch);
                newBatch.quads.Add(quad);
            }
        }

        public void Rebuild(List<Vector3> controlPoints)
        {
            TrackSegments.Clear();

            for (int num = 0; num < controlPoints.Count; num++)
            {
                // Get the 4 required points for the catmull rom spline
                Vector3 p1 = controlPoints[num - 1 < 0 ? controlPoints.Count - 1 : num - 1];
                Vector3 p2 = controlPoints[num];
                Vector3 p3 = controlPoints[(num + 1) % controlPoints.Count];
                Vector3 p4 = controlPoints[(num + 2) % controlPoints.Count];

                // Calculate number of iterations we use here based
                // on the distance of the 2 points we generate new points from.
                float distance = Vector3.Distance(p2, p3);
                int numberOfIterations =
                    (int)(SEGMENTS_PER100M * (distance / 100.0f));
                if (numberOfIterations <= 0)
                    numberOfIterations = 1;

                for (int iter = 0; iter < numberOfIterations; iter++)
                {
                    Vector3 newVertex = Vector3.CatmullRom(p1, p2, p3, p4, iter / (float)numberOfIterations);

                    Segment s = new Segment();
                    s.Position = newVertex;
                    TrackSegments.Add(s);
                } // for (iter)


            } // for (num)

            for (int i = TrackSegments.Count - 1; i >= 0; i--)
            {
                int pos = i;
                int prevpos = i - 1;
                if (pos >= TrackSegments.Count) pos = pos - (TrackSegments.Count);
                if (prevpos >= TrackSegments.Count) prevpos = prevpos - TrackSegments.Count;
                if (prevpos < 0) prevpos = TrackSegments.Count + prevpos;

                Vector3 normal = TrackSegments[prevpos].Position - TrackSegments[pos].Position;
                normal.Normalize();
                TrackSegments[i].Normal = normal;

                TrackSegments[i].Paint(i, RoadBrush.Road, AboveBrush.None, SceneryBrush.None, SceneryBrush.None);
            }
            TrackSegments[0].Paint(0, RoadBrush.Road, AboveBrush.StartGrid, SceneryBrush.None, SceneryBrush.None);

            Length = TrackSegments.Count;
        }

        public static Track BuildFromControlPoints(List<Vector3> controlPoints)
        {
            Track t = new Track();

            t.Rebuild(controlPoints);

            return t;
        }

#if !WINRT
        public void AsyncLoad(object sender, DoWorkEventArgs e)
        {
#else
        public async Task AsyncLoad()
        {
#endif
            
            foreach (string s in PackedSegments)
            {
                Segment seg = Segment.FromString(s);
                TrackSegments.Add(seg);
            }

            PrepareBatches();

            HasLoaded = true;
        }

        public static Track Load(string filename, ContentManager content, ParallaxManager pMan, bool async)
        {
            Track returnTrack = null;

            string trackXML = content.Load<string>("tracks/" + filename);

            XmlSerializer xmls = new XmlSerializer(typeof(Track));

            using (StringReader stream = new StringReader(trackXML))
            {
                returnTrack = (Track)xmls.Deserialize(stream);
            }

            returnTrack.LoadContent(content);
            returnTrack.LoadHorizon(content, pMan);

            returnTrack.HasLoaded = false;

            if (async)
            {
#if !WINRT
            returnTrack.bgW.DoWork += returnTrack.AsyncLoad;
            returnTrack.bgW.RunWorkerAsync();
#else
                ThreadPool.RunAsync(async delegate { returnTrack.AsyncLoad(); });
#endif
            }
            else
            {
                foreach (string s in returnTrack.PackedSegments)
                {
                    Segment seg = Segment.FromString(s);
                    returnTrack.TrackSegments.Add(seg);
                }

                returnTrack.PrepareBatches();

                returnTrack.HasLoaded = true;
            }

            return returnTrack;
        }

       
    }
}
