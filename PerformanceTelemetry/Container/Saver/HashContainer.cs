using System;
using System.Collections.Generic;

namespace PerformanceTelemetry.Container.Saver
{
    public class HashContainer
    {
        //��������� ����� ����� �� ������ ���� ������-����������, ��� ��� ��������� � ���� ���� ������ �� ������ ����������
        private readonly HashSet<Guid> _hash = new HashSet<Guid>();

        public bool Contains(
            Guid guid
            )
        {
            return
                _hash.Contains(guid);
        }

        public void Add(
            Guid guid
            )
        {
            _hash.Add(guid);
        }


    }
}