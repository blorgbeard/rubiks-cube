using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace RubiksCube {
    public class BasicCamera : ICamera {

        private float _AspectRatio;
        private float _FarPlane;
        private float _NearPlane;
        private float _FieldOfView;
        private Vector3 _Up;
        private Vector3 _LookAt;
        private Vector3 _Position;
        private bool IsViewDirty;
        private bool IsProjectionDirty;

        public Vector3 Position {
            get { return _Position; }
            set {
                _Position = value;
                IsViewDirty = true;
            }
        }
        public Vector3 LookAt {
            get { return _LookAt; }
            set {
                _LookAt = value;
                IsViewDirty = true;
            }
        }
        public Vector3 Up {
            get { return _Up; }
            set {
                _Up = value;
                IsViewDirty = true;
            }
        }
        public float FieldOfView {
            get { return _FieldOfView; }
            set {
                _FieldOfView = value;
                IsProjectionDirty = true;
            }
        }
        public float NearPlane {
            get { return _NearPlane; }
            set {
                _NearPlane = value;
                IsProjectionDirty = true;
            }
        }
        public float FarPlane {
            get { return _FarPlane; }
            set {
                _FarPlane = value;
                IsProjectionDirty = true;
            }
        }
        public float AspectRatio {
            get { return _AspectRatio; }
            set {
                _AspectRatio = value;
                IsProjectionDirty = true;
            }
        }

        public BasicCamera(Vector3 position, Vector3 lookAt, float aspectRatio) {
            Position = position;
            LookAt = lookAt;
            AspectRatio = aspectRatio;
            Up = Vector3.Up;
            FieldOfView = MathHelper.PiOver4;
            NearPlane = 1f;
            FarPlane = 150f;
        }

        private Matrix _ViewMatrix;
        public Matrix ViewMatrix {
            get {
                if (IsViewDirty) {
                    _ViewMatrix = Matrix.CreateLookAt(Position, LookAt, Up);
                    IsViewDirty = false;
                }
                return _ViewMatrix;
            }
        }

        private Matrix _ProjectionMatrix;
        public Matrix ProjectionMatrix {
            get {
                if (IsProjectionDirty) {
                	_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
                    IsProjectionDirty = false;
                }
                return _ProjectionMatrix;
            }
        }
    }
}
