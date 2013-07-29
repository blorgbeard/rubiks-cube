using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube {

    public class Cube {
        private const float GESTURE_THRESHOLD = 0.3f;        

        private class MoveAnimation {
            public int Face;
            public bool Clockwise;
            public double StartTimeSeconds;
            public double DurationSeconds;
            public Matrix Transform;
            public float FinalAngle;
        }

        private Queue<MoveAnimation> MoveQueue = new Queue<MoveAnimation>();

        private bool Broken = false;
        
        public const int FRONT = 0;
        public const int BACK = 1;
        public const int LEFT = 2;
        public const int RIGHT = 3;
        public const int UP = 4;
        public const int DOWN = 5;

        private CubeFace[] Faces;

        private CubieFace MouseMoveStartCubie;
        public Vector3 MouseMoveStartPoint;             // public temporarily for debug
        public Vector3 MouseMoveEndPoint;               // ..

        private readonly static Matrix FrontFaceBaseTransform = Matrix.CreateTranslation(0, 0, 1.5f);
        private readonly static Matrix[] FaceBaseTransforms = new[] {            
            FrontFaceBaseTransform,
            FrontFaceBaseTransform * Matrix.CreateRotationY(MathHelper.Pi),                        
            FrontFaceBaseTransform * Matrix.CreateRotationY(-MathHelper.PiOver2),            
            FrontFaceBaseTransform * Matrix.CreateRotationY(MathHelper.PiOver2),                                    
            FrontFaceBaseTransform * Matrix.CreateRotationX(-MathHelper.PiOver2),
            FrontFaceBaseTransform * Matrix.CreateRotationX(MathHelper.PiOver2),  
        };

        public void FuckThis() {
            //if (Broken) return;            
            var rnd = new Random();
            foreach (var face in Faces) {
                face.FuckThis(rnd);
            }
            Broken = true;
        }

        public Cube() {

            Faces = new CubeFace[6];

            Faces[FRONT] = new CubeFace(FRONT, FaceBaseTransforms[FRONT]);
            Faces[BACK] = new CubeFace(BACK, FaceBaseTransforms[BACK]);
            Faces[LEFT] = new CubeFace(LEFT, FaceBaseTransforms[LEFT]);
            Faces[RIGHT] = new CubeFace(RIGHT, FaceBaseTransforms[RIGHT]);
            Faces[UP] = new CubeFace(UP, FaceBaseTransforms[UP]);
            Faces[DOWN] = new CubeFace(DOWN, FaceBaseTransforms[DOWN]);

            var Front = Faces[FRONT];
            var Back = Faces[BACK];
            var Left = Faces[LEFT];
            var Right = Faces[RIGHT];
            var Up = Faces[UP];
            var Down = Faces[DOWN];


            /*
             * Every face's internal cubie-list looks like this when it is facing the camera:
             *
             *    |||
             *   -012-
             *   -7 3-
             *   -654-
             *    |||
             *    
             * The numbers are the cubie-face indexes. The lines are links to other cubies.             
             * Here we link the faces together. Only the order matters (around clockwise),
             * but we will start from the left-link on cubie-zero.
             * I derived these numbers by making a paper cube and labeling the cubies as above.
             * 
             */

            Front.SetLinkedCubies(
                Left[2], Up[6], Up[5], Up[4], Right[0], Right[7],
                Right[6], Down[2], Down[1], Down[0], Left[4], Left[3]
            );

            Up.SetLinkedCubies(
                Left[0], Back[2], Back[1], Back[0], Right[2], Right[1], Right[0],
                Front[2], Front[1], Front[0], Left[2], Left[1]
            );

            Back.SetLinkedCubies(
                Right[2], Up[2], Up[1], Up[0], Left[0], Left[7], Left[6],
                Down[6], Down[5], Down[4], Right[4], Right[3]
            );

            Down.SetLinkedCubies(
                Left[4], Front[6], Front[5], Front[4], Right[6], Right[5], Right[4],
                Back[6], Back[5], Back[4], Left[6], Left[5]
            );

            Left.SetLinkedCubies(
                Back[2], Up[0], Up[7], Up[6], Front[0], Front[7], Front[6],
                Down[0], Down[7], Down[6], Back[4], Back[3]
            );

            Right.SetLinkedCubies(
                Front[2], Up[4], Up[3], Up[2], Back[0], Back[7], Back[6],
                Down[4], Down[3], Down[2], Front[4], Front[3]
            );

        }

        public void Move(int face, bool clockwise) {
            if (Broken) return;
            Move(face, clockwise, 0.25f);
        }

        public void Move(int face, bool clockwise, float animationDurationSeconds) {
            MoveQueue.Enqueue(new MoveAnimation() {
                Face = face, Clockwise = clockwise,
                Transform = Matrix.Identity,
                StartTimeSeconds = -1,
                DurationSeconds = animationDurationSeconds
            });
        }

        private VertexPositionColorTexture[] HintLineList = new VertexPositionColorTexture[0];

        public bool BeginMouseGesture(Ray mouseRay) {
            foreach (var face in Faces) {
                for (int i = 0; i < 8; i++) {
                    var cubie = face[i];
                    var intersectionDistance = cubie.Intersects(mouseRay);
                    if (intersectionDistance != null) {
                        Console.WriteLine("Intersected cubie #{0} on face #{1}", cubie.Index, face.Index);
                        MouseMoveStartCubie = cubie;
                        MouseMoveStartPoint = mouseRay.Position + (mouseRay.Direction * intersectionDistance.Value);
                        // find possible moves
                        var moves = Gestures.Where(t => t.Cubie == cubie.Index - 1);
                        var lines = new List<VertexPositionColorTexture>();
                        Vector3 north = cubie.ParentFace.GetNorthVector();    // returns normalized vector
                        foreach (var move in moves) {
                            // add a line from intersection point on cubie in compass direction of move on the plane of the cubie
                            var direction = Vector3.TransformNormal(north, Matrix.CreateFromAxisAngle(cubie.GetFacePlane().Normal, move.CompassPoint * -MathHelper.PiOver4));
                            lines.Add(new VertexPositionColorTexture(MouseMoveStartPoint, Color.Black, Vector2.Zero));
                            lines.Add(new VertexPositionColorTexture(MouseMoveStartPoint + direction, Color.Black, Vector2.Zero));
                        }
                        HintLineList = lines.ToArray();
                        return true;
                    }
                }
            }
            return false;
        }

        // todo: this needs refactoring. atm, both CubeGame and Cube maintain some "is dragging" state.
        // move it all into one or the other (Cube, I guess).
        public void UpdateMouseGesture(Ray mouseRay) {
            var hit = mouseRay.Intersects(MouseMoveStartCubie.GetFacePlane());
            if (hit != null) {
                MouseMoveEndPoint = mouseRay.Position + (mouseRay.Direction * hit.Value);
            }
            var moved = MouseMoveEndPoint - MouseMoveStartPoint;
            CubeFace hinted = null;
            MoveGesture move = null;
            if (moved.Length() > GESTURE_THRESHOLD) {
                int compass, cubie;
                CubeFace face;
                GetMouseGesture(moved, out compass, out cubie, out face);
                move = Gestures.FirstOrDefault(t => t.CompassPoint == compass && t.Cubie == cubie);
                if (move != null) {
                    if (move.LinkedFaceIndex == -1) {
                        hinted = face;
                    }
                    else {
                        hinted = face.GetLinkedFace(cubie, move.LinkedFaceIndex);
                    }
                }
                else {
                    // no move found. try projecting the gesture onto this cubies linked faces
                    // and see if there are moves available on those.

                }
            }
            foreach (var f in Faces) {
                f.ClearRotationForHint();
            }
            if (hinted != null && move != null) {
                hinted.SetRotationForHint(move.Clockwise);
            }
        }

        private class MoveGesture {
            public int CompassPoint { get; private set; }
            public int Cubie { get; private set; }            
            /// <summary>
            /// The index of the cubie's linked face to rotate. 
            /// A cubie can be linked to one or two faces, so this can be 0 or 1 to indicate a linked face.
            /// -1 indicates the current face, not a linked face.
            /// </summary>
            public int LinkedFaceIndex { get; private set; }
            public bool Clockwise { get; set; }

            public MoveGesture(int compassPoint, int cubie, int linkedfaceindex, bool clockwise) {
                CompassPoint = compassPoint;
                Cubie = cubie;
                LinkedFaceIndex = linkedfaceindex;
                Clockwise = clockwise;
            }
        }

        /// <summary>
        /// List of pre-defined moves. I feel like there should be a better way to define these.
        /// I probably need to find a better model for the cube, so that I can define these more generically/logically.
        /// But for now, these are all hard-coded.
        /// </summary>
        private static readonly MoveGesture[] Gestures = new MoveGesture[] {
            new MoveGesture(0,0,0,false),   // pulling up on cubie #0 rotates left face
            new MoveGesture(0,7,0,false),   // pulling up on cubie #7 rotates left face
            new MoveGesture(0,6,1,false),   // pulling up on cubie #6 rotates left face
            new MoveGesture(0,2,1,true),    // pulling up on cubie #2 rotates right face
            new MoveGesture(0,3,0,true),    // pulling up on cubie #3 rotates right face
            new MoveGesture(0,4,0,true),    // pulling up on cubie #4 rotates right face            
            new MoveGesture(1,2,-1,false),  // pulling up-left on cubie #2 rotates current face
            new MoveGesture(1,6,-1,true),  // pulling up-left on cubie #6 rotates current face
            new MoveGesture(2,0,1,true),   // pulling left on cubie #0 rotates up face
            new MoveGesture(2,1,0,true),   // pulling left on cubie #1 rotates up face
            new MoveGesture(2,2,0,true),   // pulling left on cubie #2 rotates up face
            new MoveGesture(2,4,1,false),   // pulling left on cubie #4 rotates down face
            new MoveGesture(2,5,0,false),   // pulling left on cubie #5 rotates down face
            new MoveGesture(2,6,0,false),   // pulling left on cubie #6 rotates down face
            new MoveGesture(3,0,-1,false),  // pulling down-left on cubie #0 rotates current face
            new MoveGesture(3,4,-1,true),  // pulling down-left on cubie #4 rotates current face
            new MoveGesture(4,0,0, true),  // pulling down on cubie #0 rotates left face
            new MoveGesture(4,7,0, true),  // pulling down on cubie #7 rotates left face
            new MoveGesture(4,6,1, true),  // pulling down on cubie #6 rotates left face
            new MoveGesture(4,2,1, false),  // pulling down on cubie #2 rotates right face
            new MoveGesture(4,3,0, false),  // pulling down on cubie #3 rotates right face
            new MoveGesture(4,4,0, false),  // pulling down on cubie #4 rotates right face
            new MoveGesture(5,2,-1,true),   // pulling down-right on cubie #2 rotates current face
            new MoveGesture(5,6,-1,false),  // pulling down-right on cubie #6 rotates current face
            new MoveGesture(6,0,1,false),   // pulling right on cubie #0 rotates up face
            new MoveGesture(6,1,0,false),   // pulling right on cubie #1 rotates up face
            new MoveGesture(6,2,0,false),   // pulling right on cubie #2 rotates up face
            new MoveGesture(6,4,1,true),   // pulling right on cubie #4 rotates down face
            new MoveGesture(6,5,0,true),   // pulling right on cubie #5 rotates down face
            new MoveGesture(6,6,0,true),   // pulling right on cubie #6 rotates down face
            new MoveGesture(7,0,-1,true),  // pulling right-up on cubie #0 rotates current face
            new MoveGesture(7,4,-1,false),  // pulling right-up on cubie #4 rotates current face                        
        };

        private void GetMouseGesture(Vector3 moved, out int compass, out int cubie, out CubeFace face) {
            
            // We are interested in the angle of the line on the cubie plane
            // we need the "north" vector from the cubie face - to find the angle between it and "moved" vector.
            Vector3 north = MouseMoveStartCubie.ParentFace.GetNorthVector();    // returns normalized vector

            // Now find the angle between the two
            // http://stackoverflow.com/questions/5188561/signed-angle-between-two-3d-vectors-with-same-origin-within-the-same-plane-reci

            moved.Normalize();
            Vector3 cross = Vector3.Cross(moved, north);
            float sina = cross.Length();
            float cosa = Vector3.Dot(moved, north);

            float theta = (float)Math.Atan2(sina, cosa);

            var sign = Vector3.Dot(MouseMoveStartCubie.GetFacePlane().Normal, cross);

            if (sign < 0) {
                theta = MathHelper.TwoPi - theta;
            }

            // ok, now we have theta!
            compass = (int)Math.Round(theta / MathHelper.PiOver4) % 8;
            cubie = MouseMoveStartCubie.Index - 1;
            face = MouseMoveStartCubie.ParentFace;
        }

        public bool EndMouseGesture() {
            var moved = MouseMoveEndPoint - MouseMoveStartPoint;
            if (moved.Length() > GESTURE_THRESHOLD) {                
                int compass, cubie;
                CubeFace face;
                GetMouseGesture(moved, out compass, out cubie, out face);                                
                var move = Gestures.FirstOrDefault(t => t.CompassPoint == compass && t.Cubie == cubie);                
                if (move != null) {
                    if (move.LinkedFaceIndex == -1) {
                        Move(face.Index, move.Clockwise);
                    }
                    else {
                        Move(face.GetLinkedFace(cubie, move.LinkedFaceIndex).Index, move.Clockwise);
                    }
                }                
                return true;
            }
            return false;
        }

        public void Debug_HighlightIntersectedCubies(Ray ray) {
            foreach (var face in Faces) {
                for (int i = 0; i < 8; i++) {
                    var cubie = face[i];
                    if (cubie.Intersects(ray) != null) {
                        cubie.ActualColor = Color.Violet;
                        cubie.GenerateMeshWithAnimation();
                    }
                }
            }
        }

        public void Update(GameTime time) {
            if (Broken) {
            	foreach (var face in Faces) {
                    face.AnimateExplosion((float)time.ElapsedGameTime.TotalSeconds);
                }
                return;
            }            
            var move = MoveQueue.FirstOrDefault();
            if (move != null) {
                if (move.StartTimeSeconds <= 0) {
                    // move begins
                    move.StartTimeSeconds = time.TotalGameTime.TotalSeconds;
                    move.FinalAngle = move.Clockwise ? -MathHelper.PiOver2 : MathHelper.PiOver2;    // this could be set in the Clockwise setter?
                    if (Faces[move.Face].RotateAngle != 0) {
                    	// we are hinting.. set starttimeseconds back in time - 
                        // pretend we are at the point in the animation where the angle would be what it currently is.
                        // remove (current angle / final angle) % of the total duration.
                        move.StartTimeSeconds -= Faces[move.Face].RotateAngle / (move.FinalAngle /* - move.StartAngle*/) * move.DurationSeconds;
                    }
                }
                else if (move.StartTimeSeconds + move.DurationSeconds < time.TotalGameTime.TotalSeconds) {
                    // move complete
                    Faces[move.Face].RotateFace(move.Clockwise);
                    Faces[move.Face].SetRotation(0);
                    MoveQueue.Dequeue();
                }
                else {
                    // move in progress
                    var progress = (time.TotalGameTime.TotalSeconds - move.StartTimeSeconds) / move.DurationSeconds;
                    Faces[move.Face].SetRotation(move.FinalAngle * (float)progress);
                }
            }
        }

        public void Draw(BasicEffect effect) {
            foreach (var face in Faces) { 
                face.Draw(effect); 
            }
            if (HintLineList.Length > 0) {
            //    effect.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.LineList, HintLineList, 0, HintLineList.Length / 2);
            }
        }
    }
}
