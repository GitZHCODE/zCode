﻿
/*
 * Notes
 * 
 * TODO support user-specified vertex attributes
 * 
 * Refs
 * http://graphics.stanford.edu/courses/cs468-10-fall/LectureSlides/10_Subdivision.pdf
 */

using System;
using zCode.zCore;

namespace zCode.zMesh
{
    /// <summary>
    /// 
    /// </summary>
    public static class HeSubdivide
    {
        #region Tri Split

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        public static void TriSplit<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            // TODO implement
            throw new NotImplementedException();
        }

        #endregion


        #region Loop

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        public static void Loop<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            // TODO implement
            throw new NotImplementedException();
        }

        #endregion


        #region Quad Split

        /// <summary>
        /// Applies a single iteration of Catmull-Clark subdivision to the given mesh.
        /// If using external buffers to store vertex attributes, the number of vertices after subdivision equals the sum of the number of vertices, edges, and faces in the initial mesh.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        public static void QuadSplit<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            QuadSplitGeometry(mesh, position);
            QuadSplitTopology(mesh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        private static void QuadSplitGeometry<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            // create face vertices
            foreach (var f in mesh.Faces)
            {
                var v = mesh.AddVertex();

                if (!f.IsUnused)
                    position.Set(v, f.Vertices.Mean(position.Get));
            }

            // create edge vertices
            foreach (var he in mesh.Edges)
            {
                var v = mesh.AddVertex();

                if (!he.IsUnused)
                {
                    var p = (position.Get(he.Start) + position.Get(he.End)) * 0.5;
                    position.Set(v, p);
                }
            }
        }


        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        private static void QuadSplitTopology<V, E, F>(HeMesh<V, E, F> mesh)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            var verts = mesh.Vertices;
            var hedges = mesh.Halfedges;
            var faces = mesh.Faces;

            int ne = hedges.Count >> 1;
            int nf = faces.Count;

            int ev0 = verts.Count - ne; // index of first edge vertex
            int fv0 = ev0 - nf; // index of first face vertex (also the number of vertices in the initial mesh)

            // split edges
            for (int i = 0; i < ne; i++)
            {
                var he = hedges[i << 1];
                if (he.IsUnused) continue;

                var ev = verts[i + ev0];
                mesh.SplitEdgeImpl(he, ev);
            }

            // split faces
            for (int i = 0; i < nf; i++)
            {
                var f = faces[i];
                if (f.IsUnused) continue;

                var he = f.First;
                if (he.Start.Index >= fv0) he = he.Previous; // ensure halfedge starts from an old vertex
                mesh.QuadSplitFace(he, verts[i + fv0]);
            }
        }

        #endregion


        #region Catmull Clark

        /// <summary>
        /// Applies a single iteration of Catmull-Clark subdivision to the given mesh.
        /// If using external buffers to store vertex attributes, the number of vertices after subdivision equals the sum of the number of vertices, edges, and faces in the initial mesh.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        /// <param name="boundaryType"></param>
        public static void CatmullClark<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position, SmoothBoundaryType boundaryType)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            // impl ref
            // http://rosettacode.org/wiki/Catmull%E2%80%93Clark_subdivision_surface
            // http://w3.impa.br/~lcruz/courses/cma/surfaces.html

            CatmullClarkGeometry(mesh, position, boundaryType);
            QuadSplitTopology(mesh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        /// <param name="boundaryType"></param>
        private static void CatmullClarkGeometry<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position, SmoothBoundaryType boundaryType)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            var verts = mesh.Vertices;
            int fv0 = verts.Count; // index of first face vertex

            // create face vertices
            foreach (var f in mesh.Faces)
            {
                var v = mesh.AddVertex();

                if (!f.IsUnused)
                    position.Set(v, f.Vertices.Mean(position.Get));
            }

            // create edge vertices
            foreach (var he0 in mesh.Edges)
            {
                var v = mesh.AddVertex();
                if (he0.IsUnused) continue;

                if (he0.IsBoundary)
                {
                    var p = (position.Get(he0.Start) + position.Get(he0.End)) * 0.5;
                    position.Set(v, p);
                    continue;
                }

                var he1 = he0.Twin;
                var p0 = position.Get(he0.Start);
                var p1 = position.Get(he1.Start);
                var p2 = position.Get(verts[he0.Face.Index + fv0]);
                var p3 = position.Get(verts[he1.Face.Index + fv0]);
                position.Set(v, (p0 + p1 + p2 + p3) * 0.25);
            }

            // smooth old vertices
            CatmullClarkSmooth(mesh, position, boundaryType);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void CatmullClarkSmooth<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position, SmoothBoundaryType boundaryType)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            switch (boundaryType)
            {
                case SmoothBoundaryType.Fixed:
                    CatmullClarkSmoothFixed(mesh, position);
                    return;
                case SmoothBoundaryType.CornerFixed:
                    CatmullClarkSmoothCornerFixed(mesh, position);
                    return;
                case SmoothBoundaryType.Free:
                    CatmullClarkSmoothFree(mesh, position);
                    return;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// 
        /// </summary>
        private static void CatmullClarkSmoothFixed<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            var verts = mesh.Vertices;

            int ev0 = verts.Count - mesh.Edges.Count; // index of first edge vertex
            int fv0 = ev0 - mesh.Faces.Count; // index of first face vertex

            // set old vertices
            for (int i = 0; i < fv0; i++)
            {
                var v = verts[i];
                if (v.IsUnused || v.IsBoundary) continue; // skip boundary verts

                var fsum = new Vec3d();
                var esum = new Vec3d();
                int n = 0;

                foreach (var he in v.OutgoingHalfedges)
                {
                    fsum += position.Get(verts[he.Face.Index + fv0]);
                    esum += position.Get(verts[(he.Index >> 1) + ev0]);
                    n++;
                }

                var t = 1.0 / n;
                position.Set(v, (position.Get(v) * (n - 3) + fsum * t + 2 * esum * t) * t);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static void CatmullClarkSmoothCornerFixed<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            var verts = mesh.Vertices;

            int ev0 = verts.Count - mesh.Edges.Count; // index of first edge vertex
            int fv0 = ev0 - mesh.Faces.Count; // index of first face vertex

            // set old vertices
            for (int i = 0; i < fv0; i++)
            {
                var v = verts[i];
                if (v.IsUnused) continue;

                if (v.IsBoundary)
                {
                    var he0 = v.First;
                    if (he0.IsAtDegree2) continue; // skip corner verts

                    var he1 = he0.Previous;
                    var p0 = position.Get(verts[(he0.Index >> 1) + ev0]);
                    var p1 = position.Get(verts[(he1.Index >> 1) + ev0]);
                    position.Set(v, position.Get(v) * 0.5 + (p0 + p1) * 0.25);
                }
                else
                {
                    Vec3d fsum = new Vec3d();
                    Vec3d esum = new Vec3d();
                    int n = 0;

                    foreach (var he in v.OutgoingHalfedges)
                    {
                        fsum += position.Get(verts[he.Face.Index + fv0]);
                        esum += position.Get(verts[(he.Index >> 1) + ev0]);
                        n++;
                    }

                    double t = 1.0 / n;
                    position.Set(v, (position.Get(v) * (n - 3) + fsum * t + 2 * esum * t) * t);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static void CatmullClarkSmoothFree<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            var verts = mesh.Vertices;

            int ev0 = verts.Count - mesh.Edges.Count; // index of first edge vertex
            int fv0 = ev0 - mesh.Faces.Count; // index of first face vertex

            // set old vertices
            for (int i = 0; i < fv0; i++)
            {
                var v = verts[i];
                if (v.IsUnused) continue;

                if (v.IsBoundary)
                {
                    var he0 = v.First;
                    var he1 = he0.Previous;
                    var p0 = position.Get(verts[(he0.Index >> 1) + ev0]);
                    var p1 = position.Get(verts[(he1.Index >> 1) + ev0]);
                    position.Set(v, position.Get(v) * 0.5 + (p0 + p1) * 0.25);
                }
                else
                {
                    Vec3d fsum = new Vec3d();
                    Vec3d esum = new Vec3d();
                    int n = 0;

                    foreach (var he in v.OutgoingHalfedges)
                    {
                        fsum += position.Get(verts[he.Face.Index + fv0]);
                        esum += position.Get(verts[(he.Index >> 1) + ev0]);
                        n++;
                    }

                    double t = 1.0 / n;
                    position.Set(v, (position.Get(v) * (n - 3) + fsum * t + 2 * esum * t) * t);
                }
            }
        }


        /*
        /// <summary>
        /// Applies a single iteration of Catmull-Clark subdivision to the given mesh.
        /// http://rosettacode.org/wiki/Catmull%E2%80%93Clark_subdivision_surface
        /// http://w3.impa.br/~lcruz/courses/cma/surfaces.html
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        /// <param name="boundaryType"></param>
        public static void CatmullClark<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position, SmoothBoundaryType boundaryType, bool parallel)
            where V : HeMeshBase<V, E, F>.Vertex
            where E : HeMeshBase<V, E, F>.Halfedge
            where F : HeMeshBase<V, E, F>.Face
        {
            CatmullClarkGeometry(mesh, position, boundaryType, parallel);
            QuadSplitTopology(mesh);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void CatmullClarkGeometry<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position, SmoothBoundaryType boundaryType, bool parallel)
        where V : HeMeshBase<V, E, F>.Vertex
        where E : HeMeshBase<V, E, F>.Halfedge
        where F : HeMeshBase<V, E, F>.Face
        {
            var verts = mesh.Vertices;
            var edges = mesh.Edges;
            var faces = mesh.Faces;

            int fv0 = verts.Count; // index of first face vertex
            int ev0 = verts.Count + faces.Count;

            // add all new vertices
            mesh.AddVertices(faces.Count);
            mesh.AddVertices(edges.Count);

            // set attributes of face vertices
            Action<Tuple<int, int>> setFaceVerts = range =>
             {
                 for (int i = range.Item1; i < range.Item2; i++)
                 {
                     var f = faces[i];

                     if (!f.IsUnused)
                         position.Set(verts[i + fv0], f.Vertices.Mean(position.Get));
                 }
             };

            // set attributes of edge vertices
            Action<Tuple<int, int>> setEdgeVerts = range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var he0 = edges[i];
                    if (he0.IsUnused) continue;

                    if (he0.IsBoundary)
                    {
                        position.Set(verts[i + ev0], he0.Lerp(position.Get, 0.5));
                        continue;
                    }

                    var he1 = he0.Twin;
                    var p0 = position.Get(he0.Start);
                    var p1 = position.Get(he1.Start);
                    var p2 = position.Get(verts[he0.Face.Index + fv0]);
                    var p3 = position.Get(verts[he1.Face.Index + fv0]);
                    position.Set(verts[i + ev0], (p0 + p1 + p2 + p3) * 0.25);
                }
            };

            // set attributes of old vertices
            //CatmullClarkSmooth(mesh, position, boundaryType);
            Action<Tuple<int, int>> setOldVerts = range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var v = verts[i];
                    if (v.IsUnused) continue;

                    if (v.IsBoundary)
                    {
                        var he0 = v.FirstOut;
                        var he1 = he0.PrevInFace;
                        var p0 = position.Get(verts[(he0.Index >> 1) + ev0]);
                        var p1 = position.Get(verts[(he1.Index >> 1) + ev0]);
                        position.Set(v, position.Get(v) * 0.5 + (p0 + p1) * 0.25);
                    }
                    else
                    {
                        Vec3d fsum = new Vec3d();
                        Vec3d esum = new Vec3d();
                        int n = 0;

                        foreach (var he in v.OutgoingHalfedges)
                        {
                            fsum += position.Get(verts[he.Face.Index + fv0]);
                            esum += position.Get(verts[(he.Index >> 1) + ev0]);
                            n++;
                        }

                        double t = 1.0 / n;
                        position.Set(v, (position.Get(v) * (n - 3) + fsum * t + 2 * esum * t) * t);
                    }
                }
            };

            
            if (parallel)
            {
                Parallel.ForEach(Partitioner.Create(0, faces.Count), setFaceVerts);
                Parallel.ForEach(Partitioner.Create(0, edges.Count), setEdgeVerts);
                Parallel.ForEach(Partitioner.Create(0, verts.Count), setOldVerts);
            }
            else
            {
                setFaceVerts(Tuple.Create(0, faces.Count));
                setEdgeVerts(Tuple.Create(0, edges.Count));
                setOldVerts(Tuple.Create(0, verts.Count));
            }
        }
        */

        #endregion


        /// <summary>
        /// If using external buffers to store vertex attributes, the number of vertices after subdivision equals the sum of the number of vertices and faces in the initial mesh.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        /// <param name="skipBoundary"></param>
        public static void Diagonalize<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position, bool skipBoundary)
            where V : HeMesh<V, E, F>.Vertex
            where E : HeMesh<V, E, F>.Halfedge
            where F : HeMesh<V, E, F>.Face
        {
            var edges = mesh.Edges;
            var faces = mesh.Faces;

            int ne = edges.Count;
            int nf = faces.Count;

            // stellate faces
            for (int i = 0; i < nf; i++)
            {
                var f = faces[i];
                if (f.IsUnused) continue;

                var v = mesh.AddVertex();
                position.Set(v, f.GetBarycenter(position.Get));
                mesh.PokeFaceImpl(f.First, v);
            }

            // merge faces
            if (skipBoundary)
            {
                for (int i = 0; i < ne; i++)
                {
                    var he = edges[i];
                    if (he.IsUnused || he.IsBoundary) continue;
                    mesh.MergeFaces(he);
                }

            }
            else
            {
                for (int i = 0; i < ne; i++)
                {
                    var he = edges[i];
                    if (he.IsUnused) continue;
                    mesh.MergeFaces(he);
                }
            }
        }


        #region Obsolete implementations

        /*
        /// <summary>
        /// Applies a single iteration of Catmull-Clark subdivision to the given mesh.
        /// http://rosettacode.org/wiki/Catmull%E2%80%93Clark_subdivision_surface
        /// http://w3.impa.br/~lcruz/courses/cma/surfaces.html
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        /// <param name="boundaryType"></param>
        public static void CatmullClark<V, E, F>(HeMesh<V, E, F> mesh, Property<V, Vec3d> position, SmoothBoundaryType boundaryType)
            where V : HeMeshBase<V, E, F>.Vertex
            where E : HeMeshBase<V, E, F>.Halfedge
            where F : HeMeshBase<V, E, F>.Face
        {
            var verts = mesh.Vertices;
            var hedges = mesh.Halfedges;
            var faces = mesh.Faces;

            int nv = verts.Count;
            int nhe = hedges.Count;
            int nf = faces.Count;

            // create face verts
            for (int i = 0; i < nf; i++)
            {
                var f = faces[i];
                var v = mesh.AddVertex();

                if (!f.IsUnused)
                    position.Set(v, f.Vertices.Mean(position.Get));
            }

            // create edge vertices
            for (int i = 0; i < nhe; i += 2)
            {
                var he = hedges[i];
                if (he.IsUnused)
                {
                    mesh.AddVertex();
                    continue;
                }

                // split edge and set attributes of new vertex
                var p = he.Lerp(position.Get, 0.5);
                var v = mesh.SplitEdgeImpl(he).Start; // split edge (adds a new vertex)
                position.Set(v, p);


                var he0 = hedges[i];
                if (he0.IsUnused) continue;

                var he1 = hedges[i + 1];

                var v = verts[i + ev0];
                var p0 = position.Get(he0.Start);
                var p1 = position.Get(he1.Start);

                if (he0.IsBoundary)
                {
                    position.Set(v, (p0 + p1) * 0.5);
                    continue;
                }

                var p2 = position.Get(verts[he0.Face.Index + fv0]);
                var p3 = position.Get(verts[he1.Face.Index + fv0]);
                position.Set(v, (p0 + p1 + p2 + p3) * 0.25);
            }

            // split faces
            for (int i = 0; i < nf; i++)
            {
                var f = faces[i];
                if (f.IsUnused)
                {
                    mesh.AddVertex();
                    continue;
                }

                var he = f.First;
                if (he.Start.Index >= nv) he = he.PrevInFace; // ensure halfedge starts from an old vertex
                mesh.QuadSplitFace(he, verts[i + nv]);
            }
        }
        */


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="closeFaces"></param>
        /// <param name="closeVertices"></param>
        /// <returns></returns>
        public static HeMesh BevelEdges(HeMesh mesh, bool closeVertices = true, bool closeFaces = true)
        {
            return BevelEdgesTopo(mesh, closeVertices, closeFaces);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="faceCenters"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="newVertexPositions"></param>
        /// <param name="closeVertices"></param>
        /// <param name="closeFaces"></param>
        /// <returns></returns>
        public static HeMesh BevelEdges(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> faceCenters, double scaleFactor, out Vec3d[] newVertexPositions, bool closeVertices = true, bool closeFaces = true)
        {
            BevelEdgesGeom(mesh, vertexPositions, faceCenters, scaleFactor, out newVertexPositions);
            return BevelEdgesTopo(mesh, closeVertices, closeFaces);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="faceCenters"></param>
        /// <param name="faceFactors"></param>
        /// <param name="newVertexPositions"></param>
        /// <param name="closeVertices"></param>
        /// <param name="closeFaces"></param>
        /// <returns></returns>
        public static HeMesh BevelEdges(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> faceCenters, IReadOnlyList<double> faceFactors, out Vec3d[] newVertexPositions, bool closeVertices = true, bool closeFaces = true)
        {
            BevelEdgesGeom(mesh, vertexPositions, faceCenters, faceFactors, out newVertexPositions);
            return BevelEdgesTopo(mesh, closeVertices, closeFaces);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="faceCenters"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="newVertexPositions"></param>
        private static void BevelEdgesGeom(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> faceCenters, double scaleFactor, out Vec3d[] newVertexPositions)
        {
            var hedges = mesh.Halfedges;
            int nhe = hedges.Count;

            newVertexPositions = new Vec3d[nhe];

            // add one new vertex per halfedge
            for (int i = 0; i < nhe; i++)
            {
                var he = hedges[i];
                var f = he.Face;
                if (he.IsUnused || f == null) continue;

                int fi = f.Index;
                var p = vertexPositions[he.Start.Index];
                newVertexPositions[i] = Vec3d.Lerp(p, faceCenters[fi], scaleFactor);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="faceCenters"></param>
        /// <param name="faceFactors"></param>
        /// <param name="newVertexPositions"></param>
        private static void BevelEdgesGeom(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> faceCenters, IReadOnlyList<double> faceFactors, out Vec3d[] newVertexPositions)
        {
            var hedges = mesh.Halfedges;
            int nhe = hedges.Count;

            newVertexPositions = new Vec3d[nhe];

            // add one new vertex per halfedge
            for (int i = 0; i < nhe; i++)
            {
                var he = hedges[i];
                var f = he.Face;
                if (he.IsUnused || f == null) continue;

                int fi = f.Index;
                var p = vertexPositions[he.Start.Index];
                newVertexPositions[i] = Vec3d.Lerp(p, faceCenters[fi], faceFactors[fi]);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static HeMesh BevelEdgesTopo(HeMesh mesh, bool closeVertices, bool closeFaces)
        {
            var verts = mesh.Vertices;
            var hedges = mesh.Halfedges;
            var faces = mesh.Faces;

            int nhe = hedges.Count;
            int nv = verts.Count;
            int nf = faces.Count;

            var newMesh = new HeMesh(nhe, nhe << 2, nhe + nv + nf);
            var newVerts = newMesh.Vertices;
            var newFaces = newMesh.Faces;

            var fv = new List<Vertex>(4);
            fv.Fill();

            // add halfedge vertices (1 per halfedge in the original mesh)
            newVerts.Add(nhe);

            // add halfedge faces (1 per edge in the original mesh)
            for (int i = 0; i < nhe; i += 2)
            {
                var he0 = hedges[i];
                if (he0.IsUnused || he0.IsBoundary) continue;
                var he1 = he0.Twin;

                fv[0] = newVerts[he0.Next.Index];
                fv[1] = newVerts[he0.Index];
                fv[2] = newVerts[he1.Next.Index];
                fv[3] = newVerts[he1.Index];
                newFaces.AddImpl(fv);
            }

            // add vertex faces (1 per vertex in the original mesh)
            if (closeVertices)
            {
                for (int i = 0; i < nv; i++)
                {
                    var v = verts[i];
                    if (v.IsUnused || v.IsBoundary) continue;

                    // circulate vertex in reverse for consistent face windings
                    fv.Clear();
                    var he = v.FirstOut;
                    do
                    {
                        fv.Add(newVerts[he.Index]);
                        he = he.PrevInFace.Twin;
                    } while (he != v.FirstOut);

                    newFaces.AddImpl(fv);
                }
            }

            // add face faces (1 per face in the original mesh)
            if (closeFaces)
            {
                for (int i = 0; i < nf; i++)
                {
                    var f = faces[i];
                    if (f.IsUnused) continue;

                    // collect indices of face halfedges
                    fv.Clear();
                    foreach (var he in f.Halfedges)
                        fv.Add(newVerts[he.Index]);

                    newFaces.AddImpl(fv);
                }
            }

            return newMesh;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="facePoints"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="newVertexPositions"></param>
        /// <param name="closeVertices"></param>
        /// <returns></returns>
        public static HeMesh FrameDual(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> facePoints, double scaleFactor, out Vec3d[] newVertexPositions, bool closeVertices = true)
        {
            FrameDualGeom(mesh, vertexPositions, facePoints, scaleFactor, out newVertexPositions);
            return FrameDualTopo(mesh, closeVertices);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="facePoints"></param>
        /// <param name="faceFactors"></param>
        /// <param name="newVertexPositions"></param>
        /// <param name="closeVertices"></param>
        /// <returns></returns>
        public static HeMesh FrameDual(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> facePoints, IReadOnlyList<double> faceFactors, out Vec3d[] newVertexPositions, bool closeVertices = true)
        {
            FrameDualGeom(mesh, vertexPositions, facePoints, faceFactors, out newVertexPositions);
            return FrameDualTopo(mesh, closeVertices);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="facePoints"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="newVertexPositions"></param>
        private static void FrameDualGeom(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> facePoints, double scaleFactor, out Vec3d[] newVertexPositions)
        {
            var hedges = mesh.Halfedges;
            int nhe = hedges.Count;
            int nf = mesh.Faces.Count;

            newVertexPositions = new Vec3d[nf + nhe];

            // add one new vertex per face
            for (int i = 0; i < nf; i++)
                newVertexPositions[i] = facePoints[i];

            // add one new vertex per edge
            for (int i = 0; i < nhe; i++)
            {
                var he = hedges[i];
                var f = he.Face;
                if (he.IsUnused || f == null) continue;

                int fi = f.Index;
                var p = vertexPositions[he.Start.Index];
                newVertexPositions[i + nf] = Vec3d.Lerp(p, facePoints[fi], scaleFactor);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="facePoints"></param>
        /// <param name="faceFactors"></param>
        /// <param name="newVertexPositions"></param>
        private static void FrameDualGeom(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> facePoints, IReadOnlyList<double> faceFactors, out Vec3d[] newVertexPositions)
        {
            var hedges = mesh.Halfedges;
            int nhe = hedges.Count;
            int nf = mesh.Faces.Count;

            newVertexPositions = new Vec3d[nf + nhe];

            // add one new vertex per face
            for (int i = 0; i < nf; i++)
                newVertexPositions[i] = facePoints[i];

            // add one new vertex per edge
            for (int i = 0; i < nhe; i++)
            {
                var he = hedges[i];
                var f = he.Face;
                if (he.IsUnused || f == null) continue;

                int fi = f.Index;
                var p = vertexPositions[he.Start.Index];
                newVertexPositions[i + nf] = Vec3d.Lerp(p, facePoints[fi], faceFactors[fi]);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static HeMesh FrameDualTopo(HeMesh mesh, bool closeVertices)
        {
            var verts = mesh.Vertices;
            var hedges = mesh.Halfedges;
            var faces = mesh.Faces;

            int nhe = hedges.Count;
            int nv = verts.Count;
            int nf = faces.Count;

            var newMesh = new HeMesh(nhe, nhe * 5, nhe + nf);
            var newVerts = newMesh.Vertices;
            var newFaces = newMesh.Faces;

            var fv = new List<Vertex>(4);
            fv.Fill();

            // add face vertices (1 per face)
            newVerts.Add(nf);

            // add halfedge vertices (1 per halfedge)
            newVerts.Add(nhe);

            // add faces
            for (int i = 0; i < nhe; i += 2)
            {
                var he0 = hedges[i];
                if (he0.IsUnused || he0.IsBoundary) continue;
                var he1 = he0.Twin;

                var f0 = he0.Face;
                var f1 = he1.Face;

                fv[0] = newVerts[f0.Index];
                fv[1] = newVerts[he0.Index + nf];
                fv[2] = newVerts[he1.Next.Index + nf];
                fv[3] = newVerts[f1.Index];
                newFaces.AddImpl(fv);

                fv[0] = newVerts[f1.Index];
                fv[1] = newVerts[he1.Index + nf];
                fv[2] = newVerts[he0.Next.Index + nf];
                fv[3] = newVerts[f0.Index];
                newFaces.AddImpl(fv);
            }

            // add vertex faces (1 per vertex in the original mesh)
            if (closeVertices)
            {
                for (int i = 0; i < nv; i++)
                {
                    var v = verts[i];
                    if (v.IsUnused || v.IsBoundary) continue;

                    // circulate vertex in reverse for consistent face windings
                    fv.Clear();
                    var he = v.FirstOut;
                    do
                    {
                        fv.Add(newVerts[he.Index + nf]);
                        he = he.PrevInFace.Twin;
                    } while (he != v.FirstOut);

                    newFaces.AddImpl(fv);
                }
            }

            return newMesh;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faceCenters"></param>
        /// <param name="vertexNormals"></param>
        /// <param name="edgeValues"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static HeMesh Weave(HeMesh mesh, IReadOnlyList<Vec3d> faceCenters, IReadOnlyList<Vec3d> vertexNormals, IReadOnlyList<double> edgeValues, double offset)
        {
            // TODO implement
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="faceCenters"></param>
        /// <param name="faceNormals"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="offset"></param>
        /// <param name="newVertexPositions"></param>
        /// <returns></returns>
        public static HeMesh Weave(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> faceCenters, IReadOnlyList<Vec3d> faceNormals, double scaleFactor, double offset, out Vec3d[] newVertexPositions)
        {
            WeaveGeom(mesh, vertexPositions, faceCenters, faceNormals, scaleFactor, offset, out newVertexPositions);
            return WeaveTopo(mesh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexPositions"></param>
        /// <param name="faceCenters"></param>
        /// <param name="faceNormals"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="offset"></param>
        /// <param name="newVertexPositions"></param>
        private static void WeaveGeom(HeMesh mesh, IReadOnlyList<Vec3d> vertexPositions, IReadOnlyList<Vec3d> faceCenters, IReadOnlyList<Vec3d> faceNormals, double scaleFactor, double offset, out Vec3d[] newVertexPositions)
        {
            var hedges = mesh.Halfedges;
            int nhe = hedges.Count;

            newVertexPositions = new Vec3d[nhe << 2];

            // add vertices (8 per halfedge pair in m0)
            for (int i = 0; i < nhe; i += 2)
            {
                var he0 = hedges[i];
                var he1 = hedges[i + 1];
                var f0 = he0.Face;
                var f1 = he1.Face;

                Vec3d p00 = vertexPositions[he0.Start.Index];
                Vec3d p01 = vertexPositions[he1.Start.Index];
                Vec3d cen = Vec3d.Lerp(p00, p01, 0.5);

                Vec3d p0 = Vec3d.Lerp(cen, p00, scaleFactor);
                Vec3d p1 = (f0 == null) ? new Vec3d() : Vec3d.Lerp(cen, faceCenters[f0.Index], scaleFactor);
                Vec3d p2 = Vec3d.Lerp(cen, p01, scaleFactor);
                Vec3d p3 = (f1 == null) ? new Vec3d() : Vec3d.Lerp(cen, faceCenters[f1.Index], scaleFactor);

                int j = i << 2;

                // add verts
                if (he0.IsBoundary)
                {
                    newVertexPositions[j] = p0;
                    newVertexPositions[j + 1] = p1;
                    newVertexPositions[j + 2] = p0;
                    newVertexPositions[j + 3] = p1;

                    newVertexPositions[j + 4] = p2;
                    newVertexPositions[j + 5] = p3;
                    newVertexPositions[j + 6] = p2;
                    newVertexPositions[j + 7] = p3;
                }
                else
                {
                    Vec3d d = faceNormals[f0.Index] + faceNormals[f1.Index];
                    d *= offset / d.Length;

                    newVertexPositions[j] = p0 - d;
                    newVertexPositions[j + 1] = p1 - d;
                    newVertexPositions[j + 2] = p0 + d;
                    newVertexPositions[j + 3] = p1 + d;

                    newVertexPositions[j + 4] = p2 - d;
                    newVertexPositions[j + 5] = p3 - d;
                    newVertexPositions[j + 6] = p2 + d;
                    newVertexPositions[j + 7] = p3 + d;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static HeMesh WeaveTopo(HeMesh mesh)
        {
            var hedges = mesh.Halfedges;
            int nhe = hedges.Count;

            var newMesh = new HeMesh(nhe << 2, nhe << 3, nhe << 2);
            var newVerts = newMesh.Vertices;
            var newFaces = newMesh.Faces;

            // add vertices (8 per edge in initial mesh)
            for (int i = 0; i < nhe; i += 2)
                newVerts.Add(8);

            // add node faces
            for (int i = 0; i < nhe; i += 2)
            {
                var he0 = hedges[i];
                var he1 = hedges[i + 1];
                int j = i << 2;

                if (he0.Face == null)
                {
                    newFaces.Add(j, j + 5, j + 4);
                }
                else if (he1.Face == null)
                {
                    newFaces.Add(j + 4, j + 1, j);
                }
                else
                {
                    newFaces.Add(j + 5, j + 4, j + 1, j);
                    newFaces.Add(j + 7, j + 6, j + 3, j + 2);
                }
            }

            // add edge faces
            for (int i = 0; i < nhe; i++)
            {
                var he0 = hedges[i];
                if (he0.Face == null) continue;

                int j0 = i << 2;
                int j1 = he0.Next.Index << 2;

                if (he0.IsBoundary)
                {
                    if ((i & 1) == 0)
                        newFaces.Add(j0 + 1, j0 + 4, j1, j1 + 1); // even edges
                    else
                        newFaces.Add(j0 + 1, j0 - 4, j1, j1 + 1); // odd edges
                }
                else
                {
                    if ((i & 1) == 0)
                        newFaces.Add(j0 + 3, j0 + 6, j1, j1 + 1);  // even edges
                    else
                        newFaces.Add(j0 + 3, j0 - 2, j1, j1 + 1); // odd edges
                }
            }

            return newMesh;
        }
        */

        #endregion
    }
}
