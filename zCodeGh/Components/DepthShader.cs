using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using zCode.zCore;

namespace zCodeGh.Components
{
    public class DepthShader : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DepthShader class.
        /// </summary>
        public DepthShader()
          : base("Depth Shader", "Depth Shader",
              "Paints a Mesh based on vertex distance from the camera",
              "zCode", "Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "mesh", "Mesh to paint", GH_ParamAccess.item);
            pManager.AddColourParameter("colors", "colors", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("active", "active", "update the shader in real time", GH_ParamAccess.item, false);

           
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "mesh", "Painted mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            List<Color> colors = new List<Color>();
            bool active = false;


            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList(1, colors)) return;
            if (!DA.GetData(2, ref active)) return;

            Rhino.Display.RhinoView view = Rhino.RhinoDoc.ActiveDoc.Views.Find("Perspective", false);
            {
                view = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView; // assign active view
            }


            if (mesh.Normals.Count != mesh.Vertices.Count)
                mesh.Normals.ComputeNormals();

            if (mesh.VertexColors.Count != mesh.Vertices.Count)
                mesh.VertexColors.CreateMonotoneMesh(Color.Black);

            //Find Depth Range 
            Point3d closestPt = mesh.ClosestPoint(view.MainViewport.CameraLocation);
            double dd = closestPt.DistanceTo(view.MainViewport.CameraLocation);
            double dim = mesh.GetBoundingBox(true).Diagonal.Length;

            DepthShade(mesh, view.MainViewport.CameraLocation, new Interval(dd, dd + dim), colors);

            DA.SetData(0, new GH_Mesh(mesh));


            if (active) this.ExpireSolution(true);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return zCodeGh.Properties.Resources.SlurDisplay;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1cbd298f-6b7b-434a-b72a-e2c273927281"); }
        }


        public void DepthShade(Mesh mesh, Point3d cameraLocation, Interval _interval, List<Color> colors)
        {
            if (colors.Count < 2)
                throw new System.ArgumentException("must provide at least 2 colors");


            Color[] vc = mesh.VertexColors.ToArray();


            // parallelized main loop
            var chunks = System.Collections.Concurrent.Partitioner.Create(0, mesh.Vertices.Count);
            System.Threading.Tasks.Parallel.ForEach(chunks, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    double dist = mesh.Vertices[i].DistanceTo(new Point3f((float)cameraLocation.X, (float)cameraLocation.Y, (float)cameraLocation.Z));
                    double t = zMath.Remap(dist, _interval.Min, _interval.Max, 0.0, 1.0);
                    vc[i] = colors.Lerp(t);
                }
            });

            mesh.VertexColors.SetColors(vc);
        }

    }
}