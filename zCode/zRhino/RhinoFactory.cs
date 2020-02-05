﻿
/*
 * Notes
 * 
 * Factory class is split up for consistent type aliasing where T always refers to the type created by the factory method.
 */

#if USING_RHINO

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using Rhino.Geometry;
using zCode.zCore;
using zCode.zMesh;

namespace zCode.zRhino
{
    using T = Rhino.Geometry.Mesh;

    /// <summary>
    /// 
    /// </summary>
    public static partial class RhinoFactory
    {
        /// <summary>
        /// Static creation methods for meshes.
        /// </summary>
        public static class Mesh
        {
            /// <summary>
            /// 
            /// </summary>
            public static T CreateExtrusion(Polyline polyline, Vector3d direction)
            {
                if (polyline.IsClosed)
                    return CreateExtrusionClosed(polyline, direction);
                else
                    return CreateExtrusionOpen(polyline, direction);
            }


            /// <summary>
            /// 
            /// </summary>
            private static T CreateExtrusionOpen(Polyline polyline, Vector3d direction)
            {
                T result = new T();
                var verts = result.Vertices;
                var faces = result.Faces;

                int n = polyline.Count;

                // add vertices
                for (int i = 0; i < n; i++)
                    verts.Add(polyline[i]);

                for (int i = 0; i < n; i++)
                    verts.Add(polyline[i] + direction);

                // add faces
                for (int i = 0; i < n - 1; i++)
                    faces.AddFace(i, i + 1, i + n + 1, i + n);

                return result;
            }


            /// <summary>
            /// 
            /// </summary>
            private static T CreateExtrusionClosed(Polyline polyline, Vector3d direction)
            {
                T result = new T();
                var verts = result.Vertices;
                var faces = result.Faces;

                int n = polyline.Count - 1;

                // add verts
                for (int i = 0; i < n; i++)
                    verts.Add(polyline[i]);

                for (int i = 0; i < n; i++)
                    verts.Add(polyline[i] + direction);

                // add faces
                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n;
                    faces.AddFace(i, j, j + n, i + n);
                }

                return result;
            }


            /// <summary>
            /// 
            /// </summary>
            public static T CreateLoft(Polyline polylineA, Polyline polylineB)
            {
                if (polylineA.IsClosed && polylineB.IsClosed)
                    return CreateLoftClosed(polylineA, polylineB);
                else
                    return CreateLoftOpen(polylineA, polylineB);
            }


            /// <summary>
            /// 
            /// </summary>
            private static T CreateLoftOpen(Polyline polylineA, Polyline polylineB)
            {
                T result = new T();
                var verts = result.Vertices;
                var faces = result.Faces;

                int n = polylineA.Count;

                // add vertices
                verts.AddVertices(polylineA);
                verts.AddVertices(polylineB);

                // add faces
                for (int i = 0; i < n - 1; i++)
                    faces.AddFace(i, i + 1, i + n + 1, i + n);

                return result;
            }


            /// <summary>
            /// 
            /// </summary>
            private static T CreateLoftClosed(Polyline polylineA, Polyline polylineB)
            {
                T result = new T();
                var verts = result.Vertices;
                var faces = result.Faces;

                int n = polylineA.Count - 1;

                // add verts
                for (int i = 0; i < n; i++)
                    verts.Add(polylineA[i]);

                for (int i = 0; i < n; i++)
                    verts.Add(polylineB[i]);

                // add faces
                for (int i = 0; i < n; i++)
                {
                    int j = (i + 1) % n;
                    faces.AddFace(i, j, j + n, i + n);
                }

                return result;
            }


            /// <summary>
            /// 
            /// </summary>
            public static T CreateLoft(IList<Polyline> polylines)
            {
                if (Enumerable.All(polylines, p => p.IsClosed))
                    return CreateLoftClosed(polylines);
                else
                    return CreateLoftOpen(polylines);
            }


            /// <summary>
            /// 
            /// </summary>
            private static T CreateLoftOpen(IList<Polyline> polylines)
            {
                T result = new T();
                var verts = result.Vertices;
                var faces = result.Faces;

                int ny = polylines.Count;
                int nx = Enumerable.Min(polylines, p => p.Count);
                int n;

                // add vertices
                for (int i = 0; i < ny; i++)
                {
                    var poly = polylines[i];

                    for (int j = 0; j < nx; j++)
                        verts.Add(poly[j]);
                }

                // add faces
                for (int i = 0; i < ny - 1; i++)
                {
                    n = i * nx;

                    for (int j = 0; j < nx - 1; j++)
                        faces.AddFace(n + j, n + j + 1, n + j + nx + 1, n + j + nx);
                }

                return result;
            }


