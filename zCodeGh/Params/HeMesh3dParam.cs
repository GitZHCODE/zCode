using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Rhino.Geometry;

using zCodeGh.Types;

namespace zCodeGh.Params
{
    public class HeMesh3dParam : GH_PersistentParam<GH_HeMesh3d>
    {
    
        /// <summary>
        /// 
        /// </summary>
        public HeMesh3dParam()
            : base("HeMesh", "HeMesh", "", "zCode", "Parameters")
        {
        }


        /// <inheritdoc />
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }


        /// <inheritdoc />
        protected override Bitmap Icon
        {
            get { return zCodeGh.Properties.Resources.SlurParam; }
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bf4cb076-9078-4bf2-91bc-70af48c06d23"); }
        }

        /// <inheritdoc />
        protected override GH_GetterResult Prompt_Singular(ref GH_HeMesh3d value)
        {
            value = new GH_HeMesh3d();
            return GH_GetterResult.success;
        }


        /// <inheritdoc />
        protected override GH_GetterResult Prompt_Plural(ref List<GH_HeMesh3d> values)
        {
            values = new List<GH_HeMesh3d>();
            return GH_GetterResult.success;
        }

    }
}