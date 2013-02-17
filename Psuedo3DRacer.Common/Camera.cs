using Microsoft.Xna.Framework;
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

        Vector3 cameraPosition = new Vector3(0, 10, 30);
        float leftrightRot = 0;//MathHelper.PiOver2;
        float updownRot = 0;//-MathHelper.Pi / 10.0f;
        const float rotationSpeed = 0.01f;
        const float moveSpeed = 0.1f;

        public Camera()
        {
            worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 0, -100), Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4.0f / 3.0f, 0.001f, 500);
        }

        public void AddToPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        //public void GoToPosition(Vector3 position, float leftright, float updown)
        //{
        //    leftrightRot = leftright;
        //    updownRot = updown;
        //    cameraPosition = position;
        //    UpdateViewMatrix();
        //}

        public void Rotate(float leftright, float updown)
        {
            leftrightRot -= rotationSpeed * leftright;
            updownRot -= rotationSpeed * updown;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

        
    }
}
