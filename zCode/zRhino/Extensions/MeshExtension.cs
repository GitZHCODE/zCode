
/*
 * Notes
 */

#if USING_RHINO

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Drawing;
using System.Linq;

using Rhino.Geometry;
using zCode.zCore;
using zCode.zMesh;

namespace zCode.zRhino
{
    /// <summary>
    /// 
    /// </summary>
    public static class MeshExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static HeGraph3d ToHeGraph(this Mesh mesh)
        {
            return HeGraph3d.Factory.CreateFromVertexTopology(mesh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static HeMesh3d ToHeMesh(this Mesh mesh)
        {
            return HeMesh3d.Factory.CreateFromMesh(mesh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Mesh ToPolySoup(this Mesh mesh)
        {
            return RhinoFactory.Mesh.CreatePolySoup(mesh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="getColor"></param>
        /// <param name="parallel"></param>
        public static void ColorVertices(this Mesh mesh, Func<int, Color> getColor, bool parallel = false)
        {
            var verts = mesh.Vertices;
            var colors = mesh.VertexColors;
            colors.Count = verts.Count;

            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, verts.Count), range => Body(range.Item1, range.Item2));
            else
                Body(0, verts.Count);

            void Body(int from, int to)
            {
                for (int i = from; i < to; i++)
                    colors[i] = getColor(i);
            }
        }


        /// <summary>
        /// Assumes the mesh is polygon soup (i.e. vertices aren't shared between faces).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="getColor"></param>
        /// <param name="parallel"></param>
        public static void ColorFaces<T>(this Mesh mesh, Func<int, Color> getColor, bool parallel = false)
        {
            var faces = mesh.Faces;
            var colors = mesh.VertexColors;
            colors.Count = mesh.Vertices.Count;

            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, faces.Count), range => Body(range.Item1, range.Item2));
            else
                Body(0, faces.Count);

            void Body(int from, int to)
            {
                for (int i = from; i < to; i++)
                {
                    var f = faces[i];
                    Color c = getColor(i);

                    int n = (f.IsQuad) ? 4 : 3;
                    for (int j = 0; j < n; j++) colors[f[j]] = c;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexValues"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Mesh IsoTrim(this Mesh mesh, IReadOnlyList<double> vertexValues, Intervald interval)
        {
            return RhinoFactory.Mesh.CreateIsoTrim(mesh, vertexValues, interval);
        }


        /// <summary>
        /// Returns the entries of the cotangent-weighted Laplacian matrix in column-major order.
        /// Based on symmetric derivation of the Laplace-Beltrami operator detailed in http://www.cs.jhu.edu/~misha/ReadingSeminar/Papers/Vallet08.pdf.
        /// Assumes triangle mesh.
        /// </summary>
        /// <returns></returns>
        public static void GetLaplacianMatrix(this Mesh mesh, double[] result)
        {
            GetLaplacianMatrix(mesh, result, new double[mesh.Vertices.Count]);
        }


        /// <summary>
        /// Return iso-countour lines of a mesh based vertex values
        /// 
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<Line> GetIsoCurve(this Mesh mesh, IReadOnlyList<double> vertexValues, double level)
            {
            //input mesh must be triangulated
            mesh.Faces.ConvertQuadsToTriangles();

            var faces = mesh.Faces;
            var vals = vertexValues;
            List<Line> iso = new List<Line>();

            foreach (MeshFace mf in mesh.Faces)
                {
                Point3d p0 = mesh.Vertices[mf.A];
                Point3d p1 = mesh.Vertices[mf.B];
                Point3d p2 = mesh.Vertices[mf.C];

                double t0 = vertexValues[mf.A];
                double t1 = vertexValues[mf.B];
                double t2 = vertexValues[mf.C];

                int mask = 0;
                if (t0 >= level) { mask |= 1; }
                if (t1 >= level) { mask |= 2; }
                if (t2 >= level) { mask |= 4; }

                switch (mask)
                    {
                    case 0:
                        break;
                    case 1:
                        iso.Add(GetLine(p0, p1, p2, zMath.Normalize(level, t0, t1), zMath.Normalize(level, t0, t2)));
                        break;
                    case 2:
                        iso.Add(GetLine(p1, p2, p0, zMath.Normalize(level, t1, t2), zMath.Normalize(level, t1, t0)));
                        break;
                    case 3:
                        iso.Add(GetLine(p2, p0, p1, zMath.Normalize(level, t2, t0), zMath.Normalize(level, t2, t1)));
                        break;
                    case 4:
                        iso.Add(GetLine(p2, p0, p1, zMath.Normalize(level, t2, t0), zMath.Normalize(level, t2, t1)));
                        break;
                    case 5:
                        iso.Add(GetLine(p1, p2, p0, zMath.Normalize(level, t1, t2), zMath.Normalize(level, t1, t0)));
                        break;
                    case 6:
                        iso.Add(GetLine(p0, p1, p2, zMath.Normalize(level, t0, t1), zMath.Normalize(level, t0, t2)));
                        break;
                    case 7:
                        break;
                    }
                }

            return iso;

            //Return the mesh tri-face isoline 
            Line GetLine(Point3d p0, Point3d p1, Point3d p2, double t01, double t02)
                {
                Point3d p01 = LerpPoint(p0, p1, t01);
                Point3d p02 = LerpPoint(p0, p2, t02);
                return new Line(p01, p02);
                }

            //Linear Interpolation of two Rhino Point3d 
            Point3d LerpPoint(Point3d p0, Point3d p1, double t)
                {
                return p0 + (p1 - p0) * t;
                }

            }


        /// <summary>
        /// Returns the entries of the cotangent-weighted Laplacian matrix in column-major order.
        /// Based on symmetric derivation of the Laplace-Beltrami operator detailed in http://www.cs.jhu.edu/~misha/ReadingSeminar/Papers/Vallet08.pdf.
        /// Also returns the barycentric dual area of each vertex.
        /// Assumes triangle mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="entriesOut"></param>
        /// <param name="areasOut"></param>
        public static void GetLaplacianMatrix(this Mesh mesh, double[] entriesOut, double[] areasOut)
        {
            var verts = mesh.Vertices;
            int n = verts.Count;
            double t = 1.0 / 6.0;

            Array.Clear(entriesOut, 0, n * n);
            Array.Clear(areasOut, 0, n);

            // iterate faces to collect weights and vertex areas (lower triangular only)
            foreach (MeshFace mf in mesh.Faces)
            {
                // circulate verts in face
                for (int i = 0; i < 3; i++)
                {
                    int i0 = mf[i];
                    int i1 = mf[(i + 1) % 3];
                    int i2 = mf[(i + 2) % 3];

                    Vector3d v0 = verts[i0] - verts[i2];
                    Vector3d v1 = verts[i1] - verts[i2];

                    // add to vertex area
                    double a = Vector3d.CrossProduct(v0, v1).Length;
                    areasOut[i0] += a * t;

                    // add to edge cotangent weights (assumes consistent face orientation)
                    if (i1 > i0)
                        entriesOut[i0 * n + i1] += 0.5 * v0 * v1 / a;
                    else
                        entriesOut[i1 * n + i0] += 0.5 * v0 * v1 / a;
                }
            }

            // normalize weights with areas and sum along diagonals
            for (int i = 0; i < n; i++)
            {
                int ii = i * n + i;

                for (int j = i + 1; j < n; j++)
                {
                    double w = entriesOut[i * n + j];
                    w /= Math.Sqrt(areasOut[i] * areasOut[j]);
                    entriesOut[i * n + j] = w;
                    entriesOut[j * n + i] = w;

                    // sum along diagonal entries
                    entriesOut[ii] -= w;
                    entriesOut[j * n + j] -= w;
                }
            }
        }
    }
}

#endif
