using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube.HUD {

    public class Button : Control {

        public string Text { get; set; }
        public SpriteFont Font { get; set; }
        public Color ForegroundColor { get; set; }

        public Button(GraphicsDevice device, string text, SpriteFont font, Color foregroundColor) : base(device, 0, 0) {
            BackgroundColor = Color.MediumPurple;
            var measure = font.MeasureString(text);
            Width = (int)measure.X + 16;
            Height = (int)measure.Y + 8;
            Text = text;
            ForegroundColor = foregroundColor;
            Font = font;
        }
        
        protected override void PaintInternal(SpriteBatch batch, Vector2 topleft) {
            base.PaintInternal(batch, topleft);
            if (Text != null) {
                batch.DrawString(Font, Text, topleft + new Vector2(4, 4), ForegroundColor);  
            }
        }
    }
}
