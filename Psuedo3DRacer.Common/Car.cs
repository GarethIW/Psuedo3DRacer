﻿using Microsoft.Xna.Framework;
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

        public int RacePosition = 0;
        public int RaceDistanceToGo = 0;
        public int LapsToGo = 0;
        public bool StartedFirstLap = false;

        bool countedLap = false;

        double correctionCountdown = 0;

        bool hasStarted = false;

        bool applyingThrottle = false;
        bool applyingBrake = false;
        float applyingSteering = 0;

        public float steeringAmount;

        public int currentTrackPos = 0;
        int prevTrackPos = 0;

        float currentPositionOnTrack = 0f;
        float targetPositionOnTrack = 0f;

        bool offRoad = false;

        double spinTime = 0;
        float spinSpeed = 0f;
        double spinAnimTime = 0;
        int spinAnimFrame = 0;

        public string debug;

        Vector3 trackOffset = new Vector3(0, 0.13f, 0);

        Vector3 target;
        public int courseTrackPos = 0;

        Texture2D[] texDirections1;
        Texture2D[] texDirections2;
        Texture2D[] texTurnLeft;
        Texture2D[] texTurnRight;

        public Color Tint;

        int animFrame = 0;
        double animTime = 0;

        float testPitch;

        public Car(int trackPos, float offset, Track track, Color tint)
        {
            Tint = tint;

            StartedFirstLap = false;
            RaceDistanceToGo = 3 * track.Length;
            LapsToGo = 3;

            ConcentrationLevel = 50 + randomNumber.Next(1900);
            CorrectionTime = 500 + (randomNumber.NextDouble() * 4500);
            SpeedWhenTurning = 0.045f + ((float)randomNumber.NextDouble() * 0.01f);
            ReactionTime = 100 + (randomNumber.NextDouble() * 1900);
            

            SetPosition(trackPos, track, offset);

            correctionCountdown = ReactionTime;
        }

        public void LoadContent(ContentManager content, int carnum)
        {
            texDirections1 = new Texture2D[8];
            texDirections2 = new Texture2D[8];
            for (int dir = 0; dir < 8; dir++)
            {
                texDirections1[dir] = content.Load<Texture2D>("cars/" + carnum + "-" + dir + "-0");
                texDirections2[dir] = content.Load<Texture2D>("cars/" + carnum + "-" + dir + "-1");
            }

            texTurnLeft = new Texture2D[6];
            texTurnRight = new Texture2D[6];
            for (int turn = 0; turn < 3; turn++)
            {
                texTurnLeft[turn] = content.Load<Texture2D>("cars/" + carnum + "-turnl" + turn + "-0");
                texTurnLeft[turn+3] = content.Load<Texture2D>("cars/" + carnum + "-turnl" + turn + "-1");
                texTurnRight[turn] = content.Load<Texture2D>("cars/" + carnum + "-turnr" + turn + "-0");
                texTurnRight[turn+3] = content.Load<Texture2D>("cars/" + carnum + "-turnr" + turn + "-1");
            }

        }

        public void Update(GameTime gameTime, Track track, List<Car> gameCars)
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
                    //if (Math.Abs(applyingSteering) > 0.15f)
                    //{
                    //    steeringAmount = (-applyingSteering * 0.03f);
                    //}
                    //else
                    //{
                    //    steeringAmount = MathHelper.Lerp(steeringAmount, 0f, 0.2f);
                    //}

                    steeringAmount = (-applyingSteering);

                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.6f, 0.6f);
                    Yaw += steeringAmount * 0.05f;// *Speed;
                }

                //Vector3 trackNormal = track.TrackSegments[currentTrackPos].Normal;
                //float trackYaw = MathHelper.WrapAngle((float)Math.Atan2(trackNormal.X, trackNormal.Z));
                //Yaw = MathHelper.Clamp(Yaw, trackYaw - (MathHelper.PiOver4/2), trackYaw + (MathHelper.PiOver4/2));

                target = track.TrackSegments[Helper.WrapInt(currentTrackPos + 5, track.TrackSegments.Count - 1)].Position + trackOffset;
                Vector3 targetnorm = Position - target;
                Pitch = (float)Math.Atan2(-targetnorm.Y, targetnorm.Length());

                if (spinTime > 0)
                {
                    spinTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    target = track.TrackSegments[Helper.WrapInt(currentTrackPos+10, track.Length-1)].Position;
                    targetnorm = Position - target;
                    Normal = targetnorm;
                    Normal.Normalize();
                    float targetDist = targetnorm.Length();
                    Yaw = (float)Math.Atan2(targetnorm.X, targetnorm.Z);
                }

                Matrix normRot = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
                Normal = Position - Vector3.Transform(Vector3.Forward * 100f, normRot);
                Normal.Normalize();
            }

            animTime += (gameTime.ElapsedGameTime.TotalMilliseconds * (Speed * 10));
            if (animTime >= 30)
            {
                animFrame++;
                if (animFrame == 2) animFrame = 0;
                animTime = 0;
            }

            if (spinTime <= 0)
            {
                if (applyingThrottle) Speed += 0.0004f;
                else Speed -= 0.0004f;
                if (applyingBrake) Speed -= 0.001f;
                Speed = MathHelper.Clamp(Speed, 0f, 0.06f);
            }

            if (spinTime > 0)
            {
                spinAnimTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (spinAnimTime > 200)
                {
                    spinAnimFrame++;
                    if (spinAnimFrame == 8) spinAnimFrame = 0;
                    spinAnimTime = 0;
                }
            }

            if (offRoad || spinTime>0)
            {
                if (Speed > 0.02f) Speed = MathHelper.Lerp(Speed, 0.02f, 0.1f);
            }
           
            Matrix rot = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
            Vector3 rotatedVector = Vector3.Transform(Vector3.Forward, rot);
            Position += Speed * rotatedVector;

            Yaw = MathHelper.WrapAngle(Yaw);

            rotatedVector = Vector3.Transform(new Vector3(0, 0.2f, 1f), rot);
            CameraPosition = Vector3.Lerp(CameraPosition, Position + rotatedVector, 0.1f);
            rotatedVector = Vector3.Transform(new Vector3(0, 0.25f, -1f), rot);
            //rotatedVector = Vector3.Transform(rotatedVector, Matrix.CreateRotationZ(0.4f));
            CameraLookat = Vector3.Lerp(CameraLookat, Position + rotatedVector, 0.1f);
            //CameraLookat = Vector3.Transform(CameraLookat, Matrix.CreateRotationZ(0.4f));

            CheckCollisions(gameCars, track);

            // Calcuate position
            RacePosition = 1;
            foreach (Car c in gameCars)
            {
                if (c.RaceDistanceToGo < RaceDistanceToGo) RacePosition++;
            }

            if (currentTrackPos == 0 && !countedLap)
            {
                countedLap = true;

                if (!StartedFirstLap) StartedFirstLap = true;
                else LapsToGo--;
            }

            if (currentTrackPos == track.Length / 2) countedLap = false;

            RaceDistanceToGo = ((LapsToGo + (StartedFirstLap?0:1)) * track.Length) - (currentTrackPos);

            debug = "Lap: " + (4 - LapsToGo) + " | Pos: " + RacePosition;
        }

        public void Draw(GraphicsDevice gd, AlphaTestEffect effect, Camera gameCamera)
        {
            int oldRefAlpha = effect.ReferenceAlpha;

            gd.BlendState = BlendState.AlphaBlend;
            //gd.DepthStencilState = DepthStencilState.Default;
            gd.SamplerStates[0] = SamplerState.LinearClamp;

            effect.ReferenceAlpha = 150;

            Vector3 finalFace = Vector3.Zero;

           
            Vector3 faceCam = gameCamera.Position - Position; 
            float camAngle = Helper.WrapAngle((float)Math.Atan2(faceCam.X, faceCam.Z));

            Matrix rot = Matrix.CreateRotationY(camAngle);
            finalFace = Vector3.Transform(Vector3.Forward, rot);

            faceCam.Normalize();
            effect.Texture = GetTextureForDirection(faceCam, gameCamera.AttachedToCar);
        

            Quad quad;
            
            effect.DiffuseColor = Tint.ToVector3();
            quad = new Quad(Position, finalFace, Vector3.Up, 0.4f, 0.15f);
            Drawing.DrawQuad(effect, quad, gd);

            effect.ReferenceAlpha = oldRefAlpha;
        }

        void CheckCollisions(List<Car> gameCars, Track track)
        {
            SceneryType collScenery = SceneryType.Offroad;
            offRoad = false;

            if (IsPlayerControlled)
            {
                currentPositionOnTrack = (Position - track.TrackSegments[currentTrackPos].Position).Length();
                Vector3 leftV = Vector3.Cross(track.TrackSegments[currentTrackPos].Normal, Vector3.Up) * 0.5f;
                Vector3 rightV = -Vector3.Cross(track.TrackSegments[currentTrackPos].Normal, Vector3.Up) * 0.5f;

                if ((Position - leftV).Length() < (Position - rightV).Length())
                {
                    collScenery = track.TrackSegments[currentTrackPos].LeftScenery;
                }
                else
                {
                    collScenery = track.TrackSegments[currentTrackPos].RightScenery;
                }

                if (currentPositionOnTrack > 1f) collScenery = SceneryType.Wall;
                if (track.TrackSegments[currentTrackPos].Position.Y > 0.1f || track.TrackSegments[currentTrackPos].Position.Y < -0.1f) collScenery = SceneryType.Wall;

                if (currentPositionOnTrack > 0.5f) offRoad = true;
                
            }
            else
            {

            }

            if (offRoad && collScenery == SceneryType.Wall)
            {
                spinTime = 1600;
                spinSpeed = Speed;
            }

            debug = offRoad.ToString() + " | " + Enum.GetName(typeof(SceneryType), collScenery);
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

            //debug = correctionCountdown + " | " + CorrectionTime + " | " + ConcentrationLevel;

            Vector3 leftV = Vector3.Cross(track.TrackSegments[Helper.WrapInt(courseTrackPos, track.TrackSegments.Count - 1)].Normal, Vector3.Up);
            target += leftV * currentPositionOnTrack;

            target += trackOffset;
        }

        Texture2D GetTextureForDirection(Vector3 direction, bool attachedToCar)
        {
            if (spinTime > 0)
            {
                return texDirections1[spinAnimFrame];
            }

            if (!attachedToCar || !IsPlayerControlled)
            {
                float normAngle = Helper.WrapAngle((float)Math.Atan2(Normal.X, Normal.Z));
                float camAngle = Helper.WrapAngle((float)Math.Atan2(direction.X, direction.Z));

                float testAngle = Helper.WrapAngle(normAngle - camAngle);

                int returnTex = 0;

                if ((testAngle <= -(MathHelper.PiOver4 * 3.5) && testAngle > -(MathHelper.PiOver4 * 4)) || (testAngle >= 0f && testAngle < (MathHelper.PiOver4 * 0.5))) returnTex = 0;
                if (testAngle >= (MathHelper.PiOver4 * 0.5) && testAngle < (MathHelper.PiOver4 * 1.5)) returnTex = 1;
                if (testAngle >= (MathHelper.PiOver4 * 1.5) && testAngle < (MathHelper.PiOver4 * 2.5)) returnTex = 2;
                if (testAngle >= (MathHelper.PiOver4 * 2.5) && testAngle < (MathHelper.PiOver4 * 3.5)) returnTex = 3;
                if ((testAngle >= (MathHelper.PiOver4 * 3.5) && testAngle < (MathHelper.PiOver4 * 4)) || (testAngle > -(MathHelper.PiOver4 * 4) && testAngle < -(MathHelper.PiOver4 * 3.5))) returnTex = 4;
                if (testAngle <= -(MathHelper.PiOver4 * 2.5) && testAngle > -(MathHelper.PiOver4 * 3.5)) returnTex = 5;
                if (testAngle <= -(MathHelper.PiOver4 * 1.5) && testAngle > -(MathHelper.PiOver4 * 2.5)) returnTex = 6;
                if (testAngle <= -(MathHelper.PiOver4 * 0.5) && testAngle > -(MathHelper.PiOver4 * 1.5)) returnTex = 7;

                return (animFrame == 0 ? texDirections1[returnTex] : texDirections2[returnTex]);
            }
            else
            {
                float steer = Math.Abs(steeringAmount);

                if (steeringAmount < 0)
                {
                    
                    if (steer < 0.1f) return (animFrame == 0 ? texDirections1[0] : texDirections2[0]);
                    if (steer < 0.2f) return texTurnLeft[0 + (3 * animFrame)];
                    if (steer < 0.3f) return texTurnLeft[1 + (3 * animFrame)];
                    return texTurnLeft[2 + (3 * animFrame)];


                    
                }
                else
                {
                    if (steer < 0.1f) return (animFrame == 0 ? texDirections1[0] : texDirections2[0]);
                    if (steer < 0.2f) return texTurnRight[0 + (3 * animFrame)];
                    if (steer < 0.3f) return texTurnRight[1 + (3 * animFrame)];
                    return texTurnRight[2 + (3 * animFrame)];
                }
            }

            

            return (animFrame == 0 ? texDirections1[0] : texDirections2[0]);
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

        public void Steer(float direction)
        {
            applyingSteering = direction;
        }

        
    }
}
