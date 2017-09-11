using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Wrapper.Event
{
    internal class EventWrapper : IWrapper
    {
        private readonly Type _targetClassType;
        private readonly EventInfo _e;

        public EventWrapper(
            Type targetClassType,
            EventInfo e
            )
        {
            if (targetClassType == null)
            {
                throw new ArgumentNullException("targetClassType");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            _targetClassType = targetClassType;
            _e = e;
        }

        public string ToSourceCode()
        {
            var name = _e.Name;
            var type = SourceHelper.GetClassName(_e.EventHandlerType);

            var adder = Adder.Replace(
                "{_Name_}",
                name
                );

            var remover = Remover.Replace(
                "{_Name_}",
                name
                );

            var title = Title
                .Replace(
                    "{_Type_}",
                    type
                    )
                .Replace(
                    "{_Name_}",
                    name//SourceHelper.FullNameConverter(propTypeName)
                    )
                .Replace(
                    "{_Add_}", 
                    adder
                    )
                .Replace(
                    "{_Remove_}",
                    remover
                    )
                ;

            return
                title;
        }

        private const string Adder = @"
            add
            {
                _wrappedObject.{_Name_} += value;
            }
";

        private const string Remover = @"
            remove
            {
                _wrappedObject.{_Name_} -= value;
            }
";

        private const string Title = @"
        public event {_Type_} {_Name_}
        {
            {_Add_}
            {_Remove_}
        }

";
    }
}
