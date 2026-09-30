using Grasshopper;
using Grasshopper.Kernel;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Meanders.Tools.Plugin
{
    public class MeandersToolsPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Stream stream =
                Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(
                    "Meanders.Tools.Resources.Meanders.png");

            if (stream != null)
            {
                Bitmap icon = new Bitmap(stream);

                Instances.ComponentServer.AddCategoryIcon(
                    "Meanders",
                    icon);
            }

            return GH_LoadingInstruction.Proceed;
        }
    }
}