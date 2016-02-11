using System;
using System.Globalization;
using System.Text;
using System.Web;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.MultipleXml.XmlSaver
{
    public class XmlPreparator : IXmlPreparator
    {
        public StringBuilder PrepareXml(IPerformanceRecordData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var result = new StringBuilder();

            //сначала внутренние рекорды
            var innerXml = new StringBuilder();
            var children = item.GetChildren();
            if (children != null && children.Count > 0)
            {
                foreach (var citem in children)
                {
                    var childXml = PrepareXml(citem);
                    innerXml.Append(childXml);
                }
            }

            //потом стек ексцепшенов

            //формируем строку xml
            var record0 = new StringBuilder(RecordXml);
            var record1 = record0.Replace(
                "{_ClassNameBase64_}", 
                HttpUtility.UrlEncode(item.ClassName));
            var record2 = record1.Replace(
                "{_MethodName_}", 
                item.MethodName);
            var record3 = record2.Replace(
                "{_StartDate_}",
                item.StartTime.ToString("yyyyMMdd HH:mm:ss.fff"));
            var record4 = record3.Replace(
                "{_LogDate_}",
                DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"));
            var record5 = record4.Replace(
                "{_TimeInterval_}",
                item.TimeInterval.ToString(CultureInfo.InvariantCulture));
            var record6 = record5.Replace(
                "{_CreationStackBase64_}",
                HttpUtility.UrlEncode(item.CreationStack));
            var record7 = record6.Replace(
                "{_IsException_}",
                item.Exception != null ? "True" : "False");
            var record8 = record7.Replace(
                "{_Exception_}",
                item.Exception != null
                    ? PrepareExceptionXml(item.Exception).ToString()
                    : string.Empty);
            var record9 = record8.Replace(
                "{_Children_}",
                innerXml.ToString());

            result.Append(record9);

            return
                result;
        }

        private StringBuilder PrepareExceptionXml(Exception excp)
        {
            if (excp == null)
            {
                throw new ArgumentNullException("excp");
            }

            var excp0 = new StringBuilder(ExceptionXml);
            var excp1 = excp0.Replace(
                "{_dotNetExceptionType_}",
                excp.GetType().Name);
            var excp2 = excp1.Replace(
                "{_Message_}",
                excp.Message);
            var excp3 = excp2.Replace(
                "{_Stack_}",
                HttpUtility.UrlEncode(excp.StackTrace));
            var excp4 = excp3.Replace(
                "{_InnerException_}",
                excp.InnerException != null
                    ? PrepareExceptionXml(excp.InnerException).ToString()
                    : string.Empty);

            return excp4;
        }

        private const string RecordXml = @"
<PerformanceRecord>
    <ClassNameBase64>{_ClassNameBase64_}</ClassNameBase64>
    <MethodName>{_MethodName_}</MethodName>
    <StartDate>{_StartDate_}</StartDate>
    <LogDate>{_LogDate_}</LogDate>
    <TimeInterval>{_TimeInterval_}</TimeInterval>
    <CreationStackBase64>{_CreationStackBase64_}</CreationStackBase64>
    <IsException>{_IsException_}</IsException>
    {_Exception_}

    <Children>{_Children_}</Children>

</PerformanceRecord>
";

        private const string ExceptionXml = @"
<Exception>
    <dotNetExceptionType>{_dotNetExceptionType_}</dotNetExceptionType>
    <Message>{_Message_}</Message>
    <Stack>{_Stack_}</Stack>
    <InnerException>{_InnerException_}</InnerException>
</Exception>
";
    }
}