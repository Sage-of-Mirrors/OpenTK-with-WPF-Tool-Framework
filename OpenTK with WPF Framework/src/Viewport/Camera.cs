using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

namespace OpenTKFramework
{
    public class Camera
    {
        public Matrix4 ViewMatrix
        {
            get { return Matrix4.LookAt(eye, target, Vector3.UnitY); }
        }

        public Vector3 EyePos
        {
            get { return eye; }
            set { eye = value; }
        }

        Vector3 eye = new Vector3(0, 0, 0);

        public Vector3 TargetPos
        {
            get { return target; }
            set { target = value; }
        }

        Vector3 target = Vector3.Zero;

        Transform Trans = new Transform();

        public void Update()
        {
            float MoveSpeed = 50f;

            Vector3 moveDir = Vector3.Zero;

            if (Input.GetKey(Keys.W))
            {
                moveDir += Vector3.UnitZ;
            }

            if (Input.GetKey(Keys.S))
            {
                moveDir -= Vector3.UnitZ;
            }

            if (Input.GetKey(Keys.A))
            {
                moveDir += Vector3.UnitX;
            }

            if (Input.GetKey(Keys.D))
            {
                moveDir -= Vector3.UnitX;
            }

            if (Input.GetKey(Keys.E))
            {
                moveDir += Vector3.UnitY;
            }

            if (Input.GetKey(Keys.Q))
            {
                moveDir -= Vector3.UnitY;
            }

            if (Input.GetMouseButton(1))
            {
                Rotate(Input.MouseDelta.X, Input.MouseDelta.Y);
            }

            float moveSpeed = Input.GetKey(Keys.Space) ? MoveSpeed * 10f : MoveSpeed;

            // Normalize the move direction
            moveDir.NormalizeFast();

            // Make it relative to the current rotation.
            moveDir = Trans.Rotation.Multiply(moveDir);

            Trans.Position += Vector3.Multiply(moveDir, moveSpeed);

            eye = Trans.Position;

            target = Trans.Position + Trans.Forward;
        }

        private void Rotate(float x, float y)
        {
            Trans.Rotate(Vector3.UnitY, -x);
            Trans.Rotate(Trans.Right, y);

            // Clamp them from looking over the top point.
            Vector3 up = Vector3.Cross(Trans.Forward, Trans.Right);
            if (Vector3.Dot(up, Vector3.UnitY) < 0.01f)
            {
                Trans.Rotate(Trans.Right, -y);
            }
        }

        #region Raycasting

        internal Color4 CastRay(int mouseX, int mouseY, float controlWidth, float controlHeight, Matrix4 projMatrix)
        {
            Vector3 normDevCoordsRay = new Vector3((2.0f * mouseX) / controlWidth - 1.0f,
                1.0f - (2.0f * mouseY) / controlHeight, -1.0f);

            Vector4 clipRay = new Vector4(normDevCoordsRay, 1.0f);

            Vector4 eyeRay = Vector4.Transform(clipRay, Matrix4.Invert(projMatrix));

            eyeRay = new Vector4(eyeRay.X, eyeRay.Y, -1, 0);

            Vector3 unNormalizedRay = new Vector3(Vector4.Transform(eyeRay, Matrix4.Invert(ViewMatrix)).Xyz);

            Vector3 normalizedRay = Vector3.Normalize(unNormalizedRay);

            return CheckHitAxisAlignedBoundingBox(eye, normalizedRay, new Vector3(-25, -25, -25), new Vector3(25, 25, 25));
        }

        internal Color4 CheckHitAxisAlignedBoundingBox(Vector3 eye, Vector3 ray, Vector3 lowerBound, Vector3 upperBound)
        {
            Vector3 dirFrac = new Vector3(1.0f / ray.X, 1.0f / ray.Y, 1.0f / ray.Z);

            float t1 = (lowerBound.X - eye.X) * dirFrac.X;
            float t2 = (upperBound.X - eye.X) * dirFrac.X;
            float t3 = (lowerBound.Y - eye.Y) * dirFrac.Y;
            float t4 = (upperBound.Y - eye.Y) * dirFrac.Y;
            float t5 = (lowerBound.Z - eye.Z) * dirFrac.Z;
            float t6 = (upperBound.Z - eye.Z) * dirFrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            if (tmax < 0)
                return Color4.Yellow;

            if (tmin > tmax)
                return Color4.Yellow;

            else
                return Color4.Red;
        }

        internal void CheckHitBoundingSphere(Vector3 eye, Vector3 ray, float radius)
        {
            Vector3 position = new Vector3();

            float b = Vector3.Dot(ray, (eye - position));

            float c = Vector3.Dot((eye - position), (eye - position));

            c = c - radius;

            float a = (b * b) - c;

            //if (a >= 0)
            //debugRayColor = Color4.Red;

            //else
            //debugRayColor = Color4.Yellow;
        }

        #endregion
    }

    public class Transform
    {
        public Transform()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Vector3 Right
        {
            get { return Rotation.Multiply(Vector3.UnitX); }
        }

        public Vector3 Forward
        {
            get { return Rotation.Multiply(Vector3.UnitZ); }
        }

        public Vector3 Up
        {
            get { return Rotation.Multiply(Vector3.UnitY); }
        }

        public void LookAt(Vector3 worldPosition)
        {
            Rotation = Quaternion.FromAxisAngle(Vector3.Normalize((Position - worldPosition)), 0f);
        }

        public void Rotate(Vector3 axis, float angleInDegrees)
        {
            Quaternion rotQuat = Quaternion.FromAxisAngle(axis, MathHelper.DegreesToRadians(angleInDegrees));
            Rotation = rotQuat * Rotation;
        }

        public void Translate(Vector3 amount)
        {
            Position += amount;
        }
    }
}
