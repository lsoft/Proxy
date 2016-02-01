using System;
using System.Collections.Generic;
using System.IO;

namespace PerformanceTelemetry.Tests.DB.DB
{
    /// <summary>
    /// Класс, используемый для доступа к ресурсу gis.main.trn
    /// </summary>
    public class TrnHelper : EmbeddedResourceHelper
    {
        private const string MainDatabaseTrnFileName = "performancedatabase.test.trn";

        public const string MainDatabaseName = "PerformanceDatabase.Test";

        public static string GetResourceName()
        {
            return
                GetResourceName(
                    typeof(TrnHelper).Assembly,
                    MainDatabaseTrnFileName
                    );
        }

        public static Stream GetAsStream()
        {
            return
                GetAsStream(
                    typeof(TrnHelper).Assembly,
                    MainDatabaseTrnFileName
                    );
        }

        public static ITempFile ExtractTrnToTempFile()
        {
            var strFullFileName = typeof(TrnHelper).Module.FullyQualifiedName;
            var strShortFileName = typeof(TrnHelper).Module.Name;
            var currentDirectory = strFullFileName.Substring(0, strFullFileName.Length - strShortFileName.Length - 1);

            var tempTrn =
                Path.Combine(
                    currentDirectory,
                    Path.GetRandomFileName() + ".trn"
                    );


            using (var fileStream = new FileStream(tempTrn, FileMode.CreateNew))
            using (var stream = GetAsStream())
            {
                stream.CopyTo(fileStream);
            }

            return
                new TempFile(tempTrn);
        }

        public interface ITempFile : IDisposable
        {
            string FilePath
            {
                get;
            }
        }

        private class TempFile : ITempFile
        {
            public string FilePath
            {
                get;
                private set;
            }

            public TempFile(
                string filepath
                )
            {
                FilePath = filepath;
            }

            public void Dispose()
            {
                try
                {
                    File.Delete(this.FilePath);
                }
                catch
                {
                    //nothing to do
                }
            }

        }
    }
}
