using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube {

    public class CubieFace {

        private VertexPositionColorTexture[] TriangleList;
        private readonly Vector3[] TriangleList_NoAnimation;                      // used for collision-detection for move gestures, which should not take into account the spin caused by move-hints.
        private VertexPositionColorTexture[] TriangleList_Inside;

        private readonly static Dictionary<int, Color> ActualColors = new Dictionary<int, Color>() { 
            { Cube.UP, Color.Yellow }, 
            { Cube.DOWN, Color.White }, 
            { Cube.FRONT, Color.Blue }, 
            { Cube.BACK, Color.SeaGreen}, 
            { Cube.RIGHT, Color.Red }, 
            { Cube.LEFT, Color.Orange } 
        };

        public CubeFace ParentFace { get; private set; }

        private Matrix Transform_CubieOnFace;  // transform from canonical cubie at (0,0,0) to the appropriate position on a cube face
        private Matrix Transform_FaceOnCube;   // transform from the face of which this cubie is a part onto the cube in the appropriate position and orientation.  

        private readonly Matrix Transform_CubieOnCube;  // transform from canonical cubie at (0,0,0) to the appropriate position on this cubie's cube's face on the cube. No animation is added.
        private Matrix Transform_WithAnimation;         // transform from canonical cubie at (0,0,0) to the appropriate position and orientation for this cube face, with any animation (rotation, spin) included.

        private void CalculateTransforms() {
            Transform_WithAnimation = Transform_CubieOnFace * _RotateTransform * Transform_FaceOnCube * _SpinTransform;
        }

        public Matrix RotateTransform {
            get {
                return _RotateTransform;
            }
            set {
                _RotateTransform = value;
                CalculateTransforms();
                GenerateMeshWithAnimation();
            }
        }
        private Matrix _RotateTransform = Matrix.Identity;

        private bool Exploded = false;
        private Vector3 Explode_YPR_Velocity;
        private Vector3 Explode_Velocity;

        private static float RandomAngularVelocity(Random rnd) {
            return (MathHelper.PiOver4 + (float)rnd.NextDouble() * MathHelper.Pi) * (rnd.Next(1) == 0 ? -1 : 1);
        }

        public void BeginExplosion(Random rnd) {
            if (!Exploded) {
                Explode_YPR_Velocity = new Vector3(RandomAngularVelocity(rnd), RandomAngularVelocity(rnd), RandomAngularVelocity(rnd));
                Explode_Velocity = new Vector3((float)rnd.NextDouble() * 50f, (float)rnd.NextDouble() * 50f, (float)rnd.NextDouble() * 50f);
                Explode_Velocity += new Vector3(-45, 5, -45);
                Exploded = true;
            }
        }

        public void Explode(float time) {
            if (!Exploded) return;
            Transform_FaceOnCube *= Matrix.CreateTranslation(Explode_Velocity * time);
            Transform_CubieOnFace *= Matrix.CreateFromYawPitchRoll(Explode_YPR_Velocity.X * time, Explode_YPR_Velocity.Y * time, Explode_YPR_Velocity.Z * time);            

            CalculateTransforms();
            GenerateMeshWithAnimation();
            
            //if (TriangleList[0].Position.Y < 0) {
            //    // bounce!
            //    Explode_Velocity.Y *= -1;
            //}
            //else {
                Explode_Velocity.Y -= 30f * time;    // grabbity
            //}
        }

        /// <summary>
        /// The transform applied when rotating cubie faces which are attached to the rotating face.
        /// They spin around the rotating face's normal vector, you see.
        /// Should be set to Identity when not animating.
        /// </summary>
        public Matrix SpinTransform {
            get {
                return _SpinTransform;
            }
            set {
                _SpinTransform = value;
                CalculateTransforms();
                GenerateMeshWithAnimation();
            }
        }
        private Matrix _SpinTransform = Matrix.Identity;

        // these are temp debug variables
        public Color ActualColor;        
        // ---- 

        public int Index;   // ughhhhhh

        public Plane GetFacePlane() {
            // todo: cache this?            
            return new Plane(TriangleList_NoAnimation[0], TriangleList_NoAnimation[1], TriangleList_NoAnimation[2]);
        }
        
        private static bool RayIntersectTriangle(Vector3 rayPosition, Vector3 rayDirection, Vector3 tri0, Vector3 tri1, Vector3 tri2, ref float pickDistance, ref float barycentricU, ref float barycentricV) {
            
            // http://dzindzinovic.blogspot.com/2010/05/xna-ray-intersect-triangle.html
            
            // Find vectors for two edges sharing vert0
            Vector3 edge1 = tri1 - tri0;
            Vector3 edge2 = tri2 - tri0;

            // Begin calculating determinant - also used to calculate barycentricU parameter
            Vector3 pvec = Vector3.Cross(rayDirection, edge2);

            // If determinant is near zero, ray lies in plane of triangle
            float det = Vector3.Dot(edge1, pvec);
            if (det < 0.0001f)    
                return false;

            // Calculate distance from vert0 to ray origin
            Vector3 tvec = rayPosition - tri0;

            // Calculate barycentricU parameter and test bounds
            barycentricU = Vector3.Dot(tvec, pvec);
            if (barycentricU < 0.0f || barycentricU > det)
                return false;

            // Prepare to test barycentricV parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate barycentricV parameter and test bounds
            barycentricV = Vector3.Dot(rayDirection, qvec);
            if (barycentricV < 0.0f || barycentricU + barycentricV > det)
                return false;

            // Calculate pickDistance, scale parameters, ray intersects triangle
            pickDistance = Vector3.Dot(edge2, qvec);
            float fInvDet = 1.0f / det;
            pickDistance *= fInvDet;
            barycentricU *= fInvDet;
            barycentricV *= fInvDet;

            return true;
        }

        public float? Intersects(Ray ray) {
            float pickDistance = 0f, baryU = 0f, baryV = 0f;
            // depends on order of triangle vertices in order to work
            // I do not understand why, but it currently works.
            if (RayIntersectTriangle(ray.Position, ray.Direction, TriangleList_NoAnimation[0], TriangleList_NoAnimation[2], TriangleList_NoAnimation[1], ref pickDistance, ref baryU, ref baryV)) {
                return pickDistance;
            }
            if (RayIntersectTriangle(ray.Position, ray.Direction, TriangleList_NoAnimation[3], TriangleList_NoAnimation[5], TriangleList_NoAnimation[4], ref pickDistance, ref baryU, ref baryV)) {
                return pickDistance;
            }
            return null;
        }

        /// <summary>
        /// Transform a vector for animation and create a Vertex object with it.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="color"></param>
        /// <param name="TextureX"></param>
        /// <param name="TextureY"></param>
        /// <returns></returns>
        private VertexPositionColorTexture CreateTransformedVertex(Vector3 vector, Color color, int TextureX, int TextureY) {
            Vector3 transformedVector = Vector3.Transform(vector, Transform_WithAnimation);
            return new VertexPositionColorTexture(transformedVector, color, new Vector2(TextureX, TextureY));
        }

        private static readonly Vector3[] UntransformedTriangleList = new[] {
	        new Vector3(-0.5f, +0.5f, 0),
	        new Vector3(+0.5f, -0.5f, 0),
	        new Vector3(-0.5f, -0.5f, 0),
	        new Vector3(-0.5f, +0.5f, 0),
	        new Vector3(+0.5f, +0.5f, 0),
	        new Vector3(+0.5f, -0.5f, 0),
        };

        private static readonly Vector3[] UntransformedTriangleList_Inside = new[] {
	        new Vector3(-0.5f, +0.5f, 0),
	        new Vector3(-0.5f, -0.5f, 0),
	        new Vector3(+0.5f, -0.5f, 0),
	        new Vector3(-0.5f, +0.5f, 0),
	        new Vector3(+0.5f, -0.5f, 0),
	        new Vector3(+0.5f, +0.5f, 0),
        };

        private static readonly Color InsideColor = Color.FromNonPremultiplied(96, 96, 96, 255);

        public void GenerateMeshWithAnimation() {
            TriangleList = new[] {
                CreateTransformedVertex(UntransformedTriangleList[0], ActualColor, 0, 1),
                CreateTransformedVertex(UntransformedTriangleList[1], ActualColor, 1, 0),
                CreateTransformedVertex(UntransformedTriangleList[2], ActualColor, 0, 0),
                CreateTransformedVertex(UntransformedTriangleList[3], ActualColor, 0, 1),
                CreateTransformedVertex(UntransformedTriangleList[4], ActualColor, 1, 1),
                CreateTransformedVertex(UntransformedTriangleList[5], ActualColor, 1, 0),
            };
            TriangleList_Inside = new[] {
                CreateTransformedVertex(UntransformedTriangleList_Inside[0], InsideColor, 0, 0),                
                CreateTransformedVertex(UntransformedTriangleList_Inside[1], InsideColor, 0, 0),
                CreateTransformedVertex(UntransformedTriangleList_Inside[2], InsideColor, 0, 0),
                CreateTransformedVertex(UntransformedTriangleList_Inside[3], InsideColor, 0, 0),
                CreateTransformedVertex(UntransformedTriangleList_Inside[4], InsideColor, 0, 0),
                CreateTransformedVertex(UntransformedTriangleList_Inside[5], InsideColor, 0, 0),                
            };            
        }

        private int _ColorIndex;
        public int ColorIndex {
            get {
                return _ColorIndex;
            }
            set {
                _ColorIndex = value;
                GenerateMeshWithAnimation();
            }
        }
        //public int Rotation { get; set; }   // could implement this, and add textured cubes..

        public CubieFace(CubeFace parent, int color, Matrix transformCubieOntoFace, Matrix transformFaceOntoCube, int index) {
            ParentFace = parent;
            Index = index;
            Transform_CubieOnFace = transformCubieOntoFace;             // don't call setter, don't call GenerateMesh()
            Transform_FaceOnCube = transformFaceOntoCube;
            Transform_CubieOnCube = Transform_CubieOnFace * Transform_FaceOnCube;
            ActualColor = ActualColors[color];          
            //ActualColor = Color.FromNonPremultiplied(Math.Max((byte)128, ActualColor.R) - Index * 15, Math.Max((byte)128, ActualColor.G) - Index * 15, Math.Max((byte)128, ActualColor.B) - Index * 15, ActualColor.A);
            _ColorIndex = color;
            CalculateTransforms();
            GenerateMeshWithAnimation(); // now that everything is set

            TriangleList_NoAnimation = new[] {
                Vector3.Transform(UntransformedTriangleList[0], Transform_CubieOnCube),
                Vector3.Transform(UntransformedTriangleList[1], Transform_CubieOnCube),
                Vector3.Transform(UntransformedTriangleList[2], Transform_CubieOnCube),
                Vector3.Transform(UntransformedTriangleList[3], Transform_CubieOnCube),
                Vector3.Transform(UntransformedTriangleList[4], Transform_CubieOnCube),
                Vector3.Transform(UntransformedTriangleList[5], Transform_CubieOnCube),
            };
        }
        
        public void Draw(Effect effect) {
            // draw square in the XY plane, centred on origin, facing forward (negative z), edge length = 1
            var device = effect.GraphicsDevice;
            device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, TriangleList, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            // now draw an inside face so that the background doesn't show through when rotating faces
            device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, TriangleList_Inside, 0, 2, VertexPositionColorTexture.VertexDeclaration);            
        }

    }
}
