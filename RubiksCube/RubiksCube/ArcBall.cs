using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RubiksCube {
    public class ArcBall {

        public bool IsDragging { get; private set; }

        public GraphicsDevice Device { get; set; }
        public ICamera Camera { get; set; }

        public Matrix Rotation { get; set; }

        private readonly float BallRadiusSquared = 49f;
        private Matrix PreviousRotation { get; set; }
        private Vector3 StartPointOnSphere;

        private Vector3 GetPointOnSphere(Vector2 mousePosition) {
            Vector3 nearsource = new Vector3(mousePosition, 0f);
            Vector3 farsource = new Vector3(mousePosition, 1f);
            var nearpoint = Device.Viewport.Unproject(nearsource, Camera.ProjectionMatrix, Camera.ViewMatrix, Matrix.Identity);
            var farpoint = Device.Viewport.Unproject(farsource, Camera.ProjectionMatrix, Camera.ViewMatrix, Matrix.Identity);
            var direction = (farpoint - nearpoint);
            direction.Normalize();
            var ray = new Ray(nearpoint, direction);
            float? distanceAtZeroZ = ray.Intersects(new Plane(Vector3.Forward, 0f));
            var pointAtZeroZ = ray.Position + (ray.Direction * distanceAtZeroZ.Value);
            float pzLength2 = pointAtZeroZ.LengthSquared();
            if (pzLength2 <= BallRadiusSquared) {
                Vector3 result = new Vector3(pointAtZeroZ.X, pointAtZeroZ.Y, (float)Math.Sqrt(BallRadiusSquared - pzLength2));
                result.Normalize();
                return result;
            }
            else {
                pointAtZeroZ.Normalize();                
                return pointAtZeroZ;
            }
        }

        public bool StartDrag(MouseState mouse) {
            var MouseStart = new Vector2(mouse.X, mouse.Y);
            Vector3 pointOnSphere = GetPointOnSphere(MouseStart);
            if (pointOnSphere != Vector3.Zero) {
                PreviousRotation = Rotation;
                StartPointOnSphere = pointOnSphere;
                IsDragging = true;
                return true;
            }
            return false;
        }

        public void UpdateDrag(MouseState mouse) {
            var mousePos = new Vector2(mouse.X, mouse.Y);
            var pointOnSphere = GetPointOnSphere(mousePos);
            if (pointOnSphere != Vector3.Zero) {
                if ((StartPointOnSphere - pointOnSphere).LengthSquared() <= 0.000001f) {
                    Rotation = PreviousRotation;
                    return;
                }
                // cross product gives us an axis perpendicular to both
                var axis = Vector3.Cross(StartPointOnSphere, pointOnSphere);
                axis.Normalize();
                // dot product gives us cos(angle) between them
                var angle = (float)Math.Acos(Vector3.Dot(StartPointOnSphere, pointOnSphere));
                // get rotation matrix from those
                var rotate = Matrix.CreateFromAxisAngle(axis, angle * 4f);
                Rotation = PreviousRotation * rotate;
            }            
        }

        public void EndDrag() {
            IsDragging = false;
        }

        public ArcBall(GraphicsDevice device, ICamera camera) {
            Device = device;
            Camera = camera;
            PreviousRotation = Matrix.Identity;
            Rotation = Matrix.Identity;
        }

    }
}
