﻿using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.ECS.Components.Renderables._3D
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorNormal : IVertexType
    {
        public Vector3 position;
        public Color color;
        public Vector3 normal;


        private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;


        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
        {
            this.position = position;
            this.color = color;
            this.normal = normal;
        }
    }
}