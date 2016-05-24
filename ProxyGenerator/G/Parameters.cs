using System;
using ProxyGenerator.PL;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.G
{
    /// <summary>
    /// ����� �������� ��� ��������� ������ ����
    /// </summary>
    public class Parameters
    {
        public IProxyPayloadFactory ProxyPayloadFactory
        {
            get;
            private set;
        }

        public WrapResolverDelegate WrapResolver
        {
            get;
            private set;
        }

        public string GeneratedAssemblyName
        {
            get;
            private set;
        }

        public string[] AdditionalReferencedAssembliesLocation
        {
            get;
            private set;
        }

        /// <summary>
        /// ����������� ������ �������� ��� ��������� ������ ����
        /// </summary>
        /// <param name="proxyPayloadFactory">������� �������� �������� ��������</param>
        /// <param name="wrapResolver">�������-������������, ���� �� �������� �����</param>
        /// <param name="generatedAssemblyName">�������������� �������� ����� ������������ ������</param>
        /// <param name="additionalReferencedAssembliesLocation">������, �� ������� ���� ������������� ������� ��������� ��� ���������� ������</param>
        public Parameters(
            IProxyPayloadFactory proxyPayloadFactory,
            WrapResolverDelegate wrapResolver,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null
            )
        {
            if (proxyPayloadFactory == null)
            {
                throw new ArgumentNullException("proxyPayloadFactory");
            }
            if (wrapResolver == null)
            {
                throw new ArgumentNullException("wrapResolver");
            }
            //generatedAssemblyName allowed to be null
            //additionalReferencedAssembliesLocation allowed to be null

            ProxyPayloadFactory = proxyPayloadFactory;
            WrapResolver = wrapResolver;
            GeneratedAssemblyName = generatedAssemblyName;
            AdditionalReferencedAssembliesLocation = additionalReferencedAssembliesLocation;
        }
    }
}