using System;

namespace ProxyGenerator.Generator
{
    /// <summary>
    /// Прокси ключ для внутреннего хранилища скомпилированных типов проксей
    /// </summary>
    internal class ProxyKey : IEquatable<ProxyKey>
    {
        private readonly Type _i;
        private readonly Type _c;

        public ProxyKey(
            Type i,
            Type c)
        {
            if (i == null)
            {
                throw new ArgumentNullException("i");
            }
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }

            _i = i;
            _c = c;
        }

        #region equality

        public bool Equals(ProxyKey other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(other._i, _i) && Equals(other._c, _c);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof (ProxyKey))
            {
                return false;
            }
            return Equals((ProxyKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_i != null ? _i.GetHashCode() : 0)*397) ^ (_c != null ? _c.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ProxyKey left, ProxyKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProxyKey left, ProxyKey right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
