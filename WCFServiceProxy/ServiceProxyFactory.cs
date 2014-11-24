namespace EyeCatch.WCF.ServiceProxy
{
    using System;

    /// <summary>
    /// Factory for creating proxy wrappers.
    /// </summary>
    public class ServiceProxyFactory
    {
        /// <summary>
        /// Wraps a new instance of the specified proxy.
        /// </summary>
        /// <typeparam name="TProxy">The proxy type.</typeparam>
        /// <returns>A wrapped proxy.</returns>
        public static IServiceProxyWrapper<TProxy> Create<TProxy>() where TProxy : class
        {
            return new ServiceProxyWrapper<TProxy>();
        }

        /// <summary>
        /// Wraps the specified proxy instance.
        /// </summary>
        /// <typeparam name="TProxy">The proxy type.</typeparam>
        /// <param name="endpointConfigurationName">The endpoint configuration name.</param>
        /// <returns>A wrapped proxy.</returns>
        public static IServiceProxyWrapper<TProxy> Create<TProxy>(string endpointConfigurationName) where TProxy : class
        {
            return new ServiceProxyWrapper<TProxy>(endpointConfigurationName);
        }
    }
}