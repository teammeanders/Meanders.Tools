using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Meanders.Tools.Core;
using Meanders.Tools.Grasshopper.Goo;
using Meanders.Tools.Plugin;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Meanders.Tools.Grasshopper.Components
{
    public class ME_Attribute_Component : GH_Component
    {
        public ME_Attribute_Component()
            : base(
                "ME Attribute",
                "ME Attr",
                "Create or modify Meanders attributes.",
                "Meanders",
                "Attributes")
        {
        }

        protected override void RegisterInputParams(
            GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Existing Attributes",
                "A",
                "Existing ME Attribute or Rhino ObjectAttributes.",
                GH_ParamAccess.item);

            pManager.AddTextParameter(
                "Keys",
                "K",
                "User text keys.",
                GH_ParamAccess.list);

            pManager.AddTextParameter(
                "Values",
                "V",
                "User text values.",
                GH_ParamAccess.list);

            pManager.AddTextParameter(
                "Name",
                "N",
                "Object name.",
                GH_ParamAccess.item);

            pManager.AddTextParameter(
                "Layer",
                "L",
                "Layer full path.",
                GH_ParamAccess.item);

            pManager.AddColourParameter(
                "Object Color",
                "Oc",
                "Object color.",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(
     GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Attributes",
                "A",
                "Meanders attributes.",
                GH_ParamAccess.item);

            pManager.AddTextParameter(
                "Keys",
                "K",
                "User text keys.",
                GH_ParamAccess.list);

            pManager.AddTextParameter(
                "Values",
                "V",
                "User text values.",
                GH_ParamAccess.list);

            pManager.AddTextParameter(
                "Name",
                "N",
                "Object name.",
                GH_ParamAccess.item);

            pManager.AddTextParameter(
                "Layer",
                "L",
                "Layer full path.",
                GH_ParamAccess.item);

            pManager.AddColourParameter(
                "Object Color",
                "Oc",
                "Object color.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstance(
            IGH_DataAccess DA)
        {
            ME_Attribute attribute = null;

            object existing = null;

            if (DA.GetData(0, ref existing))
            {
                if (existing is ME_Attribute_Goo goo)
                {
                    attribute = goo.Value != null
                        ? goo.Value.Duplicate()
                        : new ME_Attribute();
                }
                else if (existing is ME_Attribute meAttribute)
                {
                    attribute = meAttribute.Duplicate();
                }
                else if (existing is ObjectAttributes rhinoAttributes)
                {
                    attribute = new ME_Attribute(rhinoAttributes);
                }
                else
                {
                    AddRuntimeMessage(
                        GH_RuntimeMessageLevel.Error,
                        "Existing Attributes must be ME Attribute or Rhino ObjectAttributes.");

                    return;
                }
            }

            if (attribute == null)
                attribute = new ME_Attribute();

            // -----------------------------------------------------
            // Name
            // -----------------------------------------------------

            string name = string.Empty;

            if (DA.GetData(3, ref name))
            {
                if (!string.IsNullOrEmpty(name))
                    attribute.Name = name;
            }

            // -----------------------------------------------------
            // Layer
            // -----------------------------------------------------

            string layerName = string.Empty;

            if (DA.GetData(4, ref layerName))
            {
                if (!string.IsNullOrWhiteSpace(layerName))
                    attribute.Layer = layerName;
            }

            // -----------------------------------------------------
            // Object Color
            // -----------------------------------------------------

            Color objectColor = Color.Empty;

            if (DA.GetData(5, ref objectColor))
            {
                if (objectColor != Color.Empty)
                    attribute.ObjectColor = objectColor;
            }

            // -----------------------------------------------------
            // User Text
            // -----------------------------------------------------

            List<string> keys = new List<string>();
            List<string> values = new List<string>();

            DA.GetDataList(1, keys);
            DA.GetDataList(2, values);

            if (keys.Count != values.Count)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "Keys and Values must have the same number of items.");

                return;
            }

            attribute.SetUserText(keys, values);

            // -----------------------------------------------------
            // Outputs
            // -----------------------------------------------------

            ME_Attribute_Goo output =
                new ME_Attribute_Goo(attribute);

            DA.SetData(0, output);

            Dictionary<string, string> userText =
                attribute.GetUserText();

            List<string> outputKeys =
                new List<string>(userText.Keys);

            List<string> outputValues =
                new List<string>(userText.Values);

            DA.SetDataList(1, outputKeys);
            DA.SetDataList(2, outputValues);

            DA.SetData(3, attribute.Name);
            DA.SetData(4, attribute.Layer);
            DA.SetData(5, attribute.ObjectColor);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override Bitmap Icon =>
    MeandersIconLoader.Load("me-attribute.png");

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid(
                    "C91E7F42-5A63-4D88-B2E1-06F4A9C73D15");
            }
        }
    }
}