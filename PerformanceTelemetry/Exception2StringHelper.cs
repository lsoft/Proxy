using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTelemetry
{
    public class Exception2StringHelper
    {
        public static string ToFullString(
            Exception excp
            )
        {
            if (excp == null)
            {
                throw new ArgumentNullException("excp");
            }

            var sb = new StringBuilder();

            AppendException(sb, excp);

            return
                sb.ToString();
        }

        private static void AppendException(StringBuilder sb, Exception excp)
        {
            if (sb == null)
            {
                throw new ArgumentNullException("sb");
            }
            if (excp == null)
            {
                throw new ArgumentNullException("excp");
            }

            sb.AppendLine(excp.Message);
            sb.AppendLine(excp.Source);
            sb.AppendLine(excp.StackTrace);
            sb.AppendLine();

            if (excp.InnerException != null)
            {
                AppendException(sb, excp.InnerException);
            }
        }

    }
}
