using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver
{
    /// <summary>
    /// контейнер стека не обязан быть потоко-защищенным, так как обращения к нему идут только из потока сохранения
    /// </summary>
    public class StackIdContainer : IDisposable
    {
        //хешер для поиска соотв. стека
        private readonly HashAlgorithm _hashAlgorithm;

        //контейнер ключа должен быть регистрозависимым, так как язык C# регистрозависим к именам классов и методов
        private readonly Dictionary<string, int> _dict0 = new Dictionary<string, int>();

        private bool _disposed = false;
        
        public StackIdContainer(
            
            )
        {
            _hashAlgorithm = MD5.Create();
        }

        public void ForceAdd(
            string key,
            int stackId
            )
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            _dict0[key] = stackId;
        }

        public int AddIfNecessaryAndReturnId(
            IPerformanceRecordData item,
            Func<string, IPerformanceRecordData, int> storeStackFunc
            )
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (storeStackFunc == null)
            {
                throw new ArgumentNullException("storeStackFunc");
            }

            int index;

            var key = GenerateKey(item.ClassName, item.MethodName);
            if (!_dict0.TryGetValue(key, out index))
            {
                index = storeStackFunc(key, item);

                ForceAdd(key, index);
            }

            return
                index;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _hashAlgorithm.Dispose();
            }
        }

        private string GenerateKey(
            string className,
            string methodName
            )
        {
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            
            return
                string.Format(
                    "{0}.{1}",
                    className,
                    methodName
                    );
        }
    }
}