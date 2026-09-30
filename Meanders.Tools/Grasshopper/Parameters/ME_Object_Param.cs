using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Meanders.Tools.Grasshopper.Goo;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using Meanders.Tools.Plugin;

namespace Meanders.Tools.Grasshopper.Parameters
{
    public class ME_Object_Param :
        GH_Param<ME_Object_Goo>,
        IGH_PreviewObject,
        IGH_BakeAwareObject
    {
        private bool m_hidden;

        public ME_Object_Param()
            : base(
                "ME Object",
                "ME Obj",
                "Meanders Object",
                "Meanders",
                "Params",
                GH_ParamAccess.tree)
        {
            m_hidden = false;
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "A7F3C2D1-6E54-4B91-9F28-21D8E6C04A73");
            }
        }

        protected override Bitmap Icon =>
     MeandersIconLoader.Load("me-object-param.png");

        // ------------------------------------------------------------
        // Preview
        // ------------------------------------------------------------

        bool IGH_PreviewObject.Hidden
        {
            get { return m_hidden; }
            set { m_hidden = value; }
        }

        bool IGH_PreviewObject.IsPreviewCapable
        {
            get { return true; }
        }

        Rhino.Geometry.BoundingBox
            IGH_PreviewObject.ClippingBox
        {
            get
            {
                return base.Preview_ComputeClippingBox();
            }
        }

        void IGH_PreviewObject.DrawViewportMeshes(
            IGH_PreviewArgs args)
        {
            base.Preview_DrawMeshes(args);
        }

        void IGH_PreviewObject.DrawViewportWires(
            IGH_PreviewArgs args)
        {
            base.Preview_DrawWires(args);
        }

        // ------------------------------------------------------------
        // Bake
        // ------------------------------------------------------------

        bool IGH_BakeAwareObject.IsBakeCapable
        {
            get
            {
                return true;
            }
        }

        void IGH_BakeAwareObject.BakeGeometry(
            RhinoDoc doc,
            List<Guid> obj_ids)
        {
            if (doc == null)
                return;

            foreach (ME_Object_Goo meObject
                in VolatileData.AllData(true))
            {
                if (meObject == null)
                    continue;

                Guid objGuid;

                if (meObject.BakeGeometry(
                    doc,
                    null,
                    out objGuid))
                {
                    obj_ids.Add(objGuid);
                }
            }
        }

        void IGH_BakeAwareObject.BakeGeometry(
            RhinoDoc doc,
            ObjectAttributes att,
            List<Guid> obj_ids)
        {
            if (doc == null)
                return;

            foreach (ME_Object_Goo meObject
                in VolatileData.AllData(true))
            {
                if (meObject == null)
                    continue;

                Guid objGuid;

                if (meObject.BakeGeometry(
                    doc,
                    att,
                    out objGuid))
                {
                    obj_ids.Add(objGuid);
                }
            }
        }
    }
}