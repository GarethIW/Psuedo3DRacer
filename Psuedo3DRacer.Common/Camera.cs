using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psuedo3DRacer.Common
{
    public class Camera
    {
        public Matrix worldMatrix;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        public Vector3 Position = new Vector3(0, 10, 30);
        public float Yaw = 0;//MathHelper.PiOver2;
        public float Pitch = 0;//-MathHelper.Pi / 10.0f;

        public bool AttachedToCar = false;

        const float rotationSpeed = 0.01f;
        const float moveSpeed = 0.1f;

        public Camera(GraphicsDevice gd)
        {
            worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.CreateLookAt(Position, new Vector3(0, 0, -100), Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, gd.Viewport.AspectRatio, 0.001f, 100f);
        }

        public void AddToPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            Position += moveSpeed * rotatedVector;
            UpdateViewMatrix(0f);
        }

        //public void GoToPosition(Vector3 position, float leftright, float updown)
        //{
        //    leftrightRot = leftright;
        //    updownRot = updown;
        //    cameraPosition = position;
        //    UpdateViewMatrix();
        //}

        public void Rotate(float yaw, float pitch)
        {
            Yaw -= rotationSpeed * yaw;
            Pitch -= rotationSpeed * pitch;
            UpdateViewMatrix(0f);
        }

        private void UpdateViewMatrix(float roll)
        {
            Matrix cameraRotation = Matrix.CreateRotationZ(roll) * Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = Position + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(Position, cameraFinalTarget, cameraRotatedUpVector);
        }

        public Matrix ViewMatrixUpDownOnly()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(Pitch);

            Vector3 thisCamPosition = new Vector3(0f, Position.Y, 0f);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = thisCamPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            return Matrix.CreateLookAt(thisCamPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

        public void LookAt(Vector3 lookat, float roll)
        {
            Vector3 lookVect = Position - lookat;
            float dist = (Position - lookat).Length();
            Yaw = (float)Math.Atan2(lookVect.X, lookVect.Z);
            Pitch = (float)Math.Atan2(-lookVect.Y, dist);
            UpdateViewMatrix(roll);
        }

        
    }
}
