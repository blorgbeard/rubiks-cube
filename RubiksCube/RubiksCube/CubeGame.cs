using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RubiksCube.HUD;

namespace RubiksCube {

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CubeGame : Microsoft.Xna.Framework.Game {

        private Panel _Panel;
        private Vector3 Debug_MouseP1;
        private Vector3 Debug_MouseP2;
        private bool IsMouseDragging = true;
        private Ray MouseRay;

        private readonly GraphicsDeviceManager Graphics;
        private GraphicsDevice Device;
        private SpriteBatch spriteBatch;
        private SpriteFont Font;
        private BasicEffect Effect;

        private BasicCamera Camera;
        private ArcBall ArcBall;
        
        private Cube TheCube;

        public CubeGame() {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = 1024;
            Graphics.PreferredBackBufferHeight = 768;
            Graphics.PreferMultiSampling = true;            
            Content.RootDirectory = "Content";
            TheCube = new Cube();
        }

        private bool CheckKeyPress(KeyboardState kb, Keys key) {
            return !KeysWerePressed.Contains(key) && kb.IsKeyDown(key);
        }
        private Keys[] KeysWerePressed = new Keys[0];

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            Device = Graphics.GraphicsDevice;
            Device.PresentationParameters.MultiSampleCount = 8;
            //Device.PresentationParameters.IsFullScreen = true;
            Graphics.ApplyChanges();
            Device.Reset(Device.PresentationParameters);
            Effect = new BasicEffect(Device) { VertexColorEnabled = true };
            
            //Effect.EnableDefaultLighting();
            IsMouseVisible = true;

            Camera = new BasicCamera(new Vector3(0, 0, 10), Vector3.Zero, Device.Viewport.AspectRatio);
            //ArcBall = new ArcBall(Device, Camera) { Rotation = Matrix.CreateRotationY(-MathHelper.PiOver4) };
            ArcBall = new ArcBall(Device, Camera) { Rotation = Matrix.CreateRotationY(-MathHelper.PiOver4) * Matrix.CreateRotationX(MathHelper.PiOver4 / 3 * 2)};

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Fonts/DefaultUI");
            var texture = Content.Load<Texture2D>("Textures/PlainCubie");
            Effect.Texture = texture;
            Effect.TextureEnabled = true;
            Effect.View = Camera.ViewMatrix;
            Effect.Projection = Camera.ProjectionMatrix;
            UpdateWorldTransform();


            _Panel = new Panel(Device, Device.Viewport.Width, 44);
            _Panel.Controls.Add(new Button(Device, "Scramble", Font, Color.Black));
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            KeyboardState kb = Keyboard.GetState();
            if (kb.IsKeyDown(Keys.Escape)) this.Exit();
            base.Update(gameTime);
            bool shift = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
            if (CheckKeyPress(kb, Keys.F)) TheCube.Move(Cube.FRONT, !shift);
            if (CheckKeyPress(kb, Keys.B)) TheCube.Move(Cube.BACK, !shift);
            if (CheckKeyPress(kb, Keys.L)) TheCube.Move(Cube.LEFT, !shift);
            if (CheckKeyPress(kb, Keys.R)) TheCube.Move(Cube.RIGHT, !shift);
            if (CheckKeyPress(kb, Keys.U)) TheCube.Move(Cube.UP, !shift);
            if (CheckKeyPress(kb, Keys.D)) TheCube.Move(Cube.DOWN, !shift);

            if (kb.GetPressedKeys().Length > 4) {
            	TheCube.FuckThis();
            }
            KeysWerePressed = kb.GetPressedKeys();

            DoMouseGestures();
            
            var mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed) {
                if (!buttonClickProcessed) {
                    var ctrl = _Panel.GetControlAt(mouse.X, mouse.Y - Device.Viewport.Height + _Panel.Height);
                    if (ctrl != null) {
                        var rand = new Random();
                        for (int i = 0; i < 20; i++) {
                            int x = rand.Next(6);
                            TheCube.Move(x, true, 0.07f);
                        }
                    }
                    buttonClickProcessed = true;
                }
            }
            else {
                buttonClickProcessed = false;
            }

            TheCube.Update(gameTime);
            DoMouseLook();
        }

        private bool buttonClickProcessed = false;

        private void DoMouseGestures() {

            var mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed) {
                Vector3 nearsource = new Vector3((float)mouse.X, (float)mouse.Y, 0f);
                Vector3 farsource = new Vector3((float)mouse.X, (float)mouse.Y, 1f);

                Matrix world = Effect.World;

                var NearPoint = GraphicsDevice.Viewport.Unproject(nearsource, Camera.ProjectionMatrix, Camera.ViewMatrix, world);
                var FarPoint = GraphicsDevice.Viewport.Unproject(farsource, Camera.ProjectionMatrix, Camera.ViewMatrix, world);

                // Create a ray from the near clip plane to the far clip plane.
                Vector3 direction = FarPoint - NearPoint;
                direction.Normalize();
                MouseRay = new Ray(NearPoint, direction);

                if (IsMouseDragging) {
                    TheCube.UpdateMouseGesture(MouseRay);
                    Debug_MouseP2 = TheCube.MouseMoveEndPoint;
                    //IsMouseDragging = !TheCube.EndMouseGesture();
                }
                else {
                    //TheCube.Debug_HighlightIntersectedCubies(MouseRay);
                    if (TheCube.BeginMouseGesture(MouseRay)) {
                        Debug_MouseP1 = TheCube.MouseMoveStartPoint;
                        IsMouseDragging = true;
                    }
                }
            }
            else {
                if (IsMouseDragging) {
                    TheCube.EndMouseGesture();
                    IsMouseDragging = false;
                }
            }
        }

        private void DoMouseLook() {
            var mouse = Mouse.GetState();
            if (mouse.RightButton == ButtonState.Pressed) {
                if (!ArcBall.IsDragging) {
                    ArcBall.StartDrag(mouse);
                }
                else if (ArcBall.IsDragging) {
                    ArcBall.UpdateDrag(mouse);
                    UpdateWorldTransform();
                }  
            }
            else {
                if (ArcBall.IsDragging) {
                    ArcBall.EndDrag();
                }
            }
        }

        private void UpdateWorldTransform() {            
            Effect.World = ArcBall.Rotation;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // TODO: Add your drawing code here
            foreach (var pass in Effect.CurrentTechnique.Passes) {
                Effect.TextureEnabled = true;
                pass.Apply();
                
                // commented stuff is an attempt to render mirrored cube
                // it works, but it screws up the ArcBall and mouse gestures.

                //var save = Effect.World;
                //Effect.World = save * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(-2.5f, 0, 0);
                //pass.Apply();
                //TheCube.Draw(Effect);
                //Effect.World = save * Matrix.CreateTranslation(2.5f, 0, 0);
                //pass.Apply();
                TheCube.Draw(Effect);
                //Effect.World = save;
                Effect.TextureEnabled = false;
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList,
                    new[] { 
                        new VertexPositionColor(Debug_MouseP2, Color.Violet),
                        new VertexPositionColor(Debug_MouseP1 , Color.Black),
                    }, 0, 1
                );
            }

            spriteBatch.Begin();

            _Panel.Paint(spriteBatch, new Vector2(0, Device.Viewport.Height - _Panel.Height));

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
