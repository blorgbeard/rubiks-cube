using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube.HUD {
    public class Panel : Control {

        public readonly List<Control> Controls = new List<Control>();

        public Panel(GraphicsDevice device, int width, int height) : base(device, width, height) {

        }

        protected override void GenerateTextureInternal(TexturePainter tp) {
            base.GenerateTextureInternal(tp);
            foreach (var control in Controls) control.GenerateTexture();
        }

        protected override void PaintInternal(SpriteBatch batch, Vector2 topleft) {
            base.PaintInternal(batch, topleft);
            foreach (var ctrl in Controls) {
                ctrl.Paint(batch, topleft + new Vector2(4, 4));
                topleft.X += ctrl.Width + 10;
            }
        }

        public Control GetControlAt(int x, int y) {
            x += 4;
            y += 4;
            foreach (var ctrl in Controls) {
                if (x >= 0 && x <= ctrl.Width && y >= 0 && y <= ctrl.Height) {
                	return ctrl;
                }
                x += ctrl.Width + 10;
            }
            return null;
        }
    }
}