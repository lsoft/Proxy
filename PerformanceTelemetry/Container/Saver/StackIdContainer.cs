using System;
using System.Collections.Generic;

namespace PerformanceTelemetry.Container.Saver
{
    public class StackIdContainer
    {
        //��������� ����� �� ������ ���� ������-����������, ��� ��� ��������� � ���� ���� ������ �� ������ ����������
        private readonly Dictionary<Guid, int> _dict = new Dictionary<Guid, int>();

        public bool TryGet(
            Guid guid,
            out int index
            )
        {
            return
                _dict.TryGetValue(
                    guid,
                    out index
                    );
        }

        public void Add(
            Guid guid,
            int index
            )
        {
            _dict.Add(guid, index);
        }


    }
}