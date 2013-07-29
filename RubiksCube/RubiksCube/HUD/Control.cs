using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube.HUD {

    public class Control {

        protected readonly GraphicsDevice Device;

        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
            
        public int Width { get; set; }
        public int Height { get; set; }

        private Texture2D Texture { get; set; }
        
        public void GenerateTexture() {
            var tp = new TexturePainter(Width, Height);
            GenerateTextureInternal(tp);
            if (Texture != null) Texture.Dispose();
            Texture = tp.CreateTexture(Device);
        }

        protected virtual void GenerateTextureInternal(TexturePainter tp) {
            tp.Clear(BackgroundColor);
            tp.Border(BorderColor);
        }

        public Control(GraphicsDevice device, int width, int height) {
            Device = device;
            BackgroundColor = Color.SlateGray;
            BorderColor = Color.Black;
            Width = width;
            Height = height;            
        }

        public void Paint(SpriteBatch batch, Vector2 topleft) {
            if (Texture == null) {
                GenerateTexture();
            }
            PaintInternal(batch, topleft);
        }

        protected virtual void PaintInternal(SpriteBatch batch, Vector2 topleft) {
            batch.Draw(Texture, topleft, Color.White);
        }
    }
}