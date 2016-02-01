using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PerformanceTelemetry.Tests.DB.DB
{
    public class EmbeddedResourceHelper
    {
        protected static string GetResourceName(
            Assembly resourceAssembly,
            string partOfResourceName
            )
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException("resourceAssembly");
            }
            if (partOfResourceName == null)
            {
                throw new ArgumentNullException("partOfResourceName");
            }

            var resourceNames = resourceAssembly.GetManifestResourceNames().ToList();
            var resourceName = resourceNames.Find(j => j.ToLower().Contains(partOfResourceName.ToLower()));

            return
                resourceName;
        }

        protected static Stream GetAsStream(
            Assembly resourceAssembly,
            string partOfResourceName
            )
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException("resourceAssembly");
            }
            if (partOfResourceName == null)
            {
                throw new ArgumentNullException("partOfResourceName");
            }

            var resourceName = GetResourceName(resourceAssembly, partOfResourceName);

            var result = resourceAssembly.GetManifestResourceStream(resourceName);

            return
                result;
        }

        protected static string GetAsString(
            Assembly resourceAssembly,
            string partOfResourceName,
            Encoding encoding = null
            )
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException("resourceAssembly");
            }
            if (partOfResourceName == null)
            {
                throw new ArgumentNullException("partOfResourceName");
            }

            using (var s = GetAsStream(resourceAssembly, partOfResourceName))
            using (var sr = (encoding != null ? new StreamReader(s, encoding) : new StreamReader(s)))
            {
                return
                    sr.ReadToEnd();
            }
        }
    }
}