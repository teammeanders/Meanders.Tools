using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Meanders.Tools.Plugin
{
    public class MeandersToolsInfo : GH_AssemblyInfo
    {
        public override string Name => "Meanders";

        public override Bitmap Icon
        {
            get
            {
                Stream stream =
                    Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(
                        "Meanders.Tools.Resources.Meanders.png");

                if (stream == null)
                    return null;

                return new Bitmap(stream);
            }
        }

        public override string Description =>
            "Meanders design and fabrication tools for Grasshopper.";

        public override Guid Id =>
            new Guid("715d84fe-2a1e-42bc-851e-ec6ce26c0835");

        public override string AuthorName =>
            "Mehrdad Azizkhani";

        public override string AuthorContact =>
            "";

        public override string AssemblyVersion =>
            GetType().Assembly.GetName().Version.ToString();
    }
}