            /// <summary>
            /// 
            /// </summary>
            private static T CreateLoftClosed(IList<Polyline> polylines)
            {
                T result = new T();
                var verts = result.Vertices;
                var faces = result.Faces;

                int ny = polylines.Count;
                int nx = Enumerable.Min(polylines, p => p.Count) - 1;
                int n;

                // add vertices
                for (int i = 0; i < ny; i++)
                {
                    var poly = polylines[i];

                    for (int j = 0; j < nx; j++)
                        verts.Add(poly[j]);
                }

                // add faces
                for (int i = 0; i < ny - 1; i++)
                {
                    n = i * nx;

                    for (int j0 = 0; j0 < nx; j0++)
                    {
                        int j1 = (j0 + 1) % nx;
                        faces.AddFace(n + j0, n + j1, n + j1 + nx, n + j0 + nx);
                    }
                }

                return result;
            }

           
            /// <summary>
            /// 
            /// </summary>
            /// <param name="mesh"></param>
            /// <returns></returns>
            public static T CreatePolySoup(T mesh)
            {
                var verts = mesh.Vertices;
                var faces = mesh.Faces;

                var newMesh = new T();
                var newVerts = newMesh.Vertices;
                var newFaces = newMesh.Faces;

                // add verts and faces
                for (int i = 0; i < faces.Count; i++)
                {
                    var f = faces[i];
                    int nv = newVerts.Count;

                    if (f.IsTriangle)
                    {
                        for (int j = 0; j < 3; j++)
                            newVerts.Add(verts[f[j]]);

                        newFaces.AddFace(nv, nv + 1, nv + 2);
                    }
                    else
                    {
                        for (int j = 0; j < 4; j++)
                            newVerts.Add(verts[f[j]]);

                        newFaces.AddFace(nv, nv + 1, nv + 2, nv + 3);
                    }
                }

                return newMesh;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="V"></typeparam>
            /// <typeparam name="E"></typeparam>
            /// <typeparam name="F"></typeparam>
            /// <param name="mesh"></param>
            /// <param name="getPosition"></param>
            /// <param name="getColor"></param>
            /// <param name="quadrangulator"></param>
            /// <returns></returns>
            public static T CreatePolySoup<V, E, F>(HeMesh<V, E, F> mesh, Func<V, Point3f> getPosition, Func<F, Color> getColor = null, IFaceQuadrangulator<V, E, F> quadrangulator = null)
                where V : HeMesh<V, E, F>.Vertex
                where E : HeMesh<V, E, F>.Halfedge
                where F : HeMesh<V, E, F>.Face
            {
                var result = new T();
                var newVerts = result.Vertices;
                var newColors = result.VertexColors;
                var newFaces = result.Faces;

                // default quadrangulator
                if (quadrangulator == null)
                    quadrangulator = FaceQuadrangulator.CreateStrip(mesh);

                // add vertices per face
                foreach (var f in mesh.Faces)
                {
                    if (f.IsUnused) continue;
                    int degree = f.Degree;
                    int nv = newVerts.Count;

                    // add colors
                    if (getColor != null)
                    {
                        var c = getColor(f);
                        for (int i = 0; i < degree; i++) newColors.Add(c);
                    }

                    // handle n-gons
                    if (degree > 4)
                    {
                        var quads = quadrangulator.GetQuads(f);
               
                        // add first 2 vertices
                        var first = quads.First();
                        newVerts.Add(getPosition(first.Item1));
                        newVerts.Add(getPosition(first.Item2));

                        // add remaining vertices and faces
                        foreach (var quad in quads)
                        {
                            var v0 = quad.Item3;
                            var v1 = quad.Item4;
                            
                            if (v1 == null)
                            {
                                newVerts.Add(getPosition(v0));
                                newFaces.AddFace(nv, nv + 1, nv + 2);
                                break;
                            }

                            newVerts.Add(getPosition(v1));
                            newVerts.Add(getPosition(v0));
                            newFaces.AddFace(nv, nv + 1, nv + 3, nv + 2);

                            nv += 2;
                        }
                    }
                    else
                    {
                        // add face vertices
                        foreach (var v in f.Vertices)
                            newVerts.Add(getPosition(v));

                        // add face
                        if (degree == 3)
                            newFaces.AddFace(nv, nv + 1, nv + 2);
                        else
                            newFaces.AddFace(nv, nv + 1, nv + 2, nv + 3);
                    }
                }

                return result;
            }


            /*
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="V"></typeparam>
            /// <typeparam name="E"></typeparam>
            /// <typeparam name="F"></typeparam>
            /// <param name="mesh"></param>
            /// <param name="getPosition"></param>
            /// <param name="quadrangulator"></param>
            /// <returns></returns>
            public static M CreatePolySoup<V, E, F>(HeMesh<V, E, F> mesh, Func<V, Point3f> getPosition, Func<F, Color> getColor = null, IFaceQuadrangulator<V, E, F> quadrangulator = null)
                where V : HeMeshBase<V, E, F>.Vertex
                where E : HeMeshBase<V, E, F>.Halfedge
                where F : HeMeshBase<V, E, F>.Face
            {
                var result = new M();
                var newVerts = result.Vertices;
                var newColors = result.VertexColors;
                var newFaces = result.Faces;

                // default quadrangulator
                if (quadrangulator == null)
                    quadrangulator = FaceQuadrangulators.Strip.Create(mesh);

                // add vertices per face
                foreach (var f in mesh.Faces)
                {
                    if (f.IsUnused) continue;
                    int nv = newVerts.Count;
                    int degree = 0;

                    // add face vertices
                    foreach (var v in f.Vertices)
                    {
                        newVerts.Add(getPosition(v));
                        degree++;
                    }

                    // add colors
                    if (getColor != null)
                    {
                        var c = getColor(f);
                        for (int i = 0; i < degree; i++) newColors.Add(c);
                    }

                    // add face(s)
                    if (degree == 3)
                    {
                        newFaces.AddFace(nv, nv + 1, nv + 2);
                    }
                    else if (degree == 4)
                    {
                        newFaces.AddFace(nv, nv + 1, nv + 2, nv + 3);
                    }
                    else
                    {
                        foreach (var quad in quadrangulator.GetQuads(f))
                        {
                            if (quad.Item4 == null)
                                newFaces.AddFace(nv + quad.Item1, nv + quad.Item2, nv + quad.Item3);
                            else
                                newFaces.AddFace(nv + quad.Item1, nv + quad.Item2, nv + quad.Item3, nv + quad.Item4);
                        }
                    }
                }

                return result;
            }
            */


            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="V"></typeparam>
            /// <typeparam name="E"></typeparam>
            /// <typeparam name="F"></typeparam>
            /// <param name="mesh"></param>
            /// <param name="getPosition"></param>
            /// <param name="quadrangulator"></param>
            /// <param name="getNormal"></param>
            /// <param name="getTexture"></param>
            /// <param name="getColor"></param>
            /// <returns></returns>
            public static T CreateFromHeMesh<V, E, F>(HeMesh<V, E, F> mesh, Func<V, Point3f> getPosition, Func<V, Vector3f> getNormal = null, Func<V, Point2f> getTexture = null, Func<V, Color> getColor = null, IFaceQuadrangulator<V, E, F> quadrangulator = null)
                where V : HeMesh<V, E, F>.Vertex
                where E : HeMesh<V, E, F>.Halfedge
                where F : HeMesh<V, E, F>.Face
            {
                var verts = mesh.Vertices;

                var result = new T();
                var newVerts = result.Vertices;
                var newFaces = result.Faces;

                var newNorms = result.Normals;
                var newCoords = result.TextureCoordinates;
                var newColors = result.VertexColors;

                // default quadrangulator
                if (quadrangulator == null)
                    quadrangulator = FaceQuadrangulator.CreateStrip(mesh);

                // add vertices
                for (int i = 0; i < verts.Count; i++)
                {
                    var v = verts[i];
                    newVerts.Add(getPosition(v));

                    if (getNormal != null)
                        newNorms.Add(getNormal(v));

                    if (getTexture != null)
                        newCoords.Add(getTexture(v));

                    if (getColor != null)
                        newColors.Add(getColor(v));
                }

                // add faces
                foreach (var f in mesh.Faces)
                {
                    if (f.IsUnused) continue;
                    var he = f.First;
                    int degree = f.Degree;

                    if (degree == 3)
                    {
                        newFaces.AddFace(
                            he.Start.Index,
                            he.Next.Start.Index,
                            he.Previous.Start.Index
                            );
                    }
                    else if (degree == 4)
                    {
                        newFaces.AddFace(
                            he.Start.Index,
                            he.Next.Start.Index,
                            he.Next.Next.Start.Index,
                            he.Previous.Start.Index
                            );
                    }
                    else
                    {
                        foreach (var quad in quadrangulator.GetQuads(f))
                        {
                            if (quad.Item4 == null)
                                newFaces.AddFace(quad.Item1, quad.Item2, quad.Item3);
                            else
                                newFaces.AddFace(quad.Item1, quad.Item2, quad.Item3, quad.Item4);
                        }
                    }
                }

                return result;
            }
            

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="V"></typeparam>
            /// <typeparam name="E"></typeparam>
            /// <typeparam name="F"></typeparam>
            /// <param name="strip"></param>
            /// <param name="getPosition"></param>
            /// <param name="getNormal"></param>
            /// <param name="getTexture"></param>
            /// <param name="getColor"></param>
            /// <returns></returns>
            public static T CreateFromQuadStrip<V, E, F>(HeQuadStrip<V, E, F> strip, Func<V, Point3f> getPosition, Func<V, Vector3f> getNormal = null, Func<V, Point2f> getTexture = null, Func<V, Color> getColor = null)
                where V : HeMesh<V, E, F>.Vertex
                where E : HeMesh<V, E, F>.Halfedge
                where F : HeMesh<V, E, F>.Face
            {
                var mesh = new T();

                var verts = mesh.Vertices;
                var faces = mesh.Faces;

                var norms = mesh.Normals;
                var texCoords = mesh.TextureCoordinates;
                var colors = mesh.VertexColors;

                int skip = strip.IsPeriodic ? 1 : 0;

                // add verts
                foreach (var he in strip.SkipLast(skip))
                {
                    var v0 = he.Start;
                    var v1 = he.End;

                    verts.Add(getPosition(v0));
                    verts.Add(getPosition(v1));

                    if (getNormal != null)
                    {
                        norms.Add(getNormal(v0));
                        norms.Add(getNormal(v1));
                    }

                    if (getTexture != null)
                    {
                        texCoords.Add(getTexture(v0));
                        texCoords.Add(getTexture(v1));
                    }

                    if (getColor != null)
                    {
                        colors.Add(getColor(v0));
                        colors.Add(getColor(v1));
                    }
                }

                // add faces
                var n = verts.Count - 2;
                for (int i = 0; i < n; i += 2)
                    faces.AddFace(i, i + 1, i + 3, i + 2);

                // add last face
                if (strip.IsPeriodic)
                    faces.AddFace(n, n + 1, 1, 0);

                return mesh;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="mesh"></param>
            /// <param name="vertexValues"></param>
            /// <param name="interval"></param>
            /// <returns></returns>
            public static T CreateIsoTrim(T mesh, IReadOnlyList<double> vertexValues, Interval interval)
            {

                //TO DO 
                //implement multithread version 
                //Known issues: degenerate case, vertex value equal to threshold

                // input mesh must be triangulated
                mesh.Faces.ConvertQuadsToTriangles();

                interval = SortInterval(interval);

                T _a = new T(); //band within the interval 
                T _b = new T(); //band outside the interval

                foreach (MeshFace mf in mesh.Faces)
                {
                    Point3d p0 = mesh.Vertices[mf.A];
                    Point3d p1 = mesh.Vertices[mf.B];
                    Point3d p2 = mesh.Vertices[mf.C];

                    double t0 = vertexValues[mf.A];
                    double t1 = vertexValues[mf.B];
                    double t2 = vertexValues[mf.C];


                    //Create a mask based on face vertex values
                    int mask = 0;

                    if (interval.IncludesParameter(t0)) { mask += 1; }
                    if (t0 > interval.T1) { mask += 2; }

                    if (interval.IncludesParameter(t1)) { mask += 3; }
                    if (t1 > interval.T1) { mask += 6; }

                    if (interval.IncludesParameter(t2)) { mask += 9; }
                    if (t2 > interval.T1) { mask += 18; }

                    List<Point3d> verts;

                    switch (mask)
                    {
                        case 0:

                            verts = new List<Point3d>();
                            verts.Add(p0);
                            verts.Add(p1);
                            verts.Add(p2);
                            AddFace(_a, verts);
                            break;

                        case 1:
                            Split(_a, _b, p0, p1, p2, Normalize(t0, t1, interval.T0), Normalize(t0, t2, interval.T0));
                            break;
                        case 2:
                            Trapezoid(_a, _b, p0, p1, p2, Normalize(t0, t1, interval.T1), Normalize(t0, t1, interval.T0), Normalize(t0, t2, interval.T1), Normalize(t0, t2, interval.T0));
                            break;
                        case 3:
                            Split(_a, _b, p1, p2, p0, Normalize(t1, t2, interval.T0), Normalize(t1, t0, interval.T0));
                            break;
                        case 4:
                            Split(_b, _a, p2, p0, p1, Normalize(t2, t0, interval.T0), Normalize(t2, t1, interval.T0));
                            break;
                        case 5:
                            Pentagon(_a, _b, p1, p2, p0, Normalize(t1, t2, interval.T0), Normalize(t1, t0, interval.T1), Normalize(t2, t0, interval.T0), Normalize(t2, t0, interval.T1));
                            break;
                        case 6:
                            Trapezoid(_a, _b, p1, p2, p0, Normalize(t1, t2, interval.T1), Normalize(t1, t2, interval.T0), Normalize(t1, t0, interval.T1), Normalize(t1, t0, interval.T0));
                            break;
                        case 7:
                            Pentagon(_a, _b, p0, p1, p2, Normalize(t0, t1, interval.T1), Normalize(t0, t2, interval.T0), Normalize(t1, t2, interval.T1), Normalize(t1, t2, interval.T0));
                            break;
                        case 8:
                            Trapezoid(_a, _b, p2, p0, p1, Normalize(t2, t0, interval.T0), Normalize(t2, t0, interval.T1), Normalize(t2, t1, interval.T0), Normalize(t2, t1, interval.T1));
                            break;
                        case 9:
                            Split(_a, _b, p2, p0, p1, Normalize(t2, t0, interval.T0), Normalize(t2, t1, interval.T0));
                            break;
                        case 10:
                            Split(_b, _a, p1, p2, p0, Normalize(t1, t2, interval.T0), Normalize(t1, t0, interval.T0));
                            break;
                        case 11:
                            Pentagon(_a, _b, p2, p0, p1, Normalize(t2, t0, interval.T1), Normalize(t2, t1, interval.T0), Normalize(t0, t1, interval.T1), Normalize(t0, t1, interval.T0));
                            break;
                        case 12:
                            Split(_b, _a, p0, p1, p2, Normalize(t0, t1, interval.T0), Normalize(t0, t2, interval.T0));
                            break;
                        case 13:
                            verts = new List<Point3d>();
                            verts.Add(p0);
                            verts.Add(p1);
                            verts.Add(p2);
                            AddFace(_b, verts);
                            break;
                        case 14:
                            Split(_b, _a, p0, p1, p2, Normalize(t0, t1, interval.T1), Normalize(t0, t2, interval.T1));
                            break;
                        case 15:
                            Pentagon(_a, _b, p2, p0, p1, Normalize(t2, t0, interval.T0), Normalize(t2, t1, interval.T1), Normalize(t0, t1, interval.T0), Normalize(t0, t1, interval.T1));
                            break;
                        case 16:
                            Split(_b, _a, p1, p2, p0, Normalize(t1, t2, interval.T1), Normalize(t1, t0, interval.T1));
                            break;
                        case 17:
                            Split(_a, _b, p2, p0, p1, Normalize(t2, t0, interval.T1), Normalize(t2, t1, interval.T1));
                            break;
                        case 18:
                            Trapezoid(_a, _b, p2, p0, p1, Normalize(t2, t0, interval.T1), Normalize(t2, t0, interval.T0), Normalize(t2, t1, interval.T1), Normalize(t2, t1, interval.T0));
                            break;
                        case 19:
                            Pentagon(_a, _b, p0, p1, p2, Normalize(t0, t1, interval.T0), Normalize(t0, t2, interval.T1), Normalize(t1, t2, interval.T0), Normalize(t1, t2, interval.T1));
                            break;
                        case 20:
                            Trapezoid(_a, _b, p1, p2, p0, Normalize(t1, t2, interval.T0), Normalize(t1, t2, interval.T1), Normalize(t1, t0, interval.T0), Normalize(t1, t0, interval.T1));
                            break;
                        case 21:
                            Pentagon(_a, _b, p1, p2, p0, Normalize(t1, t2, interval.T1), Normalize(t1, t0, interval.T0), Normalize(t2, t0, interval.T1), Normalize(t2, t0, interval.T0));
                            break;
                        case 22:
                            Split(_b, _a, p2, p0, p1, Normalize(t2, t0, interval.T1), Normalize(t2, t1, interval.T1));
                            break;
                        case 23:
                            Split(_a, _b, p1, p2, p0, Normalize(t1, t2, interval.T1), Normalize(t1, t0, interval.T1));
                            break;
                        case 24:
                            Trapezoid(_a, _b, p0, p1, p2, Normalize(t0, t1, interval.T0), Normalize(t0, t1, interval.T1), Normalize(t0, t2, interval.T0), Normalize(t0, t2, interval.T1));
                            break;
                        case 25:
                            Split(_a, _b, p0, p1, p2, Normalize(t0, t1, interval.T1), Normalize(t0, t2, interval.T1));
                            break;
                        case 26:
                            verts = new List<Point3d>();
                            verts.Add(p0);
                            verts.Add(p1);
                            verts.Add(p2);
                            AddFace(_a, verts);
                            break;
                    }
                }

                return _b;


                void Split(T m0, T m1, Point3d p0, Point3d p1, Point3d p2, double t0, double t1)
                {
                    Point3d p01 = Lerp(p0, p1, t0);
                    Point3d p02 = Lerp(p0, p2, t1);

                    //gets the quad
                    List<Point3d> verts;
                    verts = new List<Point3d>();
                    verts.Add(p1);
                    verts.Add(p2);
                    verts.Add(p02);
                    verts.Add(p01);
                    AddFace(m0, verts);

                    //get the tri
                    verts = new List<Point3d>();
                    verts.Add(p0);
                    verts.Add(p01);
                    verts.Add(p02);
                    AddFace(m1, verts);
                }

                void Trapezoid(T m0, T m1, Point3d p0, Point3d p1, Point3d p2, double t0a, double t0b, double t1a, double t1b)
                {
                    Point3d p01a = Lerp(p0, p1, t0a);
                    Point3d p01b = Lerp(p0, p1, t0b);

                    Point3d p02a = Lerp(p0, p2, t1a);
                    Point3d p02b = Lerp(p0, p2, t1b);

                    //gets the quad
                    List<Point3d> verts;
                    verts = new List<Point3d>();
                    verts.Add(p1);
                    verts.Add(p2);
                    verts.Add(p02b);
                    verts.Add(p01b);
                    AddFace(m0, verts);

                    //gets the quad
                    verts = new List<Point3d>();
                    verts.Add(p01b);
                    verts.Add(p02b);
                    verts.Add(p02a);
                    verts.Add(p01a);
                    AddFace(m1, verts);

                    //get the tri
                    verts = new List<Point3d>();
                    verts.Add(p0);
                    verts.Add(p01a);
                    verts.Add(p02a);
                    AddFace(m0, verts);
                }


                void Pentagon(T m0, T m1, Point3d p0, Point3d p1, Point3d p2, double t0, double t1, double t2a, double t2b)
                {
                    Point3d p01 = Lerp(p0, p1, t0);

                    Point3d p02 = Lerp(p0, p2, t1);

                    Point3d p03a = Lerp(p1, p2, t2a);
                    Point3d p03b = Lerp(p1, p2, t2b);

                    List<Point3d> verts;

                    //get the tri
                    verts = new List<Point3d>();
                    verts.Add(p0);
                    verts.Add(p01);
                    verts.Add(p02);
                    AddFace(m1, verts);

                    //gets the quad
                    verts = new List<Point3d>();
                    verts.Add(p02);
                    verts.Add(p01);
                    verts.Add(p03a);
                    verts.Add(p03b);
                    AddFace(m1, verts);

                    //get the tri
                    verts = new List<Point3d>();
                    verts.Add(p01);
                    verts.Add(p03a);
                    verts.Add(p1);
                    AddFace(m0, verts);

                    //get the tri
                    verts = new List<Point3d>();
                    verts.Add(p02);
                    verts.Add(p03b);
                    verts.Add(p2);
                    AddFace(m0, verts);
                }



                void AddFace(T m, List<Point3d> verts)
                {

                    if (verts.Count == 3)
                    {
                        m.Vertices.Add(verts[0]);
                        m.Vertices.Add(verts[1]);
                        m.Vertices.Add(verts[2]);
                        
                        int count = m.Vertices.Count;
                        m.Faces.AddFace(count - 3, count - 2, count - 1);

                        
                    }
                    else if (verts.Count == 4)
                    {
                        m.Vertices.Add(verts[0]);
                        m.Vertices.Add(verts[1]);
                        m.Vertices.Add(verts[2]);
                        m.Vertices.Add(verts[3]);
                        int count = m.Vertices.Count;
                        m.Faces.AddFace(count - 4, count - 3, count - 2, count - 1);
                    }
                    else if (verts.Count == 5)
                    {
                        m.Vertices.Add(verts[0]);
                        m.Vertices.Add(verts[1]);
                        m.Vertices.Add(verts[2]);
                        m.Vertices.Add(verts[3]);
                        m.Vertices.Add(verts[4]);
                        int count = m.Vertices.Count;

                        m.Faces.AddFace(count - 4, count - 3, count - 2, count - 1);
                        m.Faces.AddFace(count - 5, count - 4, count - 3);
                    }
                }



                Point3d Lerp(Point3d p0, Point3d p1, double t)
                {
                    return p0 + (p1 - p0) * t;
                }

                double Normalize(double t0, double t1, double t)
                {
                    return (t - t0) / (t1 - t0);
                }

                Interval SortInterval(Interval _interval)
                {
                    if (_interval.T0 > _interval.T1)
                    {
                        return _interval = new Interval(_interval.T1, _interval.T0);
                    }
                    else
                    {
                        return _interval;
                    }
                }
            }

        }
    }
}

namespace zCode.zRhino
{
    using T = Rhino.Geometry.Transform;

