using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psuedo3DRacer.Common
{
    public class Car
    {
        public Vector3 Position;
        public Vector3 Normal;
        public float Yaw;
        public float Pitch;
        public float Speed = 0.05f;

        public Vector3 CameraPosition;
        public Vector3 CameraLookat;

        int currentTrackPos = 0;
        int prevTrackPos = 0;

        public string debug;

        Vector3 trackOffset = new Vector3(0, 0.15f, 0);

        Vector3 target;
        public int courseTrackPos = 0;

        Texture2D[] texDirections;

        public Car(int trackPos, Track track)
        {
            courseTrackPos = trackPos;
            currentTrackPos = trackPos;

            PlotCourse(track);
            Position = track.TrackSegments[Helper.WrapInt(courseTrackPos - 10, track.TrackSegments.Count - 1)].Position + trackOffset;
        }

        public void LoadContent(ContentManager content, int carnum)
        {
            texDirections = new Texture2D[4];
            for (int dir = 0; dir < 4; dir++)
                texDirections[dir] = content.Load<Texture2D>("cars/" + carnum + "-" + dir);
        }

        public void Update(GameTime gameTime, Track track)
        {
            float dist = 99999f;
            for (int i = 0; i < track.TrackSegments.Count; i++)
                if ((Position - track.TrackSegments[i].Position).Length() < dist)
                {
                    dist = (Position - track.TrackSegments[i].Position).Length();
                    currentTrackPos = i;
                }

            if ((Position - target).Length() < 0.5f)
            {
                courseTrackPos += 1;
                if (courseTrackPos > track.TrackSegments.Count - 1) courseTrackPos -= (track.TrackSegments.Count - 1);
                PlotCourse(track);
            }

            Vector3 targetnorm = Position - target;
            Normal = targetnorm;
            Normal.Normalize();
            float targetDist = targetnorm.Length();
            Yaw = (float)Math.Atan2(targetnorm.X, targetnorm.Z);
            Pitch = (float)Math.Atan2(-targetnorm.Y, targetDist);
            Matrix rot = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);

            Vector3 rotatedVector = Vector3.Transform(Vector3.Forward, rot);
            Position += Speed * rotatedVector;

            rotatedVector = Vector3.Transform(new Vector3(0, 0.3f, 1f), rot);
            CameraPosition = Vector3.Lerp(CameraPosition, Position + rotatedVector, 0.1f);
            rotatedVector = Vector3.Transform(new Vector3(0, 0.25f, -1f), rot);
            CameraLookat = Vector3.Lerp(CameraLookat, Position + rotatedVector, 0.1f);
        }

        public void Draw(GraphicsDevice gd, AlphaTestEffect effect, Camera gameCamera)
        {
            gd.BlendState = BlendState.AlphaBlend;
            //gd.DepthStencilState = DepthStencilState.Default;
            gd.SamplerStates[0] = SamplerState.LinearClamp;

            //Matrix cameraRotation = Matrix.CreateRotationX(gameCamera.Pitch) * Matrix.CreateRotationY(gameCamera.Yaw);
            Vector3 faceCam = gameCamera.Position - Position; //Vector3.Transform(Vector3.Forward, -cameraRotation);
            faceCam.Normalize();

            Quad quad;
            effect.Texture = GetTextureForDirection(faceCam);
            effect.DiffuseColor = Color.White.ToVector3();
            quad = new Quad(Position, faceCam, Vector3.Up, 0.3f, 0.2f);
            Drawing.DrawQuad(effect, quad, gd);
        }

        void PlotCourse(Track track)
        {
            target = track.TrackSegments[Helper.WrapInt(courseTrackPos, track.TrackSegments.Count - 1)].Position + trackOffset;
        }

        Texture2D GetTextureForDirection(Vector3 direction)
        {
            float normAngle = Helper.WrapAngle((float)Math.Atan2(Normal.X, Normal.Z));
            float camAngle = Helper.WrapAngle((float)Math.Atan2(direction.X, direction.Z));

            float testAngle = Helper.WrapAngle(camAngle - normAngle);

            int returnTex = 0;
            //if ((testAngle <= -(MathHelper.PiOver4 * 3) && testAngle> -(MathHelper.PiOver4 * 4)) || (testAngle >= 0f && testAngle < (MathHelper.PiOver4 * 1))) returnTex = 0;
            //if (testAngle >= (MathHelper.PiOver4 * 1) && testAngle < (MathHelper.PiOver4 * 3)) returnTex = 1;
            //if ((testAngle >= (MathHelper.PiOver4 * 3) && testAngle < (MathHelper.PiOver4 * 4)) || (testAngle> -(MathHelper.PiOver4 * 4) && testAngle < -(MathHelper.PiOver4 * 3))) returnTex = 2;
            //if (testAngle <= -(MathHelper.PiOver4 * 1) && testAngle > -(MathHelper.PiOver4 * 3)) returnTex = 3;

            if ((testAngle <= -(MathHelper.PiOver4 * 3.25) && testAngle > -(MathHelper.PiOver4 * 4)) || (testAngle >= 0f && testAngle < (MathHelper.PiOver4 * 0.75))) returnTex = 0;
            if (testAngle >= (MathHelper.PiOver4 * 0.75) && testAngle < (MathHelper.PiOver4 * 3.25)) returnTex = 1;
            if ((testAngle >= (MathHelper.PiOver4 * 3.25) && testAngle < (MathHelper.PiOver4 * 4)) || (testAngle > -(MathHelper.PiOver4 * 4) && testAngle < -(MathHelper.PiOver4 * 3.25))) returnTex = 2;
            if (testAngle <= -(MathHelper.PiOver4 * 0.75) && testAngle > -(MathHelper.PiOver4 * 3.25)) returnTex = 3;

            debug = normAngle + " | " + camAngle + " | " + testAngle;

            return texDirections[returnTex];
        }
    }
}
