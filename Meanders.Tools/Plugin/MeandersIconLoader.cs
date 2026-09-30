using System.Drawing;
using System.IO;
using System.Reflection;

namespace Meanders.Tools.Plugin
{
    internal static class MeandersIconLoader
    {
        public static Bitmap Load(string fileName)
        {
            string resourceName =
                $"Meanders.Tools.Resources.Icons.{fileName}";

            Stream stream =
                Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(resourceName);

            if (stream == null)
                return null;

            return new Bitmap(stream);
        }
    }
}