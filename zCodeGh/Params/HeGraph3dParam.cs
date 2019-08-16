using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Rhino.Geometry;

using zCodeGh.Types;

namespace zCodeGh.Params
{
    public class HeGraph3dParam : GH_PersistentParam<GH_HeGraph3d>
    {
    

        /// <summary>
        /// 
        /// </summary>
        public HeGraph3dParam()
            : base("HeGraph", "HeGraph", "", "zCode", "Parameters")
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

 
        public override Guid ComponentGuid
        {
            get { return new Guid("83033fb2-c34d-4c01-ac2d-ce48d9d66883"); }
        }
        /// <inheritdoc />
        protected override GH_GetterResult Prompt_Singular(ref GH_HeGraph3d value)
        {
            value = new GH_HeGraph3d();
            return GH_GetterResult.success;
        }


        /// <inheritdoc />
        protected override GH_GetterResult Prompt_Plural(ref List<GH_HeGraph3d> values)
        {
            values = new List<GH_HeGraph3d>();
            return GH_GetterResult.success;
        }

    }
}