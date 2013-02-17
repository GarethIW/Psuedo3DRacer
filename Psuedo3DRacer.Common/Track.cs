using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psuedo3DRacer.Common
{
    public class Track
    {
        const int SEGMENTS_PER100M = 1000;

        public int Length = 0;

        public List<Segment> TrackSegments = new List<Segment>();

        Dictionary<string, Texture2D> textDict = new Dictionary<string, Texture2D>();

        public void LoadContent(ContentManager content)
        {
            textDict.Add("road-normal", content.Load<Texture2D>("road-normal"));
            textDict.Add("road-tunnel", content.Load<Texture2D>("tunnel"));
            textDict.Add("road-wood", content.Load<Texture2D>("road-wood"));
            textDict.Add("tree", content.Load<Texture2D>("tree"));
            textDict.Add("girder", content.Load<Texture2D>("girder"));

        }

        public void Draw(GraphicsDevice gd, BasicEffect effect, int startPos, int distance)
        {
            gd.BlendState = BlendState.AlphaBlend;
            gd.DepthStencilState = DepthStencilState.Default;
            gd.SamplerStates[0] = SamplerState.LinearClamp;

            for (int i = startPos+distance; i>=startPos; i--)
            {
                int pos = i;
                int prevpos = (i - 1);
                if (pos >= TrackSegments.Count) pos = pos - (TrackSegments.Count);
                if (prevpos >= TrackSegments.Count) prevpos = prevpos - TrackSegments.Count;
                if (prevpos < 0) prevpos = 0;

                Quad quad = new Quad(TrackSegments[pos].Position+TrackSegments[pos].Offset, TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].Size.X, TrackSegments[pos].Size.Y);
                effect.Texture = textDict[TrackSegments[pos].TextureName];
                effect.DiffuseColor = TrackSegments[pos].Tint;
                Drawing.DrawQuad(effect, quad, gd);

                quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].Offset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].Size.X, TrackSegments[pos].Size.Y);
                Drawing.DrawQuad(effect, quad, gd);

                if (!string.IsNullOrEmpty(TrackSegments[pos].LeftTextureName))
                {
                    effect.Texture = textDict[TrackSegments[pos].LeftTextureName];
                    effect.DiffuseColor = TrackSegments[pos].LeftTint;

                    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].LeftOffset, TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].LeftSize.X, TrackSegments[pos].LeftSize.Y);
                    Drawing.DrawQuad(effect, quad, gd);
                    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].LeftOffset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].LeftSize.X, TrackSegments[pos].LeftSize.Y);
                    Drawing.DrawQuad(effect, quad, gd);
                }

                if (!string.IsNullOrEmpty(TrackSegments[pos].RightTextureName))
                {
                    effect.Texture = textDict[TrackSegments[pos].RightTextureName];
                    effect.DiffuseColor = TrackSegments[pos].RightTint;

                    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].RightOffset, TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].RightSize.X, TrackSegments[pos].RightSize.Y);
                    Drawing.DrawQuad(effect, quad, gd);
                    quad = new Quad(TrackSegments[pos].Position + TrackSegments[pos].RightOffset, -TrackSegments[pos].Normal, Vector3.Up, TrackSegments[pos].RightSize.X, TrackSegments[pos].RightSize.Y);
                    Drawing.DrawQuad(effect, quad, gd);
                }
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

                TrackSegments[i].Paint(i, RoadBrush.Road, SceneryBrush.None, SceneryBrush.None);
            }

            Length = TrackSegments.Count;
        }

        public static Track BuildFromControlPoints(List<Vector3> controlPoints)
        {
            Track t = new Track();

            t.Rebuild(controlPoints);

            return t;
        }
    }
}
