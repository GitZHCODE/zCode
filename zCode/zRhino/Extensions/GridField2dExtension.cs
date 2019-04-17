﻿
/*
 * Notes
 */

#if USING_RHINO

using System;
using System.Collections.Generic;
using System.Drawing;

using Rhino.Geometry;
using zCode.zCore;
using zCode.zField;

namespace zCode.zRhino
{
    /// <summary>
    /// 
    /// </summary>
    public static class GridField2dExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static Mesh MeshSelection(this Grid2d field, IEnumerable<int> selection)
        {
            var mesh = new Mesh();
            var verts = mesh.Vertices;
            var faces = mesh.Faces;

            (var dx, var dy) = (field.Scale * 0.5);

            // add vertices
            foreach (int index in selection)
            {
                var p = field.CoordinateAt(index);
                verts.Add(p.X - dx, p.Y - dy, 0.0);
                verts.Add(p.X + dx, p.Y - dy, 0.0);
                verts.Add(p.X - dx, p.Y + dy, 0.0);
                verts.Add(p.X + dx, p.Y + dy, 0.0);
            }

            // add faces
            for (int i = 0; i < verts.Count; i += 4)
                faces.AddFace(i, i + 1, i + 3, i + 2);

            return mesh;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="getColor"></param>
        /// <returns></returns>
        public static Mesh ToMesh<T>(this GridField2d<T> field, Func<T, Color> getColor)
            where T : struct
        {
            return ToMesh(field, getColor, t => 0.0);
        }


        /// <summary>
        /// Converts the field to a colored heightfield mesh.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="getColor"></param>
        /// <param name="getHeight"></param>
        /// <returns></returns>
        public static Mesh ToMesh<T>(this GridField2d<T> field, Func<T, Color> getColor, Func<T, double> getHeight)
            where T : struct
        {
            Mesh mesh = new Mesh();
            var verts = mesh.Vertices;
            var colors = mesh.VertexColors;
            var faces = mesh.Faces;

            var values = field.Values;
            int index = 0;

            for (int j = 0; j < field.CountY; j++)
            {
                for (int i = 0; i < field.CountX; i++)
                {
                    var t = values[index++];

                    Vec2d p = field.CoordinateAt(i, j);
                    verts.Add(p.X, p.Y, getHeight(t));
                    colors.Add(getColor(t));
                }
            }

            int nx = field.CountX;
            int ny = field.CountY;

            for (int j = 0; j < ny - 1; j++)
            {
                for (int i = 0; i < nx - 1; i++)
                {
                    index = field.IndexAtUnsafe(i, j);
                    faces.AddFace(index, index + 1, index + 1 + nx, index + nx);
                }
            }

            return mesh;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="getColor"></param>
        /// <returns></returns>
        public static Mesh ToPolySoup<T>(this GridField2d<T> field, Func<T, Color> getColor)
            where T : struct
        {
            Mesh mesh = new Mesh();
            var verts = mesh.Vertices;
            var colors = mesh.VertexColors;
            var faces = mesh.Faces;

            var values = field.Values;
            (var dx, var dy) = (field.Scale * 0.5);

            int index = 0;

            // add vertices
            for (int i = 0; i < field.CountY; i++)
            {
                for (int j = 0; j < field.CountX; j++)
                {
                    Vec2d p = field.CoordinateAt(j, i);
                    verts.Add(p.X - dx, p.Y - dy, 0.0);
                    verts.Add(p.X + dx, p.Y - dy, 0.0);
                    verts.Add(p.X - dx, p.Y + dy, 0.0);
                    verts.Add(p.X + dx, p.Y + dy, 0.0);

                    var c = getColor(values[index++]);
                    for (int k = 0; k < 4; k++) colors.Add(c);
                }
            }

            // add faces
            for (int i = 0; i < verts.Count; i += 4)
                faces.AddFace(i, i + 1, i + 3, i + 2);

            return mesh;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="getColor"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static Mesh MeshSelection<T>(this GridField2d<T> field, Func<T, Color> getColor, IEnumerable<int> selection)
            where T : struct
        {
            var mesh = new Mesh();
            var verts = mesh.Vertices;
            var colors = mesh.VertexColors;
            var faces = mesh.Faces;

            var values = field.Values;
            (var dx, var dy) = (field.Scale * 0.5);

            // add vertices
            foreach (int index in selection)
            {
                var p = field.CoordinateAt(index);

                verts.Add(p.X - dx, p.Y - dy, 0.0);
                verts.Add(p.X + dx, p.Y - dy, 0.0);
                verts.Add(p.X - dx, p.Y + dy, 0.0);
                verts.Add(p.X + dx, p.Y + dy, 0.0);

                var c = getColor(values[index]);
                for (int k = 0; k < 4; k++) colors.Add(c);
            }

            // add faces
            for (int i = 0; i < verts.Count; i += 4)
                faces.AddFace(i, i + 1, i + 3, i + 2);

            return mesh;
        }
    }
}

#endif