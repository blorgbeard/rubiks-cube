using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube.HUD {
    public class TexturePainter {

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color[] Data { get; private set; }

        public TexturePainter(int width, int height) {
            Width = width;
            Height = height;
            Data = new Color[width * height];
        }

        public void Clear(Color color) {
            for (int i = 0; i < Data.Length; i++) {
                Data[i] = color;
            }
        }

        public void Border(Color color) {
            int bottomRowStart = Width * (Height - 1);
            int widthSub1 = Width - 1;
            for (int i = 0; i < Width || i < Height; i++) {
                if (i < Width) {
                    Data[i] = color;
                    Data[bottomRowStart + i] = color;
                }                
                if (i < Height) {
                    int rowStart = i * Width;
                    Data[rowStart] = color;
                    Data[rowStart + widthSub1] = color;
                }
            }
        }

        public void DrawString(RenderedString text, int left, int top) {
            // todo: bounds checking!
            for (int y = 0; y < text.Height; y++) {
                for (int x = 0; x < text.Width; x++) {
                    Data[((top + y) * Width) + left + x] = text.Data[y * text.Width + x];
                }
            }
        }

        public Texture2D CreateTexture(GraphicsDevice device) {
            var result = new Texture2D(device, Width, Height, false, SurfaceFormat.Color);
            result.SetData<Color>(Data);
            return result;
        }
    }
}
