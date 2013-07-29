using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RubiksCube {

    public struct VertexPositionNormalColor : IVertexType {

        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Color Color;

        public VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color) {
            Position = position;
            Normal = normal;
            Color = color;
        }

        public VertexPositionNormalColor(Vector3 position, Color color) {
            Position = position;
            Color = color;
            Normal = Vector3.Zero;
        }

        public VertexPositionNormalColor(VertexPositionNormalColor vertex, Vector3 normal) {
            Position = vertex.Position;
            Color = vertex.Color;
            Normal = normal;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
           new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
           new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
           new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration {
            get { return VertexDeclaration; }
        }
    }
}
