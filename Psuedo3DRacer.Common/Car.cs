using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psuedo3DRacer.Common
{
    public class Car : IDisposable
    {
        static Random randomNumber = new Random();

        public bool IsPlayerControlled = false;

        public Vector3 Position;
        public Vector3 Normal;
        public float Yaw;
        public float Pitch;
        public float Speed = 0.0f;
        Vector3 travellingDirection;

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
        public bool Finished = false;

        public int CupPoints = 0;

        bool countedLap = false;

        double correctionCountdown = 0;

        public double immuneTime = 0;

        bool hasStarted = false;

        float applyingThrottle = 0f;
        public bool applyingBrake = false;
        float applyingSteering = 0;

        public float steeringAmount;

        public int currentTrackPos = 0;
        int prevTrackPos = 0;

        float currentPositionOnTrack = 0f;
        float targetPositionOnTrack = 0f;

        bool offRoad = false;

        bool overtaking = false;

        public bool camAttached = false;

        double spinTime = 0;
        float spinSpeed = 0f;
        double spinAnimTime = 0;
        int spinAnimFrame = 0;

        public string debug;

        Vector3 trackOffset = new Vector3(0, 0.10f, 0);

        Vector3 target;
        public int courseTrackPos = 0;

        public Texture2D[] texDirections1;
        Texture2D[] texDirections2;
        Texture2D[] texTurnLeft;
        Texture2D[] texTurnRight;

        public Color Tint;

        int animFrame = 0;
        double animTime = 0;

        public SoundEffectInstance engineSound;
        public SoundEffectInstance tyreSound;
        public SoundEffectInstance crashSound;
        public AudioEmitter emitter;

        public Car(int trackPos, float offset, Track track, Color tint)
        {
            Tint = tint;

            

            ConcentrationLevel = 50 + randomNumber.Next(1900);
            CorrectionTime = 500 + (randomNumber.NextDouble() * 4500);
            SpeedWhenTurning = 0.045f + ((float)randomNumber.NextDouble() * 0.01f);
            ReactionTime = 100 + (randomNumber.NextDouble() * 1900);

            Reset(trackPos, offset, track);

            
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

            AudioListener listener = new AudioListener();
            emitter = new AudioEmitter();
            //emitter.DopplerScale = 100f;
            //emitter.Velocity = Vector3.One;
            engineSound = content.Load<SoundEffect>("audio/sfx/engine_01").CreateInstance();
            tyreSound = content.Load<SoundEffect>("audio/sfx/screech").CreateInstance();
            crashSound = content.Load<SoundEffect>("audio/sfx/crash").CreateInstance();
            engineSound.IsLooped = true;
            engineSound.Volume = 0f;
            //engineSound.Apply3D(listener, emitter);
            engineSound.Play();
            tyreSound.IsLooped = true;
            tyreSound.Volume = 0f;
            //tyreSound.Apply3D(listener, emitter);

            crashSound.IsLooped = false;
            crashSound.Volume = 0f;
            
        }

        public void Reset(int trackPos, float offset, Track track)
        {
            StartedFirstLap = false;
            RaceDistanceToGo = 3 * track.Length;
            LapsToGo = 3;
            Finished = false;

            Yaw = 0f;
            Pitch = 0f;

            SetPosition(trackPos, track, offset);

            correctionCountdown = ReactionTime;

            Speed = 0f;
            hasStarted = false;


            countedLap = false;
            applyingBrake = false;
            applyingThrottle = 0f;
            applyingSteering = 0f;
            steeringAmount = 0f;

            currentTrackPos = 0;
            prevTrackPos = 0;

            offRoad = false;

            overtaking = false;

            spinTime = 0;
            spinSpeed = 0f;
            spinAnimFrame = 0;
            spinAnimTime = 0;


        }

        public void Update(GameTime gameTime, Track track, List<Car> gameCars)
        {
            debug = "";

            float dist = 99999f;
            for (int i = 0; i < track.TrackSegments.Count; i++)
                if ((Position - track.TrackSegments[i].Position).Length() < dist)
                {
                    dist = (Position - track.TrackSegments[i].Position).Length();
                    currentTrackPos = i;
                }


            if (immuneTime > 0) immuneTime -= gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!IsPlayerControlled)
            {
                if (!hasStarted)
                {
                    correctionCountdown -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (correctionCountdown <= 0)
                    {
                        hasStarted = true;
                        applyingThrottle = 1f;

                        correctionCountdown = ReactionTime;
                    }

                   
                }


                if ((Position - target).Length() < 0.5f)
                {
                    courseTrackPos += 1;
                    if (courseTrackPos > track.TrackSegments.Count - 1) courseTrackPos -= (track.TrackSegments.Count - 1);
                    PlotCourse(track);
                }

                if(!overtaking) currentPositionOnTrack = MathHelper.Lerp(currentPositionOnTrack, targetPositionOnTrack, 0.01f);

                if (hasStarted)
                {
                    if (Math.Abs(currentPositionOnTrack - targetPositionOnTrack) > 0.02f)
                    {
                        //if(currentPositionOnTrack-targetPositionOnTrack<0f) steeringAmount +=0.001f;
                        //if(currentPositionOnTrack-targetPositionOnTrack>0f) steeringAmount -=0.001f;
                        if (Speed > SpeedWhenTurning) applyingThrottle = 0f;
                        else
                            applyingThrottle = 1f;
                    }
                    else applyingThrottle = 1f;
                }

                if (correctionCountdown > 0) correctionCountdown -= gameTime.ElapsedGameTime.TotalMilliseconds;

                lastYaw = Yaw;

                Vector3 targetnorm = Position - target;
                Normal = targetnorm;
                Normal.Normalize();
                float targetDist = targetnorm.Length();
                Yaw = (float)Math.Atan2(targetnorm.X, targetnorm.Z);
                Pitch = (float)Math.Atan2(-targetnorm.Y, targetDist);

                //if (lastYaw > Yaw) steeringAmount += 0.01f;
                //if (lastYaw == Yaw)
                //{
                //    steeringAmount = MathHelper.Lerp(steeringAmount, 0f, 0.01f);
                //}
                //if (lastYaw < Yaw) steeringAmount -= 0.01f;

                //if (camAttached)
                //{
                    steeringAmount = MathHelper.Lerp(steeringAmount, 0f, 0.1f);
                    steeringAmount = MathHelper.Lerp(steeringAmount, steeringAmount + ((Yaw - lastYaw) * 2f), 0.5f);
                //}


                steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f);
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

                    steeringAmount = MathHelper.Clamp(steeringAmount, -0.5f, 0.5f);
                    Yaw += steeringAmount * 0.05f;// *Speed;


                }
                else steeringAmount = MathHelper.Lerp(steeringAmount, 0f, 0.1f);

                //Vector3 trackNormal = track.TrackSegments[currentTrackPos].Normal;
                //float trackYaw = MathHelper.WrapAngle((float)Math.Atan2(trackNormal.X, trackNormal.Z));
                //Yaw = MathHelper.Clamp(Yaw, trackYaw - (MathHelper.PiOver4/2), trackYaw + (MathHelper.PiOver4/2));

                target = track.TrackSegments[Helper.WrapInt(currentTrackPos + 5, track.TrackSegments.Count - 1)].Position + trackOffset;
                Vector3 targetnorm = Position - target;
                Pitch = (float)Math.Atan2(-targetnorm.Y, targetnorm.Length());

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
                if (applyingThrottle > 0f)
                {
                    if(Speed< 0.06f * applyingThrottle)
                        Speed += 0.0004f;
                    else
                        Speed -= 0.0004f;
                }
                else
                {
                    Speed -= 0.0004f;
                }
                if (applyingBrake)
                {
                    Speed -= 0.001f;
                    
                }
                Speed = MathHelper.Clamp(Speed, 0f, (0.06f));
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

            if (spinTime > 0)
            {
                spinTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
                target = track.TrackSegments[Helper.WrapInt(currentTrackPos + 10, track.Length - 1)].Position + trackOffset;
                Vector3 targetnorm = Position - target;
                Normal = targetnorm;
                Normal.Normalize();
                float targetDist = targetnorm.Length();
                Yaw = (float)Math.Atan2(targetnorm.X, targetnorm.Z);
                if (!IsPlayerControlled) PlotCourse(track);
            }

            if (offRoad || spinTime>0)
            {
                if (Speed > 0.02f) Speed = MathHelper.Lerp(Speed, 0.02f, 0.1f);
            }

            //Vector3 cornerAngle = track.TrackSegments[Helper.WrapInt(courseTrackPos, track.TrackSegments.Count - 1)].Normal;
            //float targetAngle = Helper.WrapAngle((float)Math.Atan2(cornerAngle.X, cornerAngle.Z));
            //targetPositionOnTrack = -MathHelper.Clamp((0.35f / 0.5f) * targetAngle, -0.35f, 0.35f);

            Matrix rot = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
            Vector3 rotatedVector = Vector3.Transform(Vector3.Forward, rot);

            //travellingDirection = Vector3.Lerp(travellingDirection, rotatedVector, 0.12f - (Speed));
           // travellingDirection.Y = rotatedVector.Y;

            Position += Speed * rotatedVector;//travellingDirection;

            Yaw = MathHelper.WrapAngle(Yaw);

            rotatedVector = Vector3.Transform(new Vector3(0, 0.2f, 0.75f), rot);
            CameraPosition = Vector3.Lerp(CameraPosition, Position + rotatedVector, 0.2f);
            rotatedVector = Vector3.Transform(new Vector3(0, 0.25f, -1f), rot);
            //rotatedVector = Vector3.Transform(rotatedVector, Matrix.CreateRotationZ(0.4f));
            CameraLookat = Vector3.Lerp(CameraLookat, Position + rotatedVector, 0.1f);
            //CameraLookat = Vector3.Transform(CameraLookat, Matrix.CreateRotationZ(0.4f));

            CheckCollisions(gameCars, track);

            // Calcuate position
            if (!gameCars[7].Finished)
            {
                RacePosition = 1;
                foreach (Car c in gameCars)
                {
                    if (c.RaceDistanceToGo < RaceDistanceToGo) RacePosition++;
                }
            }
            else
            {
                if (!Finished)
                {
                    Finished = true;

                    CupPoints += (9 - RacePosition);
                }
            }

            if (currentTrackPos == 0 && !countedLap)
            {
                countedLap = true;

                if (!StartedFirstLap) StartedFirstLap = true;
                else LapsToGo--;

                if (LapsToGo == 0)
                {
                    if (!Finished)
                    {
                        Finished = true;

                        CupPoints += (9 - RacePosition);
                    }

                    if (IsPlayerControlled)
                    {
                        IsPlayerControlled = false;
                        courseTrackPos = 20;
                        PlotCourse(track);
                    }
                }
            }

            if (currentTrackPos == track.Length / 2) countedLap = false;

            RaceDistanceToGo = ((LapsToGo + (StartedFirstLap?0:1)) * track.Length) - (currentTrackPos);

            

            //debug = "Lap: " + (4 - LapsToGo) + " | Pos: " + RacePosition;
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
                
                Vector3 leftV = Vector3.Cross(track.TrackSegments[currentTrackPos].Normal, Vector3.Up) * 0.5f;
                Vector3 rightV = -(Vector3.Cross(track.TrackSegments[currentTrackPos].Normal, Vector3.Up) * 0.5f);

                if (((Position - trackOffset) - (track.TrackSegments[currentTrackPos].Position + leftV)).Length() < ((Position + trackOffset) - (track.TrackSegments[currentTrackPos].Position + rightV)).Length())
                {
                    currentPositionOnTrack = -((Position - trackOffset) - track.TrackSegments[currentTrackPos].Position).Length();
                    collScenery = track.TrackSegments[currentTrackPos].LeftScenery;
                }
                else
                {
                    currentPositionOnTrack = ((Position - trackOffset) - track.TrackSegments[currentTrackPos].Position).Length();
                    collScenery = track.TrackSegments[currentTrackPos].RightScenery;
                }

                if (currentPositionOnTrack > 1f || currentPositionOnTrack < -1f) collScenery = SceneryType.Wall;
                if (track.TrackSegments[currentTrackPos].Position.Y > 0.05f || track.TrackSegments[currentTrackPos].Position.Y < -0.1f) collScenery = SceneryType.Wall;

                if (currentPositionOnTrack > 0.5f || currentPositionOnTrack < -0.5f) offRoad = true;

                
                
            }
            else
            {
                overtaking = false;
                int foundCarDistance = 99999;

                foreach (Car c in gameCars)
                {
                    if (c == this) continue;

                    int trackDist = Helper.WrapInt(c.currentTrackPos - currentTrackPos, track.Length - 1);
                    if (trackDist > 0 && trackDist<10  + (30-(c.Speed * 500)) && ((Speed>=c.Speed && trackDist<foundCarDistance) || c.IsPlayerControlled))
                    {
                    //if ((c.Position - Position).Length() < 10f && ((c.Position - Position).Length()<foundCarDistance || c.IsPlayerControlled))
                    //{
                        if (c.IsPlayerControlled) foundCarDistance = 0;
                        else foundCarDistance = trackDist;// (c.Position - Position).Length();

                        if (currentPositionOnTrack < c.currentPositionOnTrack)
                        {
                            targetPositionOnTrack = c.currentPositionOnTrack - (0.4f);
                            if (targetPositionOnTrack < -0.45f) targetPositionOnTrack = c.currentPositionOnTrack + (0.35f);
                        }
                        else
                        {
                            targetPositionOnTrack = c.currentPositionOnTrack + (0.4f);
                            if (targetPositionOnTrack > 0.45f) targetPositionOnTrack = c.currentPositionOnTrack - (0.35f);
                        }
                        //if (c.currentPositionOnTrack <= 0f) targetPositionOnTrack = c.currentPositionOnTrack + (0.4f);// * (3f - (c.Position - Position).Length()));
                        //else targetPositionOnTrack = c.currentPositionOnTrack - (0.4f);// * (3f - (c.Position - Position).Length()));
                        currentPositionOnTrack = MathHelper.Lerp(currentPositionOnTrack, targetPositionOnTrack, 0.02f);
                        overtaking = true;
                        PlotCourse(track);
                        //}
                    }
                }

                //debug += foundCarDistance;
            }

            foreach (Car c in gameCars)
            {
                if (c == this) continue;

                if ((c.Position - Position).Length() < 0.15f && c.Speed <= Speed && c.spinTime<=0 && spinTime<=0 && c.immuneTime<=0 && immuneTime<=0)
                {
                    int trackDist = Helper.WrapInt(c.currentTrackPos - currentTrackPos, track.Length - 1);
                    if (trackDist > 0)
                    {
                        crashSound.Play();
                        spinTime = 1600;
                        spinSpeed = 0.03f;
                        immuneTime = 3000;
                        c.spinTime = 800;
                        c.spinSpeed = 1f;
                        c.immuneTime = 3000;
                    }
                }

            }

            if (offRoad && collScenery == SceneryType.Wall)
            {
                crashSound.Play();
                spinTime = 1600;
                spinSpeed = Speed;
            }

            
        }

        float lastYaw;
        void PlotCourse(Track track)
        {
            
            target = track.TrackSegments[Helper.WrapInt(courseTrackPos, track.TrackSegments.Count - 1)].Position;

            Vector3 drivingLineTarget = track.TrackSegments[Helper.WrapInt(courseTrackPos + 50, track.TrackSegments.Count - 1)].Normal;
            float targetAngle = Helper.WrapAngle((float)Math.Atan2(drivingLineTarget.X, drivingLineTarget.Z) - Yaw);
            //debug = targetAngle + " | " + targetPositionOnTrack.ToString();
            

            if (correctionCountdown <= 0)
            {
                if (!overtaking)
                {
                    targetPositionOnTrack = -MathHelper.Clamp((0.35f / 0.5f) * targetAngle, -0.35f, 0.35f);

                    if (randomNumber.Next(ConcentrationLevel) == 1)
                    {
                        if (targetPositionOnTrack > -0.25f && targetPositionOnTrack < 0.25f)
                        {
                            if (randomNumber.Next(2) == 1)
                            {
                                targetPositionOnTrack = ((float)randomNumber.NextDouble() * -0.2f) - 0.25f;
                            }
                            else
                            {
                                targetPositionOnTrack = ((float)randomNumber.NextDouble() * 0.2f) + 0.25f;
                            }
                        }
                        else if (targetPositionOnTrack <= -0.25f)
                        {
                            targetPositionOnTrack = ((float)randomNumber.NextDouble() * 0.45f);
                        }
                        else if (targetPositionOnTrack >= 0.25f)
                        {
                            targetPositionOnTrack = ((float)randomNumber.NextDouble() * -0.45f);
                        }
                    }
                }
            }

            //debug = correctionCountdown + " | " + CorrectionTime + " | " + ConcentrationLevel;

            Vector3 leftV = -Vector3.Cross(track.TrackSegments[Helper.WrapInt(courseTrackPos, track.TrackSegments.Count - 1)].Normal, Vector3.Up);
            target += leftV * currentPositionOnTrack;

            target += trackOffset;
        }

        Texture2D GetTextureForDirection(Vector3 direction, bool attachedToCar)
        {
            if (spinTime > 0)
            {
                return texDirections1[spinAnimFrame];
            }

            if (!camAttached)// || !IsPlayerControlled)
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

            Vector3 leftV = -Vector3.Cross(track.TrackSegments[Helper.WrapInt(trackPos, track.TrackSegments.Count - 1)].Normal, Vector3.Up);
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

        public void ApplyThrottle(float applied)
        {
            applyingThrottle = applied;
        }

        public void ApplyBrake(bool isApplied)
        {
            applyingBrake = isApplied;
        }

        public void Steer(float direction)
        {
            applyingSteering = direction;
        }



        public void Dispose()
        {
            if (engineSound.State == SoundState.Paused || engineSound.State == SoundState.Playing)
            {
                engineSound.Stop();
            }
            if (tyreSound.State == SoundState.Paused || engineSound.State == SoundState.Playing)
                tyreSound.Stop();

            engineSound.Dispose();
            tyreSound.Dispose();
        }
    }
}
