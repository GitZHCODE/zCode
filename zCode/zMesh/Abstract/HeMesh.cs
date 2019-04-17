﻿
/*
 * Notes
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

using zCode.zCore;

namespace zCode.zMesh
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TV"></typeparam>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TF"></typeparam>
    [Serializable]
    public abstract class HeMesh<TV, TE, TF> : HeStructure<TV, TE, TF>
        where TV : HeMesh<TV, TE, TF>.Vertex
        where TE : HeMesh<TV, TE, TF>.Halfedge
        where TF : HeMesh<TV, TE, TF>.Face
    {
        #region Nested types

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public abstract class Vertex : HeVertex<TV, TE, TF>
        {
        }


        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public abstract class Halfedge : Halfedge<TV, TE, TF>
        {
            /// <summary>
            /// 
            /// </summary>
            internal void Bypass()
            {
                if (IsAtDegree1)
                {
                    Start.MakeUnused();
                    return;
                }

                var he = NextAtStart;
                Previous.MakeConsecutive(he);

                if (IsFirstAtStart)
                    Start.First = he;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public abstract class Face : HeFace<TV, TE, TF>
        {
        }

        #endregion
        
        
        /// <summary>Buffer used when adding a new face to the mesh. Avoids repeated allocation.</summary>
        private List<(TV, TE)> _addFaceBuffer = new List<(TV, TE)>(DefaultCapacity);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexCapacity"></param>
        /// <param name="hedgeCapacity"></param>
        /// <param name="faceCapacity"></param>
        public HeMesh(int vertexCapacity = DefaultCapacity, int hedgeCapacity = DefaultCapacity, int faceCapacity = DefaultCapacity)
            : base(vertexCapacity, hedgeCapacity, faceCapacity)
        {
        }
        

        /// <summary>
        /// Returns true if all halfedges have a face.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                for (int i = 0; i < Halfedges.Count; i += 2)
                {
                    var he = Halfedges[i];
                    if (!he.IsUnused && he.IsBoundary) return false;
                }

                return true;
            }
        }


        /// <summary>
        /// Returns the Euler number of the mesh.
        /// </summary>
        public int EulerNumber
        {
            get { return Vertices.Count - (Halfedges.Count >> 1) + Faces.Count; }
        }


        /// <summary>
        /// Removes all unused elements from the mesh.
        /// </summary>
        public void Compact()
        {
            Vertices.Compact();
            Halfedges.Compact();
            Faces.Compact();
        }


        /// <summary>
        /// 
        /// </summary>
        public void TrimExcess()
        {
            Vertices.TrimExcess();
            Halfedges.TrimExcess();
            Faces.TrimExcess();
        }
        

        /// <summary>
        /// Returns the number of holes in the mesh.
        /// </summary>
        /// <returns></returns>
        public int CountHoles()
        {
            int currTag = Halfedges.NextTag;
            int n = 0;

            for (int i = 0; i < Halfedges.Count; i++)
            {
                var he = Halfedges[i];
                if (he.IsUnused || he.Face != null || he.Tag == currTag) continue;

                do
                {
                    he.Tag = currTag;
                    he = he.Next;
                } while (he.Tag != currTag);

                n++;
            }

            return n;
        }


        /// <summary>
        /// Returns the first halfedge from each hole in the mesh.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TE> GetHoles()
        {
            int currTag = Halfedges.NextTag;

            for (int i = 0; i < Halfedges.Count; i++)
            {
                var he = Halfedges[i];
                if (he.IsUnused || he.Face != null || he.Tag == currTag) continue;

                do
                {
                    he.Tag = currTag;
                    he = he.Next;
                } while (he.Tag != currTag);

                yield return he;
            }
        }


        /// <summary>
        /// Returns the number of boundary vertices in the mesh.
        /// </summary>
        /// <returns></returns>
        public int CountBoundaryVertices()
        {
            int n = 0;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                if (!v.IsUnused && v.IsBoundary) n++;
            }

            return n;
        }


        /// <summary>
        /// Returns each boundary vertex in the mesh.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TV> GetBoundaryVertices()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                if (!v.IsUnused && v.IsBoundary) yield return v;
            }
        }


        /// <summary>
        /// Returns the number of vertices with multiple incident boundary edges.
        /// </summary>
        /// <returns></returns>
        public int CountNonManifoldVertices()
        {
            var n = 0;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                if (!v.IsUnused && !v.IsManifold) n++;
            }

            return n;
        }


        /// <summary>
        /// Returns each non-manifold vertex in the mesh.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TV> GetNonManifoldVertices()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                if (!v.IsUnused && !v.IsManifold) yield return v;
            }
        }


        /// <summary>
        /// Appends a deep copy of the given mesh to this mesh.
        /// </summary>
        /// <typeparam name="UV"></typeparam>
        /// <typeparam name="UE"></typeparam>
        /// <typeparam name="UF"></typeparam>
        /// <param name="other"></param>
        /// <param name="setVertex"></param>
        /// <param name="setHedge"></param>
        /// <param name="setFace"></param>
        public void Append<UV, UE, UF>(HeMesh<UV, UE, UF> other, Action<TV, UV> setVertex = null, Action<TE, UE> setHedge = null, Action<TF, UF> setFace = null)
            where UV : HeMesh<UV, UE, UF>.Vertex
            where UE : HeMesh<UV, UE, UF>.Halfedge
            where UF : HeMesh<UV, UE, UF>.Face
        {
            var otherVerts = other.Vertices;
            var otherHedges = other.Halfedges;
            var otherFaces = other.Faces;

            int nvA = Vertices.Count;
            int nhA = Halfedges.Count;
            int nfA = Faces.Count;

            // cache in case of appending to self
            int nvB = otherVerts.Count;
            int nhB = otherHedges.Count;
            int nfB = otherFaces.Count;

            // append new elements
            for (int i = 0; i < nvB; i++)
                AddVertex();

            for (int i = 0; i < nhB; i += 2)
                AddEdge();

            for (int i = 0; i < nfB; i++)
                AddFace();

            // link new vertices to new halfedges
            for (int i = 0; i < nvB; i++)
            {
                var v0 = otherVerts[i];
                var v1 = Vertices[i + nvA];

                // transfer attributes
                setVertex?.Invoke(v1, v0);

                if (v0.IsUnused) continue;
                v1.First = Halfedges[v0.First.Index + nhA];
            }

            // link new faces to new halfedges
            for (int i = 0; i < nfB; i++)
            {
                var f0 = otherFaces[i];
                var f1 = Faces[i + nfA];

                // transfer attributes
                setFace?.Invoke(f1, f0);

                if (f0.IsUnused) continue;
                f1.First = Halfedges[f0.First.Index + nhA];
            }

            // link new halfedges to eachother, new vertices, and new faces
            for (int i = 0; i < nhB; i++)
            {
                var he0 = otherHedges[i];
                var he1 = Halfedges[i + nhA];

                // transfer attributes
                setHedge?.Invoke(he1, he0);

                if (he0.IsUnused) continue;
                he1.Previous = Halfedges[he0.Previous.Index + nhA];
                he1.Next = Halfedges[he0.Next.Index + nhA];
                he1.Start = Vertices[he0.Start.Index + nvA];

                if (he0.Face != null)
                    he1.Face = Faces[he0.Face.Index + nfA];
            }
        }


        /// <summary>
        /// Appends the dual of the given mesh to this mesh.
        /// Note this method preserves indexical correspondance between primal and dual elements.
        /// </summary>
        /// <typeparam name="UV"></typeparam>
        /// <typeparam name="UE"></typeparam>
        /// <typeparam name="UF"></typeparam>
        /// <param name="other"></param>
        /// <param name="setVertex"></param>
        /// <param name="setHedge"></param>
        /// <param name="setFace"></param>
        public void AppendDual<UV, UE, UF>(HeMesh<UV, UE, UF> other, Action<TV, UF> setVertex = null, Action<TE, UE> setHedge = null, Action<TF, UV> setFace = null)
            where UV : HeMesh<UV, UE, UF>.Vertex
            where UE : HeMesh<UV, UE, UF>.Halfedge
            where UF : HeMesh<UV, UE, UF>.Face
        {
            var vertsB = other.Vertices;
            var hedgesB = other.Halfedges;
            var facesB = other.Faces;

            int nvA = Vertices.Count;
            int nhA = Halfedges.Count;
            int nfA = Faces.Count;

            // cache in case of appending to self
            int nvB = vertsB.Count;
            int nhB = hedgesB.Count;
            int nfB = facesB.Count;

            // add new elements
            for (int i = 0; i < nfB; i++)
                AddVertex();

            for (int i = 0; i < nvB; i++)
                AddFace();

            // add halfedge pairs and set their face/vertex refs
            // spins each halfedge such that its face in the primal mesh corresponds with its start vertex in the dual
            for (int i = 0; i < nhB; i += 2)
            {
                var heA0 = AddEdge();
                var heB0 = hedgesB[i];
                if (heB0.IsUnused || heB0.IsBoundary) continue; // skip boundary edges

                var heB1 = hedgesB[i + 1];
                var vB0 = heB0.Start;
                var vB1 = heB1.Start;

                var b0 = vB0.IsBoundary;
                var b1 = vB1.IsBoundary;
                if (b0 && b1) continue; // skip invalid dual edges

                var heA1 = heA0.Twin;
                heA0.Start = Vertices[heB0.Face.Index + nvA];
                heA1.Start = Vertices[heB1.Face.Index + nvA];

                if (!b0) heA1.Face = Faces[vB0.Index + nfA]; // vB0 is interior
                if (!b1) heA0.Face = Faces[vB1.Index + nfA]; // vB1 is interior
            }

            // set halfedge -> halfedge refs
            for (int i = 0; i < nhB; i++)
            {
                var heA0 = Halfedges[i + nhA];
                var heB0 = hedgesB[i];

                // transfer attributes
                setHedge?.Invoke(heA0, heB0);

                // invalid dual edges have a null start vertex
                if (heA0.Start == null)
                    continue;

                var heB1 = heB0.Next;
                var heA1 = Halfedges[heB1.Index + nhA];

                // backtrack around primal face, until dual edge is valid
                while (heA1.Start == null)
                {
                    heB1 = heB1.Next;
                    heA1 = Halfedges[heB1.Index + nhA];
                }

                heA1.Twin.MakeConsecutive(heA0);
            }

            // set dual face -> halfedge refs 
            // must be set before vertex refs to check for boundary invariant
            for (int i = 0; i < nvB; i++)
            {
                var fA = Faces[i + nfA];
                var vB = vertsB[i];

                // transfer attributes
                setFace?.Invoke(fA, vB);

                if (vB.IsUnused || vB.IsBoundary) continue;
                fA.First = Halfedges[vB.First.Twin.Index + nhA]; // can assume dual edge around interior vertex is valid
            }

            // set dual vertex -> halfedge refs
            for (int i = 0; i < nfB; i++)
            {
                var vA = Vertices[i + nvA];
                var fB = facesB[i];

                // transfer attributes
                setVertex?.Invoke(vA, fB);

                if (fB.IsUnused) continue;
                var heB = fB.First; // primal halfedge
                var heA = Halfedges[heB.Index + nhA]; // corresponding dual halfedge

                // find first used dual halfedge
                while (heA.IsUnused)
                {
                    heB = heB.Next;
                    if (heB == fB.First) goto EndFor; // dual vertex has no valid halfedges
                    heA = Halfedges[heB.Index + nhA];
                }

                vA.First = heA;
                vA.SetFirstToBoundary();

                EndFor:;
            }

            // cleanup any appended degree 2 faces
            for (int i = nfA; i < Faces.Count; i++)
            {
                var f = Faces[i];
                if (!f.IsUnused && f.IsDegree2)
                    CleanupDegree2Face(f.First);
            }
        }

        #region Element Operators

        /*
         * Notes
         * 
         * Elements operators only consider topology. 
         * Handling of element attributes is left to the user as this will vary between applications.
         */ 
        
        #region Edge Operators

        /// <summary>
        /// Creates a new pair of halfedges between the given vertices and add them to the list.
        /// Returns the halfedge starting from v0.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        internal TE AddEdge(TV v0, TV v1)
        {
            var he = AddEdge();
            he.Start = v0;
            he.Twin.Start = v1;
            return he;
        }


        /// <summary>
        /// Detatches the given edge and flags it for removal.
        /// Note that this method does not update face->halfedge refs
        /// </summary>
        /// <param name="hedge"></param>
        private void RemoveEdge(TE hedge)
        {
            hedge.Bypass();
            hedge.Twin.Bypass();
            hedge.MakeUnused();
        }


        /// <summary>
        /// Splits the given edge creating a new vertex and halfedge pair.
        /// Returns the new halfedge which starts at the new vertex.
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        public TE SplitEdge(TE hedge)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            return SplitEdgeImpl(hedge);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        internal TE SplitEdgeImpl(TE hedge)
        {
            return SplitEdgeImpl(hedge, AddVertex());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        internal TE SplitEdgeImpl(TE hedge, TV vertex)
        {
            var he0 = hedge;
            var he1 = he0.Twin;

            var v0 = vertex;
            var v1 = he1.Start;

            var he2 = AddEdge(v0, v1);
            var he3 = he2.Twin;

            // update halfedge->vertex refs
            he1.Start = v0;

            // update halfedge->face refs
            he2.Face = he0.Face;
            he3.Face = he1.Face;

            // update vertex->halfedge refs if necessary
            if (v1.First == he1)
            {
                v1.First = he3;
                v0.First = he1;
            }
            else
            {
                v0.First = he2;
            }

            // update halfedge->halfedge refs
            he2.MakeConsecutive(he0.Next);
            he0.MakeConsecutive(he2);
            he1.Previous.MakeConsecutive(he3);
            he3.MakeConsecutive(he1);

            return he2;
        }


        /// <summary>
        /// Inserts the specified number of vertices along the given edge.
        /// </summary>
        /// <param name="hedge"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public TE DivideEdge(TE hedge, int count)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            return DivideEdgeImpl(hedge, count);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private TE DivideEdgeImpl(TE hedge, int count)
        {
            for (int i = 0; i < count; i++)
                hedge = SplitEdgeImpl(hedge);

            return hedge;
        }


        /*
        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        /// <param name="halfedge"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        internal TE SplitEdgeImpl(TE halfedge, double t)
        {
            TE he0 = halfedge;
            TE he1 = he0.Twin;

            HeVertex v0 = he0.Start;
            HeVertex v1 = Parent.Vertices.Add(he0.PointAt(t));

            TE he2 = AddPair(v0, v1);
            TE he3 = he2.Twin;

            // update halfedge->vertex refs
            he0.Start = v1;

            // update halfedge->face refs
            he2.Face = he0.Face;
            he3.Face = he1.Face;

            // update vertex->halfedge refs if necessary
            if (v0.First == he0)
            {
                v0.First = he2;
                v1.First = he0;
            }
            else
            {
                v1.First = he3;
            }

            // update halfedge->halfedge refs
            Halfedge.MakeConsecutive(he0.Previous, he2);
            Halfedge.MakeConsecutive(he2, he0);
            Halfedge.MakeConsecutive(he3, he1.Next);
            Halfedge.MakeConsecutive(he1, he3);

            return he2;
        }
        */


        /// <summary>
        /// Splits an edge by adding a new vertex in the middle. 
        /// Faces adjacent to the given edge are also split at the new vertex.
        /// Returns the new halfedge outgoing from the new vertex or null on failure.
        /// Assumes triangle mesh.
        /// </summary>
        public TE SplitEdgeFace(TE hedge)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            return SplitEdgeFaceImpl(hedge);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        private TE SplitEdgeFaceImpl(TE hedge)
        {
            // split edge
            var he0 = SplitEdgeImpl(hedge);
            var he1 = he0.NextAtStart;

            // split left face if it exists
            if (he0.Face != null)
                SplitFaceImpl(he0, he0.Next.Next);

            // split right face if it exists
            if (he1.Face != null)
                SplitFaceImpl(he1, he1.Next.Next);

            return he0;
        }


        /// <summary>
        /// Collapses the given halfedge by merging the vertices at either end.
        /// If successful, the start vertex of the given halfedge is removed.
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        public bool CollapseEdge(TE hedge)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            return CollapseEdgeImpl(hedge);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        private bool CollapseEdgeImpl(TE hedge)
        {
            if (!CanCollapse(hedge))
                return false;

            var he0 = hedge; // to be removed
            var he1 = he0.Twin; // to be removed

            var v0 = he0.Start; // to be removed
            var v1 = he1.Start;

            var he2 = he0.Next;
            var he3 = he1.Next;

            var f0 = he0.Face;
            var f1 = he1.Face;

            // update start vertex of all halfedges starting at v0
            foreach (var he in he0.CirculateStart.Skip(1))
                he.Start = v1;

            // update outgoing halfedge from v1 if necessary
            var he4 = v0.First;

            if (he4.Face == null && he4 != he0)
                v1.First = he4;
            else if (v1.First == he1)
                v1.First = he3;

            // update halfedge-halfedge refs
            he0.Previous.MakeConsecutive(he2);
            he1.Previous.MakeConsecutive(he3);

            // update face->halfedge refs or handle collapsed faces/holes
            if (f0 == null)
            {
                if (he2.IsInDegree1)
                    CleanupDegree1Hole(he2);
            }
            else
            {
                if (he2.IsInDegree2)
                    CleanupDegree2Face(he2);
                else if (f0.First == he0)
                    f0.First = he2;
            }

            if (f1 == null)
            {
                if (he3.IsInDegree1)
                    CleanupDegree1Hole(he3);
            }
            else
            {
                if (he3.IsInDegree2)
                    CleanupDegree2Face(he3);
                else if (f1.First == he1)
                    f1.First = he3;
            }

            // flag elements for removal
            v0.MakeUnused();
            he0.MakeUnused();
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        private bool CanCollapse(TE hedge)
        {
            // avoids creation of non-manifold vertices
            // if (hedge.IsBridge) return false;

            var he0 = hedge;
            var he1 = hedge.PreviousAtStart;

            for (int i = 0; i < 2; i++)
            {
                he0 = he0.NextAtStart;
                if (he0 == he1) return true;
            }

            var he2 = hedge;
            var he3 = he2.Twin.Previous;

            for (int i = 0; i < 2; i++)
            {
                he2 = he2.Next.Twin;
                if (he2 == he3) return true;
            }

            // check for common vertices
            he0 = he0.Twin;
            he1 = he1.Twin;
            int currTag = Vertices.NextTag;

            // tag verts between he0 and he1
            do
            {
                he0.Start.Tag = currTag;
                he0 = he0.Next.Twin;
            } while (he0 != he1);

            // check for tags between he2 and he3
            do
            {
                if (he2.Start.Tag == currTag) return false;
                he2 = he2.Next.Twin;
            } while (he2 != he3);

            return true;
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        private bool CanCollapse(HeMeshHalfedge hedge)
        {
            var he0 = hedge;
            var he1 = hedge.Twin;

            int n = 0;
            if (he0.IsInDegree3) n++;
            if (he1.IsInDegree3) n++;

            return Owner.Vertices.CountCommonNeighboursImpl(he0.Start, he1.Start) == n;
        }
        */


        /// <summary>
        /// 
        /// </summary>
        private void CleanupDegree2Face(TE hedge)
        {
            var he0 = hedge; // to be removed
            var he1 = he0.Twin; // to be removed
            var he2 = he0.Next;
            var he3 = he1.Next;

            var v0 = he0.Start;
            var v1 = he1.Start;

            var f0 = he0.Face; // to be removed
            var f1 = he1.Face;

            // update vertex->halfedge refs
            if (v0.First == he0) v0.First = he3;
            if (v1.First == he1) v1.First = he2;

            // update face->halfedge refs
            if (f1 != null && f1.First == he1) f1.First = he2;

            // update halfedge->halfedge refs
            he1.Previous.MakeConsecutive(he2);
            he2.MakeConsecutive(he3);

            // update halfedge->face ref
            he2.Face = f1;

            // handle potential invalid edge
            if (!he2.IsManifold) RemoveEdge(he2);

            // flag for removal
            f0.MakeUnused();
            he0.MakeUnused();
        }


        /// <summary>
        /// 
        /// </summary>
        private void CleanupDegree2Hole(TE hedge)
        {
            var he0 = hedge; // to be removed
            var he1 = he0.Twin; // to be removed
            var he2 = he0.Next;
            var he3 = he1.Next;

            var v0 = he0.Start;
            var v1 = he1.Start;

            var f1 = he1.Face;

            // update vertex->halfedge refs
            // must look for another faceless halfedge to maintain boundary invariant for v0 and v1
            if (v0.First == he0)
            {
                var he = he0.NextBoundaryAtStart;
                v0.First = (he == null) ? he3 : he;
            }

            if (v1.First == he2)
            {
                var he = he2.NextBoundaryAtStart;
                v1.First = (he == null) ? he2 : he;
            }

            // update face->halfedge refs
            if (f1.First == he1) f1.First = he2;

            // update halfedge->face refs
            he2.Face = f1;

            // update halfedge->halfedge refs
            he1.Previous.MakeConsecutive(he2);
            he2.MakeConsecutive(he3);

            // flag elements for removal
            he0.MakeUnused();
        }


        /// <summary>
        ///
        /// </summary>
        private void CleanupDegree1Hole(TE hedge)
        {
            var he0 = hedge; // to be removed
            var he1 = he0.Twin; // to be removed

            var v0 = he0.Start;
            var f1 = he1.Face;

            // update vertex->halfedge refs
            // must look for another boundary halfedge to maintain boundary invariant for v0
            if (v0.First == he0)
            {
                var he = he0.NextBoundaryAtStart;
                v0.First = (he == null) ? he1.Next : he;
            }

            // update face->halfedge refs
            if (f1.First == he1) f1.First = he1.Next;

            // update halfedge->halfedge refs
            he1.Previous.MakeConsecutive(he1.Next);

            // flag elements for removal
            he0.MakeUnused();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        public bool SpinEdge(TE hedge)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            // halfedge must be adjacent to 2 faces
            if (hedge.IsBoundary)
                return false;

            // don't allow for the creation of valence 1 vertices
            if (hedge.IsAtDegree2 || hedge.Twin.IsAtDegree2)
                return false;

            SpinEdgeImpl(hedge);
            return true;
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        private void SpinEdgeImpl(TE hedge)
        {
            var he0 = hedge;
            var he1 = he0.Twin;

            var he2 = he0.Next;
            var he3 = he1.Next;

            var v0 = he0.Start;
            var v1 = he1.Start;

            // update vertex->halfedge refs if necessary
            if (v0.First == he0) v0.First = he3;
            if (v1.First == he1) v1.First = he2;

            // update halfedge->vertex refs
            he0.Start = he3.End;
            he1.Start = he2.End;

            var f0 = he0.Face;
            var f1 = he1.Face;

            // update face->halfedge refs if necessary
            if (he2 == f0.First) f0.First = he2.Next;
            if (he3 == f1.First) f1.First = he3.Next;

            // update halfedge->face refs
            he2.Face = f1;
            he3.Face = f0;

            // update halfedge->halfedge refs
            he0.MakeConsecutive(he2.Next);
            he1.MakeConsecutive(he3.Next);
            he1.Previous.MakeConsecutive(he2);
            he0.Previous.MakeConsecutive(he3);
            he2.MakeConsecutive(he1);
            he3.MakeConsecutive(he0);
        }


        /// <summary>
        /// Returns the new halfedge with the same orientation as the given one.
        /// The given halfedge will have a null face reference after this operation.
        /// </summary>
        /// <param name="hedge"></param>
        public TE DetachEdge(TE hedge)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            // halfedge must be adjacent to 2 faces
            if (hedge.IsBoundary)
                return null;

            return DetachEdgeImpl(hedge);
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        internal TE DetachEdgeImpl(TE hedge)
        {
            int mask = 0;
            if (hedge.Start.IsBoundary) mask |= 1;
            if (hedge.End.IsBoundary) mask |= 2;

            switch (mask)
            {
                case 0:
                    return DetachEdgeInterior(hedge);
                case 1:
                    return DetachEdgeAtStart(hedge);
                case 2:
                    return DetachEdgeAtEnd(hedge);
                case 3:
                    return DetachEdgeBoundary(hedge);
            }

            return null;
        }


        /// <summary>
        /// Assumes both start and end vertices are interior.
        /// </summary>
        private TE DetachEdgeInterior(TE hedge)
        {
            var he0 = hedge;
            var he1 = he0.Twin;

            var v0 = he0.Start;
            var v1 = he1.Start;

            var he2 = AddEdge(v1, v0);
            var he3 = he2.Twin;

            var f0 = he0.Face;

            // update halfedge-face refs
            he0.Face = he2.Face = null;
            he3.Face = f0;

            //update face-halfedge ref if necessary
            if (f0.First == he0)
                f0.First = he3;

            // update halfedge-halfedge refs
            he0.Previous.MakeConsecutive(he3);
            he3.MakeConsecutive(he0.Next);

            he0.MakeConsecutive(he2);
            he2.MakeConsecutive(he0);

            // update vertex-halfedge refs
            v0.First = he0;
            v1.First = he2;

            return he3;
        }


        /// <summary>
        /// Assumes both start and end vertices are on the mesh boundary.
        /// </summary>
        private TE DetachEdgeBoundary(TE hedge)
        {
            var he0 = hedge;
            var he1 = he0.Twin;

            var v0 = he0.Start;
            var v1 = he1.Start;

            var v2 = AddVertex();
            var v3 = AddVertex();

            var he2 = AddEdge(v3, v2);
            var he3 = he2.Twin;

            var he4 = v0.First;
            var he5 = v1.First;

            var f0 = he0.Face;

            // update halfedge-face refs
            he0.Face = he2.Face = null;
            he3.Face = f0;

            //update face-halfedge ref if necessary
            if (f0.First == he0)
                f0.First = he3;

            // update halfedge-halfedge refs
            he0.Previous.MakeConsecutive(he3);
            he3.MakeConsecutive(he0.Next);

            he4.Previous.MakeConsecutive(he0);
            he5.Previous.MakeConsecutive(he2);

            he0.MakeConsecutive(he5);
            he2.MakeConsecutive(he4);

            // update vertex-halfedge refs
            v0.First = he0;
            v2.First = he4;
            v3.First = he2;

            //update halfedge-vertex refs around each new vert
            foreach (var he in he2.CirculateStart.Skip(1))
                he.Start = v3;

            foreach (var he in he3.CirculateStart.Skip(1))
                he.Start = v2;

            return he3;
        }


        /// <summary>
        /// Assumes vertex at the start of the given halfedge is on the boundary.
        /// </summary>
        private TE DetachEdgeAtStart(TE hedge)
        {
            var he0 = hedge;
            var he1 = he0.Twin;

            var v0 = he0.Start;
            var v1 = he1.Start;
            var v2 = AddVertex();

            var he2 = AddEdge(v1, v2);
            var he3 = he2.Twin;
            var he4 = v0.First;

            var f0 = he0.Face;

            // update halfedge-face refs
            he0.Face = he2.Face = null;
            he3.Face = f0;

            //update face-halfedge ref if necessary
            if (f0.First == he0)
                f0.First = he3;

            // update halfedge-halfedge refs
            he0.Previous.MakeConsecutive(he3);
            he3.MakeConsecutive(he0.Next);

            he4.Previous.MakeConsecutive(he0);
            he0.MakeConsecutive(he2);
            he2.MakeConsecutive(he4);

            // update vertex-halfedge refs
            v0.First = he0;
            v1.First = he2;
            v2.First = he4;

            //update halfedge-vertex refs around each new vert
            foreach (var he in he3.CirculateStart.Skip(1))
                he.Start = v2;

            return he3;
        }


        /// <summary>
        /// Assumes vertex at the end of the given halfedge is on the boundary.
        /// </summary>
        private TE DetachEdgeAtEnd(TE hedge)
        {
            var he0 = hedge;
            var he1 = he0.Twin;

            var v0 = he0.Start;
            var v1 = he1.Start;
            var v2 = AddVertex();

            var he2 = AddEdge(v2, v0);
            var he3 = he2.Twin;
            var he4 = v1.First;

            var f0 = he0.Face;

            // update halfedge-face refs
            he0.Face = he2.Face = null;
            he3.Face = f0;

            //update face-halfedge ref if necessary
            if (f0.First == he0)
                f0.First = he3;

            // update halfedge-halfedge refs
            he0.Previous.MakeConsecutive(he3);
            he3.MakeConsecutive(he0.Next);

            he4.Previous.MakeConsecutive(he2);
            he2.MakeConsecutive(he0);
            he0.MakeConsecutive(he4);

            // update vertex-halfedge refs
            v0.First = he0;
            v2.First = he2;

            //update halfedge-vertex refs around each new vert
            foreach (var he in he2.CirculateStart.Skip(1))
                he.Start = v2;

            return he3;
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="he0"></param>
        /// <param name="he1"></param>
        /// <returns></returns>
        public bool MergeEdges(TE he0, TE he1)
        {
            he0.UsedCheck();
            he1.UsedCheck();
            OwnsCheck(he0);
            OwnsCheck(he1);

            // both halfedges must be on boundary
            if (he0.Face != null || he1.Face != null)
                return false;

            // can't merge edges which belong to the same face
            if (he0.Twin.Face == he1.Twin.Face)
                return false;

            // DEBUG doesn't consider edges 
            TE he2 = he0.Next;
            TE he3 = he1.Next;

            if (he2 == he1)
                ZipEdgeImpl(he0);
            else if (he1.Next == he0)
                ZipEdgeImpl(he1);
            else
                MergeEdgesImpl(he0, he1);

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        internal void MergeEdgesImpl(TE he0, TE he1)
        {
            TE he2 = he0.Twin;
            TE he3 = he1.Twin;

            HeVertex v0 = he0.Start;
            HeVertex v1 = he1.Start;
            HeVertex v2 = he2.Start;
            HeVertex v3 = he3.Start;

            HeFace f3 = he3.Face;

            // update vertex refs for all halfedges around v1
            foreach (TE he in he1.CirculateStart.Skip(1))
                he.Start = v2;

            // update vertex refs for all halfedges around v3
            foreach (TE he in he3.CirculateStart.Skip(1))
                he.Start = v0;

            // update vertex->halfedge refs
            v0.First = he1.Next;
            v1.First = he0.Next;

            // update halfedge->face refs
            if (f3.First == he3) f3.First = he0;
            he0.Face = f3;

            // update halfedge->halfedge refs
            Halfedge.MakeConsecutive(he1.Previous, he0.Next);
            Halfedge.MakeConsecutive(he0.Previous, he1.Next);
            Halfedge.MakeConsecutive(he0, he3.Next);
            Halfedge.MakeConsecutive(he3.Previous, he0);

            // flag elements for removal
            he1.MakeUnused();
            he3.MakeUnused();
            v1.MakeUnused();
            v3.MakeUnused();
        }


          /// <summary>
          /// 
          /// </summary>
          /// <param name="halfedge"></param>
          public bool ZipEdge(TE halfedge)
          {
              halfedge.UsedCheck();
              OwnsCheck(halfedge);

              // halfedge must be on boundary
              if (halfedge.Face != null)
                  return false;

              // can't zip from valence 2 vertex
              if (halfedge.Next.IsFromDegree2)
                  return false;

              ZipEdgeImpl(halfedge);
              return true;
          }


          /// <summary>
          /// 
          /// </summary>
          /// <param name="he0"></param>
          internal void ZipEdgeImpl(TE halfedge)
          {
              TE he0 = halfedge;
              TE he1 = he0.Next;
              TE he2 = he1.Twin;

              HeVertex v0 = he0.Start;
              HeVertex v1 = he1.Start;
              HeVertex v2 = he2.Start;

              HeFace f2 = he2.Face;

              // update vertex refs for all halfedges around v2
              foreach (TE he in he2.CirculateStart.Skip(1))
                  he.Start = v0;

              // update vertex->halfedge refs
              v0.First = he1.Next;
              TE he3 = he2.Next.FindBoundaryAtStart(); // check for another boundary edge at v1
              v1.First = (he3 == he1) ? he0.Twin : he3;

              // update halfedge->face refs
              if (f2.First == he2) f2.First = he0;
              he0.Face = f2;

              // update halfedge->halfedge refs
              Halfedge.MakeConsecutive(he0.Previous, he1.Next);
              Halfedge.MakeConsecutive(he2.Previous, he0);
              Halfedge.MakeConsecutive(he0, he2.Next);

              // flag elements as unused
              he1.MakeUnused();
              he2.MakeUnused();
              v2.MakeUnused();
          }
          */

        #endregion


        #region Vertex Operators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quantity"></param>
        public void AddVertices(int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                var v = NewVertex();
                Vertices.Add(v);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public void RemoveVertex(TV vertex)
        {
            vertex.UnusedCheck();
            Vertices.OwnsCheck(vertex);

            RemoveVertexImpl(vertex);
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private void RemoveVertexImpl(TV vertex)
        {
            var he = vertex.First;
            int n = vertex.Degree;

            for (int i = 0; i < n; i++)
            {
                if (!he.IsHole) RemoveFaceImpl(he.Face);
                he = he.Twin.Next;
            }
        }


        /// <summary>
        /// Merges a pair of boundary vertices.
        /// The first vertex is flagged as unused.
        /// Note that this method may produce non-manifold vertices.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public bool MergeVertices(TV v0, TV v1)
        {
            v0.UnusedCheck();
            v1.UnusedCheck();

            Vertices.OwnsCheck(v0);
            Vertices.OwnsCheck(v1);

            if (v0 == v1)
                return false;

            if (!v0.IsBoundary || !v1.IsBoundary)
                return false;

            return MergeVerticesImpl(v0, v1);
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        private bool MergeVerticesImpl(TV v0, TV v1)
        {
            var he0 = v0.First;
            var he1 = v1.First;
            var he2 = he0.Previous;
            var he3 = he1.Previous;

            // if vertices are connected, just collapse the edge between them
            if (he0 == he3)
                return CollapseEdgeImpl(he0);
            else if (he1 == he2)
                return CollapseEdgeImpl(he1);

            // update halfedge->vertex refs for all edges emanating from v1
            foreach (var he in v0.OutgoingHalfedges)
                he.Start = v1;

            // update halfedge->halfedge refs
            he3.MakeConsecutive(he0);
            he2.MakeConsecutive(he1);

            // deal with potential collapse of boundary loops on either side of the merge
            if (he1.Next == he2)
                CleanupDegree2Hole(he1);

            if (he0.Next == he3)
                CleanupDegree2Hole(he0);

            // flag elements for removal
            v0.MakeUnused();
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="he0"></param>
        /// <param name="he1"></param>
        public void DetachVertex(TE he0, TE he1)
        {
            // TODO implement
            throw new NotImplementedException();
        }


        /// <summary>
        /// Splits a vertex in 2 connected by a new edge.
        /// Returns the new halfedge leaving the new vertex on success and null on failure.
        /// </summary>
        /// <param name="he0"></param>
        /// <param name="he1"></param>
        /// <returns></returns>
        public TE SplitVertex(TE he0, TE he1)
        {
            he0.UnusedCheck();
            he1.UnusedCheck();

            Halfedges.OwnsCheck(he0);
            Halfedges.OwnsCheck(he1);

            if (he0.Start != he1.Start)
                return null;

            return SplitVertexImpl(he0, he1);
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        /// <param name="he0"></param>
        /// <param name="he1"></param>
        /// <returns></returns>
        private TE SplitVertexImpl(TE he0, TE he1)
        {
            // if the same edge or vertex is degree 2 then just split the edge
            if (he0 == he1 || he0.Twin.Next == he1)
                return SplitEdgeImpl(he0.Twin);

            var v0 = he0.Start;
            var v1 = AddVertex();

            var he2 = AddEdge(v0, v1);
            var he3 = he2.Twin;

            // update halfedge->face refs
            he2.Face = he0.Face;
            he3.Face = he1.Face;

            // update start vertex of all outoging edges between he0 and he1
            var he = he0;
            do
            {
                he.Start = v1;
                he = he.Twin.Next;
            } while (he != he1);

            // update vertex->halfedge refs if necessary
            if (v0.First.Start == v1)
            {
                // if v0's outgoing halfedge now starts at v1, can assume v1 is now on the boundary if v0 was originally
                v1.First = v0.First;
                v0.First = he2;
            }
            else
            {
                v1.First = he3;
            }

            // update halfedge->halfedge refs
            he0.Previous.MakeConsecutive(he2);
            he2.MakeConsecutive(he0);
            he1.Previous.MakeConsecutive(he3);
            he3.MakeConsecutive(he1);

            return he3;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertex"></param>
        public void ChamferVertex(TV vertex)
        {
            vertex.UnusedCheck();
            Vertices.OwnsCheck(vertex);

            ChamferVertexImpl(vertex);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertex"></param>
        private void ChamferVertexImpl(TV vertex)
        {
            // TODO implement
            throw new NotImplementedException();
        }

        #endregion


        #region Face Operators
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vi0"></param>
        /// <param name="vi1"></param>
        /// <param name="vi2"></param>
        /// <returns></returns>
        public TF AddFace(int vi0, int vi1, int vi2)
        {
            return AddFace(Yield());

            IEnumerable<int> Yield()
            {
                yield return vi0;
                yield return vi1;
                yield return vi2;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vi0"></param>
        /// <param name="vi1"></param>
        /// <param name="vi2"></param>
        /// <param name="vi3"></param>
        /// <returns></returns>
        public TF AddFace(int vi0, int vi1, int vi2, int vi3)
        {
            return AddFace(Yield());

            IEnumerable<int> Yield()
            {
                yield return vi0;
                yield return vi1;
                yield return vi2;
                yield return vi3;
            }
        }


        /// <summary>
        /// Adds a new face to the mesh.
        /// </summary>
        /// <param name="vertexIndices"></param>
        /// <returns></returns>
        public TF AddFace(IEnumerable<int> vertexIndices)
        {
            int currTag = Vertices.NextTag;

            // validate vertices and collect in buffer
            foreach (var vi in vertexIndices)
            {
                var v = Vertices[vi];

                if (v.Tag == currTag)
                {
                    _addFaceBuffer.Clear();
                    return null;
                }

                _addFaceBuffer.Add((v, null));
                v.Tag = currTag;
            }

            // no degenerate faces allowed
            if (_addFaceBuffer.Count < 3)
            {
                _addFaceBuffer.Clear();
                return null;
            }

            return AddFace(_addFaceBuffer);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public TF AddFace(TV v0, TV v1, TV v2)
        {
            return AddFace(Yield());

            IEnumerable<TV> Yield()
            {
                yield return v0;
                yield return v1;
                yield return v2;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public TF AddFace(TV v0, TV v1, TV v2, TV v3)
        {
            return AddFace(Yield());

            IEnumerable<TV> Yield()
            {
                yield return v0;
                yield return v1;
                yield return v2;
                yield return v3;
            }
        }


        /// <summary>
        /// Adds a new face to the mesh.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public TF AddFace(IEnumerable<TV> vertices)
        {
            int currTag = Vertices.NextTag;

            // validate vertices and collect in buffer
            foreach (var v in vertices)
            {
                Vertices.OwnsCheck(v);

                if (v.Tag == currTag)
                {
                    _addFaceBuffer.Clear();
                    return null;
                }

                _addFaceBuffer.Add((v, null));
                v.Tag = currTag;
            }

            // no degenerate faces allowed
            if (_addFaceBuffer.Count < 3)
            {
                _addFaceBuffer.Clear();
                return null;
            }
            
            return AddFace(_addFaceBuffer);
        }
        

        /// <summary>
        /// http://pointclouds.org/blog/nvcs/martin/index.php
        /// </summary>
        /// <param name="faceVerts"></param>
        /// <returns></returns>
        private TF AddFace(List<(TV Vertex, TE Halfedge)> buffer)
        {
            int n = _addFaceBuffer.Count;

            // collect all existing halfedges in the new face
            for (int i = 0; i < n; i++)
            {
                var v = buffer[i].Vertex;
                if (v.IsUnused) continue;

                // can't create a new face with an interior vertex
                if (!v.IsBoundary)
                {
                    buffer.Clear();
                    return null;
                }

                // search for an existing halfedge between consecutive vertices
                var he = v.FindHalfedgeTo(buffer[(i + 1) % n].Vertex);
                if (he == null) continue; // no halfedge found

                // can't create a new face if the halfedge already has one
                if (he.Face != null)
                {
                    buffer.Clear();
                    return null;
                }

                buffer[i] = (v, he);
            }

            /*
            // avoids creation of non-manifold vertices
            // if two consecutive new halfedges share a used vertex then that vertex will be non-manifold upon adding the face
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                if (faceHedges[i] == null && faceHedges[j] == null && !faceVerts[j].IsUnused) 
                {
                    buffer.Clear();
                    return null;
                }
            }
            */

            // create the new face
            var newFace = AddFace();

            // create any missing halfedge pairs in the face loop and assign the new face
            for (int i = 0; i < n; i++)
            {
                (var v, var he) = buffer[i];

                // if missing a halfedge, add a pair between consecutive vertices
                if (he == null)
                {
                    he = AddEdge(v, buffer[(i + 1) % n].Vertex);
                    buffer[i] = (v, he);
                }

                he.Face = newFace;
            }

            // link consecutive halfedges
            for (int i = 0; i < n; i++)
            {
                (var v0, var he0) = buffer[i];
                (var v1, var he1) = buffer[(i + 1) % n];

                var he2 = he0.Next;
                var he3 = he1.Previous;
                var he4 = he0.Twin;

                // check if halfedges are newly created
                // new halfedges will have null previous or next refs
                int mask = 0;
                if (he2 == null) mask |= 1; // e0 is new
                if (he3 == null) mask |= 2; // e1 is new

                // 0 - neither halfedge is new
                // 1 - he0 is new, he1 is old
                // 2 - he1 is new, he0 is old
                // 3 - both halfedges are new
                switch (mask)
                {
                    case 0:
                        {
                            // neither halfedge is new
                            // if he0 and he1 aren't consecutive, then deal with non-manifold vertex as per http://www.pointclouds.org/blog/nvcs/
                            // otherwise, update the first halfedge at v1
                            if (he2 != he1)
                            {
                                var he = he1.NextBoundaryAtStart; // find the next boundary halfedge around v1 (must exist if halfedges aren't consecutive)
                                v1.First = he;

                                he.Previous.MakeConsecutive(he2);
                                he3.MakeConsecutive(he);
                                he0.MakeConsecutive(he1);
                            }
                            else
                            {
                                v1.SetFirstToBoundary();
                            }

                            break;
                        }
                    case 1:
                        {
                            // he0 is new, he1 is old
                            he3.MakeConsecutive(he4);
                            v1.First = he4;
                            goto default;
                        }
                    case 2:
                        {
                            // he1 is new, he0 is old
                            he1.Twin.MakeConsecutive(he2);
                            goto default;
                        }
                    case 3:
                        {
                            // both halfedges are new
                            // deal with non-manifold case if v1 is already in use
                            if (v1.IsUnused)
                            {
                                he1.Twin.MakeConsecutive(he4);
                            }
                            else
                            {
                                v1.First.Previous.MakeConsecutive(he4);
                                he1.Twin.MakeConsecutive(v1.First);
                            }

                            v1.First = he4;
                            goto default;
                        }
                    default:
                        {
                            he0.MakeConsecutive(he1); // update refs for inner halfedges
                            break;
                        }
                }
            }

            newFace.First = buffer[0].Halfedge; // set first halfedge in the new face
            buffer.Clear();

            return newFace;
        }
        

        #if false
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vi0"></param>
        /// <param name="vi1"></param>
        /// <param name="vi2"></param>
        /// <returns></returns>
        public TF AddFace(int vi0, int vi1, int vi2)
        {
            return AddFace(Yield());

            IEnumerable<int> Yield()
            {
                yield return vi0;
                yield return vi1;
                yield return vi2;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vi0"></param>
        /// <param name="vi1"></param>
        /// <param name="vi2"></param>
        /// <param name="vi3"></param>
        /// <returns></returns>
        public TF AddFace(int vi0, int vi1, int vi2, int vi3)
        {
            return AddFace(Yield());

            IEnumerable<int> Yield()
            {
                yield return vi0;
                yield return vi1;
                yield return vi2;
                yield return vi3;
            }
        }


        /// <summary>
        /// Adds a new face to the mesh.
        /// </summary>
        /// <param name="vertexIndices"></param>
        /// <returns></returns>
        public TF AddFace(IEnumerable<int> vertexIndices)
        {
            int currTag = Vertices.NextTag;
            int n = 0;

            // check for duplicates
            foreach (var vi in vertexIndices)
            {
                var v = Vertices[vi];
                if (v.Tag == currTag) return null;
                v.Tag = currTag;
                n++;
            }

            // no degenerate faces allowed
            if (n < 3)
                return null;

            var faceVerts = new TV[n];
            vertexIndices.Select(vi => Vertices[vi]).ToArray(faceVerts);
            return AddFaceImpl(faceVerts);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public TF AddFace(TV v0, TV v1, TV v2)
        {
            return AddFace(Yield());

            IEnumerable<TV> Yield()
            {
                yield return v0;
                yield return v1;
                yield return v2;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public TF AddFace(TV v0, TV v1, TV v2, TV v3)
        {
            return AddFace(Yield());

            IEnumerable<TV> Yield()
            {
                yield return v0;
                yield return v1;
                yield return v2;
                yield return v3;
            }
        }


        /// <summary>
        /// Adds a new face to the mesh.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public TF AddFace(IEnumerable<TV> vertices)
        {
            int currTag = Vertices.NextTag;
            int n = 0;

            // validate vertices and check for duplicates
            foreach (var v in vertices)
            {
                Vertices.ContainsCheck(v);
                if (v.Tag == currTag) return null;
                v.Tag = currTag;
                n++;
            }

            // no degenerate faces allowed
            if (n < 3)
                return null;

            var faceVerts = new TV[n];
            vertices.ToArray(faceVerts);

            return AddFaceImpl(faceVerts);
        }


        /// <summary>
        /// http://pointclouds.org/blog/nvcs/martin/index.php
        /// </summary>
        /// <param name="faceVerts"></param>
        /// <returns></returns>
        private TF AddFaceImpl(TV[] faceVerts)
        {
            int n = faceVerts.Length;
            var faceHedges = new TE[n];

            // collect all existing halfedges in the new face
            for (int i = 0; i < n; i++)
            {
                var v = faceVerts[i];
                if (v.IsUnused) continue;

                // can't create a new face with an interior vertex
                if (!v.IsBoundary) return null;

                // search for an existing halfedge between consecutive vertices
                var he = v.FindHalfedge(faceVerts[(i + 1) % n]);
                if (he == null) continue; // no existing halfedge

                // can't create a new face if the halfedge already has one
                if (he.Face != null) return null;
                faceHedges[i] = he;
            }

            /*
            // avoids creation of non-manifold vertices
            // if two consecutive new halfedges share a used vertex then that vertex will be non-manifold upon adding the face
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                if (faceHedges[i] == null && faceHedges[j] == null && !faceVerts[j].IsUnused) 
                    return null;
            }
            */

            // create the new face
            var newFace = AddFace();

            // create any missing halfedge pairs in the face loop and assign the new face
            for (int i = 0; i < n; i++)
            {
                var he = faceHedges[i];

                // if missing a halfedge, add a pair between consecutive vertices
                if (he == null)
                {
                    he = AddEdge(faceVerts[i], faceVerts[(i + 1) % n]);
                    faceHedges[i] = he;
                }

                he.Face = newFace;
            }

            // link consecutive halfedges
            for (int i = 0; i < n; i++)
            {
                var he0 = faceHedges[i];
                var he1 = faceHedges[(i + 1) % n];

                var he2 = he0.Next;
                var he3 = he1.Previous;
                var he4 = he0.Twin;

                var v0 = he0.Start;
                var v1 = he1.Start;

                // check if halfedges are newly created
                // new halfedges will have null previous or next refs
                int mask = 0;
                if (he2 == null) mask |= 1; // e0 is new
                if (he3 == null) mask |= 2; // e1 is new

                // 0 - neither halfedge is new
                // 1 - he0 is new, he1 is old
                // 2 - he1 is new, he0 is old
                // 3 - both halfedges are new
                switch (mask)
                {
                    case 0:
                        {
                            // neither halfedge is new
                            // if he0 and he1 aren't consecutive, then deal with non-manifold vertex as per http://www.pointclouds.org/blog/nvcs/
                            // otherwise, update the first halfedge at v1
                            if (he2 != he1)
                            {
                                var he = he1.NextBoundaryAtStart; // find the next boundary halfedge around v1 (must exist if halfedges aren't consecutive)
                                v1.First = he;

                                he.Previous.MakeConsecutive(he2);
                                he3.MakeConsecutive(he);
                                he0.MakeConsecutive(he1);
                            }
                            else
                            {
                                v1.SetFirstToBoundary();
                            }

                            break;
                        }
                    case 1:
                        {
                            // he0 is new, he1 is old
                            he3.MakeConsecutive(he4);
                            v1.First = he4;
                            goto default;
                        }
                    case 2:
                        {
                            // he1 is new, he0 is old
                            he1.Twin.MakeConsecutive(he2);
                            goto default;
                        }
                    case 3:
                        {
                            // both halfedges are new
                            // deal with non-manifold case if v1 is already in use
                            if (v1.IsUnused)
                            {
                                he1.Twin.MakeConsecutive(he4);
                            }
                            else
                            {
                                v1.First.Previous.MakeConsecutive(he4);
                                he1.Twin.MakeConsecutive(v1.First);
                            }

                            v1.First = he4;
                            goto default;
                        }
                    default:
                        {
                            he0.MakeConsecutive(he1); // update refs for inner halfedges
                            break;
                        }
                }
            }

            newFace.First = faceHedges[0]; // set first halfedge in the new face
            return newFace;
        }
        #endif


        /// <summary>
        /// Removes a face from the mesh as well as any invalid elements created in the process.
        /// Returns true on success.
        /// </summary>
        /// <param name="face"></param>
        public void RemoveFace(TF face)
        {
            face.UnusedCheck();
            Faces.OwnsCheck(face);

            RemoveFaceImpl(face);
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private void RemoveFaceImpl(TF face)
        {
            /*
            // avoids creation of non-manifold vertices
            foreach (HeVertex v in face.Vertices)
                if (v.IsBoundary && v.First.Twin.Face != face) return false;
            */

            // update halfedge->face refs
            var he = face.First;
            do
            {
                if (he.Twin.Face == null)
                {
                    RemoveEdge(he);
                }
                else
                {
                    he.Start.First = he;
                    he.Face = null;
                }

                he = he.Next;
            } while (he.Face == face);

            // flag for removal
            face.MakeUnused();
        }


        /// <summary>
        /// Removes a halfedge pair, merging their two adajcent faces.
        /// The face of the given halfedge is removed.
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        public bool MergeFaces(TE hedge)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            return MergeFacesImpl(hedge);
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        private bool MergeFacesImpl(TE hedge)
        {
            if (hedge.IsHole)
                return MergeHoleToFace(hedge);
            else if (hedge.Twin.IsHole)
                return MergeFaceToHole(hedge);
            else
                return MergeFaceToFace(hedge);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        private bool MergeFaceToFace(TE hedge)
        {
            var he0 = hedge;
            var he1 = hedge;

            var f1 = hedge.Twin.Face;

            // backtrack to previous non-shared halfedge in f0
            do
            {
                he0 = he0.Previous;
                if (he0 == hedge) return false; // all edges in f0 are shared with f1, can't merge
            } while (he0.Twin.Face == f1);

            // advance to next non-shared halfedge in f0
            do
            {
                he1 = he1.Next;
            } while (he1.Twin.Face == f1);

            // ensure single string of shared edges between f0 and f1
            {
                var he = he1;
                do
                {
                    if (he.Twin.Face == f1) return false; // multiple strings of shared edges detected, can't merge
                    he = he.Next;
                } while (he != he0);
            }

            // advance to first shared halfedge
            he0 = he0.Next;

            // update halfedge->face refs
            {
                var he = he1;
                do
                {
                    he.Face = f1;
                    he = he.Next;
                } while (he != he0);
            }

            // remove shared edges
            {
                var he = he0;
                do
                {
                    RemoveEdge(he);
                    he = he.Next;
                } while (he != he1);
            }

            // update face->halfedge ref if necessary
            if (f1.First.IsUnused) f1.First = he1;

            // flag face as unused
            he0.Face.MakeUnused();
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        private bool MergeFaceToHole(TE hedge)
        {
            RemoveFaceImpl(hedge.Face);
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        private bool MergeHoleToFace(TE hedge)
        {
            var he0 = hedge; // has null face
            var he1 = hedge;

            var f1 = hedge.Twin.Face;

            // backtrack to previous non-shared halfedge in f0
            do
            {
                he0 = he0.Previous;
                if (he0 == hedge) return false; // all edges in f0 are shared with f1, can't merge
            } while (he0.Twin.Face == f1);

            // advance to next non-shared halfedge in f0
            do
            {
                he1 = he1.Next;
            } while (he1.Twin.Face == f1);

            // ensure single string of shared edges between f0 and f1
            {
                var he = he1;
                do
                {
                    if (he.Twin.Face == f1) return false; // multiple strings of shared edges detected, can't merge
                    he = he.Next;
                } while (he != he0);
            }

            // advance to first shared halfedge
            he0 = he0.Next;

            // update halfedge->face refs and vertex->halfedge refs if necessary
            {
                var he = he1;
                do
                {
                    he.Face = f1;
                    he.Start.SetFirstToBoundary();
                    he = he.Next;
                } while (he != he0);
            }

            // remove shared edges
            {
                var he = he0;
                do
                {
                    RemoveEdge(he);
                    he = he.Next;
                } while (he != he1);
            }

            // update face->halfedge ref if necessary
            if (f1.First.IsUnused) f1.First = he1;

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        public TF FillHole(TE hedge)
        {
            hedge.UnusedCheck();
            Halfedges.OwnsCheck(hedge);

            // halfedge must be in a hole with at least 3 edges
            if (!hedge.IsHole && hedge.IsInDegree2)
                return null;

            return FillHoleImpl(hedge);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hedge"></param>
        /// <returns></returns>
        private TF FillHoleImpl(TE hedge)
        {
            var f = AddFace();
            f.First = hedge;

            foreach (var he0 in hedge.Circulate)
            {
                he0.Face = f;
                he0.Start.SetFirstToBoundary();
            }

            return f;
        }


        /// <summary>
        /// Splits a face by creating a new halfedge pair between the start vertices of the given halfedges.
        /// Returns the new halfedge that shares a start vertex with he0.
        /// </summary>
        /// <param name="he0"></param>
        /// <param name="he1"></param>
        /// <returns></returns>
        public TE SplitFace(TE he0, TE he1)
        {
            he0.UnusedCheck();
            he1.UnusedCheck();

            Halfedges.OwnsCheck(he0);
            Halfedges.OwnsCheck(he1);

            // halfedges must be on the same face which can't be null
            if (he0.Face == null || he0.Face != he1.Face)
                return null;

            // halfedges can't be consecutive
            if (he0.Next == he1 || he1.Next == he0)
                return null;

            return SplitFaceImpl(he0, he1);
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        /// <param name="he0"></param>
        /// <param name="he1"></param>
        /// <returns></returns>
        internal TE SplitFaceImpl(TE he0, TE he1)
        {
            var f0 = he0.Face;
            var f1 = AddFace();

            var he2 = AddEdge(he0.Start, he1.Start);
            var he3 = he2.Twin;

            // set halfedge->face refs
            he3.Face = f0;
            he2.Face = f1;

            // set new halfedges as first in respective faces
            f0.First = he3;
            f1.First = he2;

            // update halfedge->halfedge refs
            he0.Previous.MakeConsecutive(he2);
            he1.Previous.MakeConsecutive(he3);
            he3.MakeConsecutive(he0);
            he2.MakeConsecutive(he1);

            // update face references of all halfedges in new loop
            var he = he2.Next;
            do
            {
                he.Face = f1;
                he = he.Next;
            } while (he != he2);

            return he2; // return halfedge adjacent to new face
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public TV PokeFace(TF face)
        {
            face.UnusedCheck();
            Faces.OwnsCheck(face);

            return PokeFaceImpl(face);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        internal TV PokeFaceImpl(TF face)
        {
            var vc = AddVertex();
            PokeFaceImpl(face.First, vc);
            return vc;
        }


        /// <summary>
        /// Assumes the given elements are valid for the operation.
        /// </summary>
        internal void PokeFaceImpl(TE first, TV center)
        {
            var he = first;
            var v = first.Start;
            var f = first.Face;

            // create new halfedges and connect to existing ones
            do
            {
                var he0 = AddEdge(he.Start, center);
                he.Previous.MakeConsecutive(he0);
                he0.Twin.MakeConsecutive(he);
                he = he.Next;
            } while (he.Start != v);

            he = first; // reset to first halfedge in face
            center.First = he.Previous; // set outgoing halfedge for the central vertex

            // connect new halfedges and create new faces where necessary
            do
            {
                var he0 = he.Previous;
                var he1 = he.Next;
                he1.MakeConsecutive(he0);

                // create new face if necessary
                if (f == null)
                {
                    f = AddFace();
                    f.First = he;
                    he.Face = f;
                }

                // assign halfedge->face refs
                he0.Face = he1.Face = f;
                f = null;

                he = he1.Twin.Next;
            } while (he.Start != v);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public TV QuadPokeFace(TF face)
        {
            face.UnusedCheck();
            Faces.OwnsCheck(face);

            return QuadPokeFaceImpl(face);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        internal TV QuadPokeFaceImpl(TF face)
        {
            var vc = AddVertex();
            QuadPokeFaceImpl(face.First, vc);
            return vc;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
        public TV QuadPokeFace(TE first)
        {
            first.UnusedCheck();
            Halfedges.OwnsCheck(first);

            if (first.IsHole)
                return null;

            return QuadPokeFaceImpl(first);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
        internal TV QuadPokeFaceImpl(TE first)
        {
            var vc = AddVertex();
            QuadPokeFaceImpl(first, vc);
            return vc;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="center"></param>
        internal void QuadPokeFaceImpl(TE first, TV center)
        {
            // TODO implement
            throw new NotImplementedException();
        }


        /// <summary>
        /// This method assumes an even number of halfedges in the face loop and that the given vertex is unused.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="center"></param>
        internal void QuadSplitFace(TE first, TV center)
        {
            // create new halfedges to central vertex and connect to old halfedges
            var he0 = first;
            do
            {
                var he1 = he0.Next;
                var he2 = AddEdge(he1.Start, center);

                he0.MakeConsecutive(he2);
                he2.Twin.MakeConsecutive(he1);

                he0 = he1.Next;
            } while (he0 != first);

            // set outgoing halfedge from central vertex
            center.First = he0.Next.Twin;

            // create new faces and connect new halfedges to eachother
            {
                var f = first.Face;
                var he1 = he0.Previous;

                do
                {
                    var he2 = he0.Next;
                    var he3 = he1.Previous;
                    he2.MakeConsecutive(he3);

                    if (f == null)
                    {
                        f = AddFace();
                        he0.Face = he1.Face = f;
                    }

                    he2.Face = he3.Face = f; // set face refs for new halfedges
                    f.First = he0; // set first halfedge in face

                    f = null;
                    he1 = he2.Twin.Next;
                    he0 = he1.Next;

                } while (he0 != first);
            }
        }


        /// <summary>
        /// Reverses the winding direction of all faces in the mesh
        /// </summary>
        public void ReverseFaces()
        {
            foreach (var he in Halfedges)
            {
                var prev = he.Previous;
                he.Previous = he.Next;
                he.Next = prev;
            }
        }


        /// <summary>
        /// Orients each face such that the first halfedge returns the minimum value for the given function.
        /// </summary>
        /// <param name="getValue"></param>
        /// <param name="parallel"></param>
        public void OrientFacesToMin<T>(Func<TE, T> getValue, bool parallel = false)
            where T : IComparable<T>
        {
            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, Faces.Count), range => Body(range.Item1, range.Item2));
            else
                Body(0, Faces.Count);

            void Body(int from, int to)
            {
                for (int i = from; i < to; i++)
                {
                    var f = Faces[i];
                    if (f.IsUnused) continue;
                    f.First = f.Halfedges.SelectMin(getValue);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parallel"></param>
        public void OrientFacesToBoundary(bool parallel = false)
        {
            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, Faces.Count), range => Body(range.Item1, range.Item2));
            else
                Body(0, Faces.Count);

            void Body(int from, int to)
            {
                for (int i = from; i < to; i++)
                {
                    var f = Faces[i];
                    if (f.IsUnused) continue;
                    f.SetFirstToBoundary();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="triangulator"></param>
        public void TriangulateFaces(IFaceTriangulator<TV, TE, TF> triangulator)
        {
            var nf = Faces.Count;

            for (int i = 0; i < nf; i++)
                triangulator.Triangulate(Faces[i]);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="quadrangulator"></param>
        public void QuadrangulateFaces(IFaceQuadrangulator<TV, TE, TF> quadrangulator)
        {
            var nf = Faces.Count;

            for (int i = 0; i < nf; i++)
                quadrangulator.Quadrangulate(Faces[i]);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="flip"></param>
        /// <returns></returns>
        public IEnumerable<TE> GetFacesOrientedQuad(bool flip)
        {
            var stack = new Stack<TE>();
            int currTag = Faces.NextTag;

            for (int i = 0; i < Faces.Count; i++)
            {
                var f = Faces[i];
                if (f.IsUnused || f.Tag == currTag) continue; // skip if unused or already visited

                f.Tag = currTag;
                stack.Push(flip ? f.First.Next : f.First);

                foreach (var he in GetFacesOrientedQuad(stack, currTag))
                    yield return he;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="flip"></param>
        /// <returns></returns>
        public IEnumerable<TE> GetFacesOrientedQuad(TF start, bool flip)
        {
            start.UnusedCheck();
            Faces.OwnsCheck(start);

            var stack = new Stack<TE>();
            int currTag = Faces.NextTag;

            start.Tag = currTag;
            stack.Push(flip ? start.First.Next : start.First);

            return GetFacesOrientedQuad(stack, currTag);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="currTag"></param>
        /// <returns></returns>
        private IEnumerable<TE> GetFacesOrientedQuad(Stack<TE> stack, int currTag)
        {
            while (stack.Count > 0)
            {
                var he0 = stack.Pop();
                yield return he0;

                foreach (var he1 in AdjacentQuads(he0))
                {
                    var f1 = he1.Face;
                    if (f1 == null || f1.Tag == currTag) continue;

                    f1.Tag = currTag;
                    stack.Push(he1);
                }
            }

            IEnumerable<TE> AdjacentQuads(TE hedge)
            {
                yield return hedge.NextAtStart.Next; // down
                yield return hedge.Previous.PreviousAtStart; // up
                yield return hedge.PreviousAtStart.Previous; // left
                yield return hedge.Next.NextAtStart; // right
            }
        }


        /// <summary>
        /// Sets the first halfedge in each face to create consistent orientation where possible.
        /// Assumes quadrilateral faces.
        /// http://page.math.tu-berlin.de/~bobenko/MinimalCircle/minsurftalk.pdf
        /// </summary>
        public void UnifyFaceOrientationQuad(bool flip)
        {
            foreach (var he in GetFacesOrientedQuad(flip))
                he.MakeFirstInFace();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="flip"></param>
        public void UnifyFaceOrientationQuad(TF start, bool flip)
        {
            foreach (var he in GetFacesOrientedQuad(start, flip))
                he.MakeFirstInFace();
        }

        #endregion

        #endregion
    }
}
