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
        /// Создать строку подключения с теми же настройками (пароля и т.п.) но с другим адресом БД
        /// </summary>
        /// <param name="folderPath">Путь к папке, где лежит БД</param>
        /// <returns></returns>
        IDatabaseConnectionString RetargetWithDifferentPath(
            string folderPath
            );
    }
}