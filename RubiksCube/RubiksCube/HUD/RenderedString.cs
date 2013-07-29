using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube.HUD {
    public class RenderedString {

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color Backgorund { get; private set; }
        public Color Foreground { get; private set; }

        public Color[] Data { get; private set; }

        public RenderedString(GraphicsDevice device, SpriteFont font, Color background, Color foreground, string text) {
            var size = font.MeasureString(text);
            Width = (int)size.X;
            Height = (int)size.Y;
            Backgorund = background;
            Foreground = foreground;
            using (var target = new RenderTarget2D(device, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None))
            using (var batch = new SpriteBatch(device)) {
                device.SetRenderTarget(target);
                device.Clear(background);
                batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                batch.DrawString(font, text, Vector2.Zero, foreground);
                batch.End();
                device.SetRenderTarget(null);
                Data = new Color[Width * Height];
                target.GetData<Color>(Data);
            }
        }
    }
}