    /// <summary>
    /// 
    /// </summary>
    public static partial class RhinoFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public static class Transform
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="plane"></param>
            public static T CreateFromPlane(Plane plane)
            {
                return Create(plane.Origin, plane.XAxis, plane.YAxis, plane.ZAxis);
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="plane"></param>
            public static T CreateInverseFromPlane(Plane plane)
            {
                return CreateOrthoInverse(plane.Origin, plane.XAxis, plane.YAxis, plane.ZAxis);
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="xAxis"></param>
            /// <param name="xyVector"></param>
            /// <returns></returns>
            public static T CreateProperRigid(Vec3d origin, Vec3d xAxis, Vec3d xyVector)
            {
                if (GeometryUtil.Orthonormalize(ref xAxis, ref xyVector, out Vec3d z))
                    return Create(origin, xAxis, xyVector, z);

                return T.Unset;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="xAxis"></param>
            /// <param name="xyVector"></param>
            /// <returns></returns>
            public static T CreateProperRigidInverse(Vec3d origin, Vec3d xAxis, Vec3d xyVector)
            {
                if (GeometryUtil.Orthonormalize(ref xAxis, ref xyVector, out Vec3d z))
                    return CreateOrthoInverse(origin, xAxis, xyVector, z);

                return T.Unset;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="basisX"></param>
            /// <param name="basisY"></param>
            /// <param name="basisZ"></param>
            /// <returns></returns>
            public static T Create(Vec3d origin, Vec3d basisX, Vec3d basisY, Vec3d basisZ)
            {
                T m = T.Identity;

                m.M00 = basisX.X;
                m.M01 = basisY.X;
                m.M02 = basisZ.X;
                m.M03 = origin.X;

                m.M10 = basisX.Y;
                m.M11 = basisY.Y;
                m.M12 = basisZ.Y;
                m.M13 = origin.Y;

                m.M20 = basisX.Z;
                m.M21 = basisY.Z;
                m.M22 = basisZ.Z;
                m.M23 = origin.Z;

                return m;
            }


            /// <summary>
            /// Assumes the given axes are orthonormal.
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <returns></returns>
            private static T CreateOrthoInverse(Vec3d origin, Vec3d x, Vec3d y, Vec3d z)
            {
                T m = T.Identity;

                m.M00 = x.X;
                m.M01 = x.Y;
                m.M02 = x.Z;
                m.M03 = -Vec3d.Dot(origin, x);

                m.M10 = y.X;
                m.M11 = y.Y;
                m.M12 = y.Z;
                m.M13 = -Vec3d.Dot(origin, y);

                m.M20 = z.X;
                m.M21= z.Y;
                m.M22 = z.Z;
                m.M23 = -Vec3d.Dot(origin, z);

                return m;
            }
        }
    }
}

#endif