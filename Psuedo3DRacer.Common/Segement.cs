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
        Wood
    }

    public enum AboveBrush
    {
        None,
        Erase,
        Tunnel,
        TunnelUpper,
        StartGrid,
    }

    public enum SceneryBrush
    {
        None,
        Erase,
        Trees,
        Girders,
        UpperWall,
        LowerWall,
        SignLeft,
        SignRight
    }

    [Serializable]
    public class Segment
    {
        static Random randomNumber = new Random();

        public Vector3 Position = Vector3.Zero;
        public Vector3 Normal = Vector3.Zero;
        public Vector3 Offset = Vector3.Zero;
        public Vector2 Size = Vector2.Zero;
        public string TextureName;
        public Vector3 Tint = Vector3.Zero;
        public SurfaceType TrackSurface;

        public Vector3 AboveOffset = Vector3.Zero;
        public Vector2 AboveSize = Vector2.Zero;
        public string AboveTextureName;
        public Vector3 AboveTint = Vector3.Zero;

        public Vector3 LeftOffset = Vector3.Zero;
        public Vector2 LeftSize = Vector2.Zero;
        public string LeftTextureName;
        public Vector3 LeftTint = Vector3.Zero;
        public SceneryType LeftScenery;

        public Vector3 RightOffset = Vector3.Zero;
        public Vector2 RightSize = Vector2.Zero;
        public string RightTextureName;
        public Vector3 RightTint = Vector3.Zero;
        public SceneryType RightScenery;

        public void Paint(int trackPos, RoadBrush road, AboveBrush above, SceneryBrush left, SceneryBrush right)
        {
            switch (road)
            {
                case RoadBrush.Road:
                    Offset = new Vector3(0f, 0f, 0f);
                    Size = new Vector2(1f, 0.05f);
                    TextureName = "road-normal";
                    TrackSurface = SurfaceType.Road;
                    if (trackPos % 2 == 0) Tint = Color.DarkGray.ToVector3();
                    else Tint = Color.White.ToVector3();
                    break;
                case RoadBrush.Wood:
                    Offset = new Vector3(0f, 0f, 0f);
                    Size = new Vector2(1f, 0.05f);
                    TextureName = "road-wood";
                    TrackSurface = SurfaceType.Road;
                    if (trackPos % 2 == 0) Tint = Color.DarkGray.ToVector3();
                    else Tint = Color.White.ToVector3();
                    break;
               
            }

            Vector3 leftV = Vector3.Cross(Normal, Vector3.Up);
            Vector3 rightV = -Vector3.Cross(Normal, Vector3.Up);

            switch (above)
            {
                case AboveBrush.Tunnel:
                    AboveOffset = new Vector3(0f,0.45f,0f);
                    AboveSize = new Vector2(3f, 1f);
                    AboveTextureName = "tunnel";
                    if (trackPos % 4 == 0) AboveTint = Color.DarkGray.ToVector3();
                    else AboveTint = Color.White.ToVector3();
                    break;
                case AboveBrush.TunnelUpper:
                    AboveOffset = new Vector3(0f, 1.25f, 0f);
                    AboveSize = new Vector2(1f, 0.5f);
                    AboveTextureName = "tunnel-upper";
                    if (trackPos % 4 == 0) AboveTint = Color.DarkGray.ToVector3();
                    else AboveTint = Color.White.ToVector3();
                    break;
                case AboveBrush.StartGrid:
                    if (trackPos % 10 == 0)
                    {
                        AboveOffset = new Vector3(0f, 0.45f, 0f);
                        AboveSize = new Vector2(1.5f, 1f);
                        AboveTextureName = "start";
                        AboveTint = Color.White.ToVector3();
                    }
                    break;
                case AboveBrush.Erase:
                    AboveTextureName = "";
                    break;

            }

            switch (left)
            {
                case SceneryBrush.Trees:
                    if (trackPos % 10 == 0)
                    {
                        LeftOffset = (leftV * 1f) + (leftV * new Vector3(((float)randomNumber.NextDouble()*3f), 0f, 0f)) + new Vector3(0f, (-Position.Y) +0.4f, 0f); 
                        LeftSize = new Vector2(0.5f, 1f);
                        LeftTextureName = "tree";
                        LeftScenery = SceneryType.Offroad;
                        LeftTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.SignLeft:
                case SceneryBrush.SignRight:
                    if (trackPos % 20 == 0)
                    {
                        LeftOffset = (leftV * 1f) + new Vector3(0f, (-Position.Y) + 0.4f, 0f);
                        LeftSize = new Vector2(0.5f, 1f);
                        LeftTextureName = left==SceneryBrush.SignLeft?"sign-left":"sign-right";
                        LeftScenery = SceneryType.Offroad;
                        LeftTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.Girders:
                    if (trackPos % 20 == 0)
                    {
                        LeftSize = new Vector2(0.25f, Position.Y + 1f);
                        LeftOffset = (leftV * 0.625f) + new Vector3(0f, (-Position.Y) + ((LeftSize.Y / 2)-0.1f), 0f);
                        LeftTextureName = "girder";
                        LeftScenery = SceneryType.Offroad;
                        LeftTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.UpperWall:
                    LeftOffset = (leftV * 1f) + new Vector3(0f, 0f, 0f);
                    LeftSize = new Vector2(1f, 3f);
                    LeftTextureName = "wall";
                    LeftScenery = SceneryType.Wall;
                    if (trackPos % 4 == 0) LeftTint = Color.DarkGray.ToVector3();
                    else LeftTint = Color.White.ToVector3();
                    break;
                case SceneryBrush.LowerWall:
                    LeftOffset = new Vector3(0f, -0.8f, 0f);
                    LeftSize = new Vector2(1.1f, 1.5f);
                    LeftTextureName = "wall";
                    LeftScenery = SceneryType.Wall;
                    if (trackPos % 4 == 0) LeftTint = Color.DarkGray.ToVector3();
                    else LeftTint = Color.White.ToVector3();
                    break;
                case SceneryBrush.Erase:
                    LeftTextureName = "";
                    break;
            }
            switch (right)
            {
                case SceneryBrush.Trees:
                    if (trackPos % 10 == 0)
                    {
                        RightOffset = (rightV * 1f) + (rightV * new Vector3(((float)randomNumber.NextDouble() * 3f), 0f, 0f)) + new Vector3(0f, (-Position.Y) +0.4f, 0f); 
                        RightSize = new Vector2(0.5f, 1f);
                        RightTextureName = "tree";
                        RightScenery = SceneryType.Offroad;
                        RightTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.SignLeft:
                case SceneryBrush.SignRight:
                    if (trackPos % 20 == 0)
                    {
                        RightOffset = (rightV * 1f) + new Vector3(0f, (-Position.Y) + 0.4f, 0f);
                        RightSize = new Vector2(0.5f, 1f);
                        RightTextureName = right == SceneryBrush.SignLeft ? "sign-left" : "sign-right";
                        RightScenery = SceneryType.Offroad;
                        RightTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.Girders:
                    if (trackPos % 20 == 0)
                    {
                        RightSize = new Vector2(0.25f, Position.Y+1f);
                        RightOffset = (rightV * 0.625f) + new Vector3(0f, (-Position.Y) + ((RightSize.Y / 2)-0.1f), 0f);
                        RightTextureName = "girder";
                        RightScenery = SceneryType.Offroad;
                        RightTint = Color.White.ToVector3();
                    }
                    break;
                case SceneryBrush.UpperWall:
                    RightOffset = (rightV * 1f) + new Vector3(0f, 0f, 0f);
                    RightSize = new Vector2(1f, 3f);
                    RightTextureName = "wall";
                    RightScenery = SceneryType.Wall;
                    if (trackPos % 4 == 0) RightTint = Color.DarkGray.ToVector3();
                    else RightTint = Color.White.ToVector3();
                    break;
                case SceneryBrush.LowerWall:
                    RightOffset = new Vector3(0f, -0.8f, 0f);
                    RightSize = new Vector2(1.1f, 1.5f);
                    RightTextureName = "wall";
                    RightScenery = SceneryType.Wall;
                    if (trackPos % 4 == 0) RightTint = Color.DarkGray.ToVector3();
                    else RightTint = Color.White.ToVector3();
                    break;
                case SceneryBrush.Erase:
                    RightTextureName = "";
                    break;
            }

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Position.X + ",");
            sb.Append(Position.Y + ",");
            sb.Append(Position.Z + ",");

            sb.Append(Normal.X + ",");
            sb.Append(Normal.Y + ",");
            sb.Append(Normal.Z + ",");

            sb.Append(Offset.X + ",");
            sb.Append(Offset.Y + ",");
            sb.Append(Offset.Z + ",");

            sb.Append(Size.X + ",");
            sb.Append(Size.Y + ",");

            sb.Append(TextureName + ",");

            sb.Append(Tint.X + ",");
            sb.Append(Tint.Y + ",");
            sb.Append(Tint.Z + ",");

            sb.Append((int)TrackSurface + ",");

            sb.Append(AboveOffset.X + ",");
            sb.Append(AboveOffset.Y + ",");
            sb.Append(AboveOffset.Z + ",");

            sb.Append(AboveSize.X + ",");
            sb.Append(AboveSize.Y + ",");

            sb.Append(AboveTextureName + ",");

            sb.Append(AboveTint.X + ",");
            sb.Append(AboveTint.Y + ",");
            sb.Append(AboveTint.Z + ",");

            sb.Append(LeftOffset.X + ",");
            sb.Append(LeftOffset.Y + ",");
            sb.Append(LeftOffset.Z + ",");

            sb.Append(LeftSize.X + ",");
            sb.Append(LeftSize.Y + ",");

            sb.Append(LeftTextureName + ",");

            sb.Append(LeftTint.X + ",");
            sb.Append(LeftTint.Y + ",");
            sb.Append(LeftTint.Z + ",");

            sb.Append((int)LeftScenery + ",");

            sb.Append(RightOffset.X + ",");
            sb.Append(RightOffset.Y + ",");
            sb.Append(RightOffset.Z + ",");

            sb.Append(RightSize.X + ",");
            sb.Append(RightSize.Y + ",");

            sb.Append(RightTextureName + ",");

            sb.Append(RightTint.X + ",");
            sb.Append(RightTint.Y + ",");
            sb.Append(RightTint.Z + ",");

            sb.Append((int)RightScenery);

            return sb.ToString();
        }

        public static Segment FromString(string segmentString)
        {
            Segment returnSegment = new Segment();

            string[] parts = segmentString.Split(',');

            returnSegment.Position.X = float.Parse(parts[0]);
            returnSegment.Position.Y = float.Parse(parts[1]);
            returnSegment.Position.Z = float.Parse(parts[2]);

            returnSegment.Normal.X = float.Parse(parts[3]);
            returnSegment.Normal.Y = float.Parse(parts[4]);
            returnSegment.Normal.Z = float.Parse(parts[5]);

            returnSegment.Offset.X = float.Parse(parts[6]);
            returnSegment.Offset.Y = float.Parse(parts[7]);
            returnSegment.Offset.Z = float.Parse(parts[8]);

            returnSegment.Size.X = float.Parse(parts[9]);
            returnSegment.Size.Y = float.Parse(parts[10]);

            returnSegment.TextureName = parts[11];

            returnSegment.Tint.X = float.Parse(parts[12]);
            returnSegment.Tint.Y = float.Parse(parts[13]);
            returnSegment.Tint.Z = float.Parse(parts[14]);

            returnSegment.TrackSurface = (SurfaceType)int.Parse(parts[15]); ;

            returnSegment.AboveOffset.X = float.Parse(parts[16]);
            returnSegment.AboveOffset.Y = float.Parse(parts[17]);
            returnSegment.AboveOffset.Z = float.Parse(parts[18]);

            returnSegment.AboveSize.X = float.Parse(parts[19]);
            returnSegment.AboveSize.Y = float.Parse(parts[20]);

            returnSegment.AboveTextureName = parts[21];

            returnSegment.AboveTint.X = float.Parse(parts[22]);
            returnSegment.AboveTint.Y = float.Parse(parts[23]);
            returnSegment.AboveTint.Z = float.Parse(parts[24]);

            returnSegment.LeftOffset.X = float.Parse(parts[25]);
            returnSegment.LeftOffset.Y = float.Parse(parts[26]);
            returnSegment.LeftOffset.Z = float.Parse(parts[27]);

            returnSegment.LeftSize.X = float.Parse(parts[28]);
            returnSegment.LeftSize.Y = float.Parse(parts[29]);

            returnSegment.LeftTextureName = parts[30];

            returnSegment.LeftTint.X = float.Parse(parts[31]);
            returnSegment.LeftTint.Y = float.Parse(parts[32]);
            returnSegment.LeftTint.Z = float.Parse(parts[33]);

            returnSegment.LeftScenery = (SceneryType)int.Parse(parts[34]);

            returnSegment.RightOffset.X = float.Parse(parts[35]);
            returnSegment.RightOffset.Y = float.Parse(parts[36]);
            returnSegment.RightOffset.Z = float.Parse(parts[37]);

            returnSegment.RightSize.X = float.Parse(parts[38]);
            returnSegment.RightSize.Y = float.Parse(parts[39]);

            returnSegment.RightTextureName = parts[40];

            returnSegment.RightTint.X = float.Parse(parts[41]);
            returnSegment.RightTint.Y = float.Parse(parts[42]);
            returnSegment.RightTint.Z = float.Parse(parts[43]);

            returnSegment.RightScenery = (SceneryType)int.Parse(parts[44]);

            return returnSegment;
        }
    }
}
