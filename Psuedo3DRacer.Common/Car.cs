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
        static Random randomNumber = new Random();

        public bool IsPlayerControlled = false;

        public Vector3 Position;
        public Vector3 Normal;
        public float Yaw;
        public float Pitch;
        public float Speed = 0.0f;

        public Vector3 CameraPosition;
        public Vector3 CameraLookat;

        public int ConcentrationLevel = 100;
        public double CorrectionTime = 1000;
        public float SpeedWhenTurning = 0.05f;
        public double ReactionTime = 500;

        double correctionCountdown = 0;

        bool hasStarted = false;

        bool applyingThrottle = false;
        bool applyingBrake = false;
        int applyingSteering = 0;

        public float steeringAmount;

        int currentTrackPos = 0;
        int prevTrackPos = 0;

        float currentPositionOnTrack = 0f;
        float targetPositionOnTrack = 0f;

        public string debug;

        Vector3 trackOffset = new Vector3(0, 0.13f, 0);

        Vector3 target;
        public int courseTrackPos = 0;

        Texture2D[] texDirections;

        public Color Tint;

        public Car(int trackPos, float offset, Track track, Color tint)
        {
            Tint = tint;

            

            ConcentrationLevel = 50 + randomNumber.Next(1900);
            CorrectionTime = 500 + (randomNumber.NextDouble() * 4500);
            SpeedWhenTurning = 0.045f + ((float)randomNumber.NextDouble() * 0.014f);
            ReactionTime = 100 + (randomNumber.NextDouble() * 1900);
            

            SetPosition(trackPos, track, offset);

            correctionCountdown = ReactionTime;
        }

        public void LoadContent(ContentManager content, int carnum)
        {
            texDirections = new Texture2D[8];
            for (int dir = 0; dir < 8; dir++)
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

            if (!IsPlayerControlled)
            {
                if (!hasStarted)
                {
                    correctionCountdown -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (correctionCountdown <= 0)
                    {
                        hasStarted = true;
                        applyingThrottle = true;

                        correctionCountdown = ReactionTime;
                    }
                }


                if ((Position - target).Length() < 0.5f)
                {
                    courseTrackPos += 1;
                    if (courseTrackPos > track.TrackSegments.Count - 1) courseTrackPos -= (track.TrackSegments.Count - 1);
                    PlotCourse(track);
                }

               currentPositionOnTrack = MathHelper.Lerp(currentPositionOnTrack, targetPositionOnTrack, 0.01f);

                if (hasStarted)
                {
                    if (Math.Abs(currentPositionOnTrack - targetPositionOnTrack) > 0.02f)
                    {
                        if (Speed > SpeedWhenTurning) applyingThrottle = false;
                        else
                            applyingThrottle = true;
                    }
                    else applyingThrottle = true;
                }

                if (correctionCountdown > 0) correctionCountdown -= gameTime.ElapsedGameTime.TotalMilliseconds;

                Vector3 targetnorm = Position - target;
                Normal = targetnorm;
                Normal.Normalize();
                float targetDist = targetnorm.Length();
                Yaw = (float)Math.Atan2(targetnorm.X, targetnorm.Z);
                Pitch = (float)Math.Atan2(-targetnorm.Y, targetDist);
            }
            else
            {
                if (Speed > 0)
                {
                    if (applyingSteering != 0)
                    {
                        steeringAmount += (-(float)applyingSteering * 0.03f);
                    }
                    else
                    {
                        steeringAmount = MathHelper.Lerp(steeringAmount, 0f, 0.2f);
                    }

                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.4f, 0.4f);
                    Yaw += steeringAmount * Speed;
                }

                //Vector3 trackNormal = track.TrackSegments[currentTrackPos].Normal;
                //float trackYaw = MathHelper.WrapAngle((float)Math.Atan2(trackNormal.X, trackNormal.Z));
                //Yaw = MathHelper.Clamp(Yaw, trackYaw - (MathHelper.PiOver4/2), trackYaw + (MathHelper.PiOver4/2));

                target = track.TrackSegments[Helper.WrapInt(currentTrackPos + 5, track.TrackSegments.Count - 1)].Position + trackOffset;
                Vector3 targetnorm = Position - target;
                Pitch = (float)Math.Atan2(-targetnorm.Y, targetnorm.Length());

                Matrix normRot = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
                Normal =  Position - Vector3.Transform(Vector3.Forward * 100f, normRot);
                Normal.Normalize();

                
            }

            if (applyingThrottle) Speed += 0.0003f;
            else Speed -= 0.0003f;
            if (applyingBrake) Speed -= 0.001f;
            Speed = MathHelper.Clamp(Speed, 0f, 0.06f);          

           
            Matrix rot = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
            Vector3 rotatedVector = Vector3.Transform(Vector3.Forward, rot);
            Position += Speed * rotatedVector;

            rotatedVector = Vector3.Transform(new Vector3(0, 0.3f, 1f), rot);
            CameraPosition = Vector3.Lerp(CameraPosition, Position + rotatedVector, 0.1f);
            rotatedVector = Vector3.Transform(new Vector3(0, 0.25f, -1f), rot);
            //rotatedVector = Vector3.Transform(rotatedVector, Matrix.CreateRotationZ(0.4f));
            CameraLookat = Vector3.Lerp(CameraLookat, Position + rotatedVector, 0.1f);
            //CameraLookat = Vector3.Transform(CameraLookat, Matrix.CreateRotationZ(0.4f));
        }

        public void Draw(GraphicsDevice gd, AlphaTestEffect effect, Camera gameCamera)
        {
            int oldRefAlpha = effect.ReferenceAlpha;

            gd.BlendState = BlendState.AlphaBlend;
            //gd.DepthStencilState = DepthStencilState.Default;
            gd.SamplerStates[0] = SamplerState.LinearClamp;

            effect.ReferenceAlpha = 150;

            //Matrix cameraRotation = Matrix.CreateRotationX(gameCamera.Pitch) * Matrix.CreateRotationY(gameCamera.Yaw);
            Vector3 faceCam = gameCamera.Position - Position; //Vector3.Transform(Vector3.Forward, -cameraRotation);
            faceCam.Normalize();
         
            Quad quad;
            effect.Texture = GetTextureForDirection(faceCam);
            effect.DiffuseColor = Tint.ToVector3();
            quad = new Quad(Position, faceCam, Vector3.Up, 0.4f, 0.2f);
            Drawing.DrawQuad(effect, quad, gd);

            effect.ReferenceAlpha = oldRefAlpha;
        }

        void PlotCourse(Track track)
        {
            target = track.TrackSegments[Helper.WrapInt(courseTrackPos, track.TrackSegments.Count - 1)].Position;

            Vector3 drivingLineTarget = track.TrackSegments[Helper.WrapInt(courseTrackPos + 50, track.TrackSegments.Count - 1)].Normal;
            float targetAngle = Helper.WrapAngle((float)Math.Atan2(drivingLineTarget.X, drivingLineTarget.Z) - Yaw);

            //debug = targetAngle + " | " + targetPositionOnTrack.ToString();

           

            if (correctionCountdown <= 0)
            {
                targetPositionOnTrack = MathHelper.Clamp((0.35f / 0.5f) * targetAngle, -0.35f, 0.35f);

                if (randomNumber.Next(ConcentrationLevel) == 1)
                {
                    targetPositionOnTrack = ((float)randomNumber.NextDouble() * 0.8f) - 0.4f;

                    correctionCountdown = CorrectionTime;
                }
            }

            debug = correctionCountdown + " | " + CorrectionTime + " | " + ConcentrationLevel;

            Vector3 leftV = Vector3.Cross(track.TrackSegments[Helper.WrapInt(courseTrackPos, track.TrackSegments.Count - 1)].Normal, Vector3.Up);
            target += leftV * currentPositionOnTrack;

            target += trackOffset;
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

            if ((testAngle <= -(MathHelper.PiOver4 * 3.5) && testAngle > -(MathHelper.PiOver4 * 4)) || (testAngle >= 0f && testAngle < (MathHelper.PiOver4 * 0.5))) returnTex = 0;
            if (testAngle >= (MathHelper.PiOver4 * 0.5) && testAngle < (MathHelper.PiOver4 * 1.5)) returnTex = 1;
            if (testAngle >= (MathHelper.PiOver4 * 1.5) && testAngle < (MathHelper.PiOver4 * 2.5)) returnTex = 2;
            if (testAngle >= (MathHelper.PiOver4 * 2.5) && testAngle < (MathHelper.PiOver4 * 3.5)) returnTex = 3;
            if ((testAngle >= (MathHelper.PiOver4 * 3.5) && testAngle < (MathHelper.PiOver4 * 4)) || (testAngle > -(MathHelper.PiOver4 * 4) && testAngle < -(MathHelper.PiOver4 * 3.5))) returnTex = 4;
            if (testAngle <= -(MathHelper.PiOver4 * 2.5) && testAngle > -(MathHelper.PiOver4 * 3.5)) returnTex = 5;
            if (testAngle <= -(MathHelper.PiOver4 * 1.5) && testAngle > -(MathHelper.PiOver4 * 2.5)) returnTex = 6;
            if (testAngle <= -(MathHelper.PiOver4 * 0.5) && testAngle > -(MathHelper.PiOver4 * 1.5)) returnTex = 7;

            

            return texDirections[returnTex];
        }

        public void SetPosition(int trackPos, Track track, float offset)
        {
            courseTrackPos = trackPos + 20 + randomNumber.Next(30);
            currentTrackPos = trackPos;

            Vector3 leftV = Vector3.Cross(track.TrackSegments[Helper.WrapInt(trackPos, track.TrackSegments.Count - 1)].Normal, Vector3.Up);
            Vector3 offsetVect = leftV * offset;

            PlotCourse(track);
            Position = track.TrackSegments[Helper.WrapInt(currentTrackPos - 10, track.TrackSegments.Count - 1)].Position + trackOffset + offsetVect;

            target += offsetVect;

            currentPositionOnTrack = offset;
            targetPositionOnTrack = offset;

            Vector3 targetnorm = track.TrackSegments[Helper.WrapInt(trackPos, track.TrackSegments.Count - 1)].Normal;
            Normal = targetnorm;
            Normal.Normalize();
            float targetDist = targetnorm.Length();
            Yaw = (float)Math.Atan2(targetnorm.X, targetnorm.Z);
            Pitch = (float)Math.Atan2(-targetnorm.Y, targetDist);
            Matrix rot = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
            CameraPosition = Position + Vector3.Transform(new Vector3(0, 0.3f, 1f), rot);
            CameraLookat = Position + Vector3.Transform(new Vector3(0, 0.25f, -1f), rot);
        }

        public void ApplyThrottle(bool isApplied)
        {
            applyingThrottle = isApplied;
        }

        public void ApplyBrake(bool isApplied)
        {
            applyingBrake = isApplied;
        }

        public void Steer(int direction)
        {
            applyingSteering = direction;
        }

        
    }
}
