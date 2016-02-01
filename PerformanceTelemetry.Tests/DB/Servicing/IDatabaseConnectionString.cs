namespace PerformanceTelemetry.Tests.DB.Servicing
{
    public interface IDatabaseConnectionString
    {
        string ConnectionString
        {
            get;
        }

        bool AllowMultiplyConnectionsToDatabase
        {
            get;
        }

        bool AllowMultipleActiveResultSets
        {
            get;
        }

        /// <summary>
        /// ������� ������ ����������� � ���� �� ����������� (������ � �.�.) �� � ������ ������� ��
        /// </summary>
        /// <param name="folderPath">���� � �����, ��� ����� ��</param>
        /// <returns></returns>
        IDatabaseConnectionString RetargetWithDifferentPath(
            string folderPath
            );
    }
}