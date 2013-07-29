using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube {

    public class CubeFace {

        private readonly CubieFace CenterCubie;
        private CubieFace[] Cubies;         // Not counting centre! Ordered around the face going clockwise. Actual start point undefined at this level.
        private CubieFace[] LinkedCubies;   // Ordered clockwise, starting at the same point as Cubies. two entries for each corner, one for each edge cubie. These point to other faces.

        public int Index; // ugh..

        private readonly Matrix Transform;

        public bool RotatedForMoveHint { get; private set; }
        public void SetRotationForHint(bool clockwise) {
            SetRotation(MathHelper.PiOver4 / 4 * (clockwise ? -1 : 1));
            RotatedForMoveHint = true;
        }
        public void ClearRotationForHint() {
            SetRotation(0);
            RotatedForMoveHint = false;
        }

        public float RotateAngle { get; private set; }
        
        public void SetRotation(float angle) {
            RotateAngle = angle;
            var rotate = Matrix.CreateRotationZ(angle);
            CenterCubie.RotateTransform = rotate;
            for (int i = 0; i < Cubies.Length; i++) {
                Cubies[i].RotateTransform = rotate;
            }
            // linked cubies must rotate about the normal of this face 
            var spin = Matrix.CreateFromAxisAngle(Vector3.TransformNormal(Vector3.Backward, Transform), angle);
            for (int i = 0; i < LinkedCubies.Length; i++) {
                LinkedCubies[i].SpinTransform = spin;
            }
        }

        public void FuckThis(Random rnd) {
            CenterCubie.BeginExplosion(rnd);
            foreach (var facelet in Cubies) facelet.BeginExplosion(rnd);
        }

        public void AnimateExplosion(float time) {
            CenterCubie.Explode(time);
            foreach (var facelet in Cubies) facelet.Explode(time);
        }

        public CubieFace this[int index] {
            get { return Cubies[index]; }
        }

        public CubieFace Cubie(int index) {
            return this[index];
        }

        public void SetLinkedCubies(IEnumerable<CubieFace> cubies) {
            this.SetLinkedCubies(cubies);
        }

        public void SetLinkedCubies(params CubieFace[] cubies) {
            LinkedCubies = cubies.ToArray();
        }

        public CubeFace GetLinkedFace(int cubieIndex, int faceIndex) {
            return LinkedCubies[CubieToFirstLink[cubieIndex] + faceIndex].ParentFace;            
        }

        private readonly static List<int> CubieToFirstLink = new List<int> {
            0, 2, 3, 5, 6, 8, 9, 11
        };

        public CubeFace(int color, Matrix transform) {            
            RotateAngle = 0;
            Index = color;
            Transform = transform;            
            CenterCubie = new CubieFace(this, color, Matrix.Identity, transform, 0);
            Cubies = new CubieFace[8];
            for (int i = 0; i < Cubies.Length; i++) {
                Cubies[i] = new CubieFace(this, color, CubieTranslations[i], transform, i + 1);
            }
        }

        public void SetAllColors(int color) {
            CenterCubie.ColorIndex = color;
            foreach (var cubie in Cubies) cubie.ColorIndex = color;
        }

        public Vector3 GetNorthVector() {
            var result = Vector3.TransformNormal(Vector3.Up, Transform);
            return result;            
        }


        /// <summary>
        /// Rotates the cubie list by the given distance.
        /// Positive distances are considered clockwise by convention, negative for anticlockwise.
        /// </summary>
        /// <param name="cubies"></param>
        /// <param name="distance"></param>
        private static void RotateCubieFaceColors(CubieFace[] cubies, int distance) {
            int[] rotatedColors = new int[cubies.Length];
            Color[] rotatedActuals = new Color[cubies.Length];
            for (int i = 0; i < cubies.Length; i++) {
                int src = WrapMod(i - distance, cubies.Length);
                rotatedActuals[i] = cubies[src].ActualColor;
                rotatedColors[i] = cubies[src].ColorIndex;
            }
            for (int i = 0; i < cubies.Length; i++) {
                cubies[i].ActualColor = rotatedActuals[i];  // hack, must be first because vertexes only regenerated on change of colorindex.
                cubies[i].ColorIndex = rotatedColors[i];
            }
        }

        public void RotateFace(bool clockwise) {
            RotateCubieFaceColors(Cubies, clockwise ? 2 : -2);
            RotateCubieFaceColors(LinkedCubies, clockwise ? 3 : -3);
        }

        private static int WrapMod(int value, int modulo) {
            return (value % modulo) + ((value < 0) ? modulo : 0);
        }

        public void Draw(BasicEffect effect) {
            // draw face in the XY plane, centred on the origin, facing forward (towards negative z), with edge length 3
            CenterCubie.Draw(effect);            
            for (int i = 0; i < Cubies.Length; i++) {                
                Cubies[i].Draw(effect);
            }                              
        }

        private static Matrix[] CubieTranslations = new Matrix[] {
            Matrix.CreateTranslation(-1,1, 0),
            Matrix.CreateTranslation(0, 1, 0),
            Matrix.CreateTranslation(1,1, 0),
            Matrix.CreateTranslation(1,0, 0),
            Matrix.CreateTranslation(1,-1, 0),
            Matrix.CreateTranslation(0,-1, 0),
            Matrix.CreateTranslation(-1,-1, 0),
            Matrix.CreateTranslation(-1,0, 0),
        };
    }
}
