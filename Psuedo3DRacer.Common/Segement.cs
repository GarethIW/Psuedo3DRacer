using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psuedo3DRacer.Common
{
    public enum SurfaceType
    {
        Road,
        Dirt
    }

    public enum SceneryType
    {
        Wall,
        Offroad
    }

    public enum RoadBrush
    {
        Road,
        Wood,
        Tunnel,
        StartGrid,
    }

    public enum SceneryBrush
    {
        None,
        Trees,
        Girders,
        Wall
    }

    public class Segment
    {
        static Random randomNumber = new Random();

        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Offset;
        public Vector2 Size;
        public string TextureName;
        public Vector3 Tint;
        public SurfaceType TrackSurface;

        public Vector3 LeftOffset;
        public Vector2 LeftSize;
        public string LeftTextureName;
        public Vector3 LeftTint;
        public SceneryType LeftScenery;

        public Vector3 RightOffset;
        public Vector2 RightSize;
        public string RightTextureName;
        public Vector3 RightTint;
        public SceneryType RightScenery;

        public void Paint(int trackPos, RoadBrush road, SceneryBrush left, SceneryBrush right)
        {
            switch (road)
            {
                case RoadBrush.Road:
                    Offset = Vector3.Zero;
                    Size = new Vector2(1f, 0.25f);
                    TextureName = "road-normal";
                    TrackSurface = SurfaceType.Road;
                    if (trackPos % 2 == 0) Tint = Color.DarkGray.ToVector3();
                    else Tint = Color.White.ToVector3();
                    break;
                case RoadBrush.Wood:
                    Offset = Vector3.Zero;
                    Size = new Vector2(1f, 0.25f);
                    TextureName = "road-wood";
                    TrackSurface = SurfaceType.Road;
                    if (trackPos % 2 == 0) Tint = Color.DarkGray.ToVector3();
                    else Tint = Color.White.ToVector3();
                    break;
                case RoadBrush.Tunnel:
                    Offset = new Vector3(0f,0.5f,0f);
                    Size = new Vector2(3f, 1f);
                    TextureName = "road-tunnel";
                    TrackSurface = SurfaceType.Road;
                    if (trackPos % 4 == 0) Tint = Color.DarkGray.ToVector3();
                    else Tint = Color.White.ToVector3();
                    break;
            }

            Vector3 leftV = Vector3.Cross(Normal, Vector3.Up);
            Vector3 rightV = -Vector3.Cross(Normal, Vector3.Up);

            switch (left)
            {
                case SceneryBrush.Trees:
                    if (trackPos % 10 == 0)
                    {
                        LeftOffset = (leftV * 1f) + (leftV * new Vector3(((float)randomNumber.NextDouble()*3f), 0f, 0f)) + new Vector3(0f, 0.5f, 0f); 
                        LeftSize = new Vector2(0.5f, 1f);
                        LeftTextureName = "tree";
                        LeftScenery = SceneryType.Offroad;
                        LeftTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.Girders:
                    if (trackPos % 20 == 0)
                    {
                        LeftOffset = (leftV * 0.65f)  + new Vector3(0f, -2f + (0.5f*Position.Y), 0f);
                        LeftSize = new Vector2(0.25f, 4f);
                        LeftTextureName = "girder";
                        LeftScenery = SceneryType.Offroad;
                        LeftTint = Color.White.ToVector3();
                    }
                    break;
            }
            switch (right)
            {
                case SceneryBrush.Trees:
                    if (trackPos % 10 == 0)
                    {
                        RightOffset = (rightV * 1f) + (rightV * new Vector3(((float)randomNumber.NextDouble() * 3f), 0f, 0f)) + new Vector3(0f, 0.5f, 0f); 
                        RightSize = new Vector2(0.5f, 1f);
                        RightTextureName = "tree";
                        RightScenery = SceneryType.Offroad;
                        RightTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.Girders:
                    if (trackPos % 20 == 0)
                    {
                        RightOffset = (rightV * 0.65f) + new Vector3(0f, -2f + (0.5f * Position.Y), 0f);
                        RightSize = new Vector2(0.25f, 4f);
                        RightTextureName = "girder";
                        RightScenery = SceneryType.Offroad;
                        RightTint = Color.White.ToVector3();
                    }
                    break;
            }
        }
    }
}
