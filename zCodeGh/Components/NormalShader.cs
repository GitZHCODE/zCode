using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using zCode.zCore;
using zCode.zField;
using zCode.zMesh;
using zCode.zRhino;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace zCodeGh.Components
{
    public class NormalShader : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public NormalShader()
          : base("Normal Shader", "Normal Shader",
              "Paint a Mesh based on vertex normal directions",
              "zCode", "Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "mesh", "Mesh to paint", GH_ParamAccess.item);
            pManager.AddColourParameter("colors", "colors", "Paint colors", GH_ParamAccess.list);
            pManager.AddVectorParameter("direction", "dir", "Direction", GH_ParamAccess.item, Vector3d.ZAxis);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("mesh", "mesh", "Painted Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            List<Color> colors = new List<Color>();
            Vector3d dir = new Vector3d();

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList(1, colors)) return;
            if (!DA.GetData(2, ref dir)) return;

            var norms = mesh.Normals;

            if (norms.Count != mesh.Vertices.Count)
                norms.ComputeNormals();

            mesh.ColorVertices(i => colors.Lerp(dir * norms[i] * 0.5 + 0.5), true);

            DA.SetData(0, new GH_Mesh(mesh));

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return zCodeGh.Properties.Resources.SlurDisplay;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("340b4273-4cf1-41d8-bdd7-ec561c390042"); }
        }
    }
}
