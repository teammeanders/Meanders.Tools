using Grasshopper.Kernel;
using Meanders.Tools.Grasshopper.Goo;
using System;
using System.Collections.Generic;
using System.Drawing;
using Meanders.Tools.Plugin;

namespace Meanders.Tools.Grasshopper.Parameters
{
    public class ME_Attribute_Param : GH_PersistentParam<ME_Attribute_Goo>
    {
        public ME_Attribute_Param()
            : base(
                "ME Attribute",
                "ME Attr",
                "Meanders Attributes",
                "Meanders",
                "Params")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("B6C4D8A1-3E72-4F95-8C16-7A9E2D51F403"); }
        }

        protected override Bitmap Icon =>
    MeandersIconLoader.Load("me-attribute-param.png");

        protected override GH_GetterResult Prompt_Singular(
            ref ME_Attribute_Goo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(
            ref List<ME_Attribute_Goo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}