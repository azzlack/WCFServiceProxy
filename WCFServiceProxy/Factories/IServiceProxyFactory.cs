namespace EyeCatch.WCF.ServiceProxy.Factories
{
    using System;

    /// <summary>
    /// Factory for generating service proxies.
    /// Generated proxies closes gracefully.
    /// </summary>
    /// <typeparam name="TProxy">The service interface.</typeparam>
    public interface IServiceProxyFactory<out TProxy> where TProxy : class
    {
        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        void Use(Action<TProxy> callback);

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">(Optional) The service endpoint configuration name.</param>
        /// <param name="callback">The code block to execute.</param>
        void Use(string endPointConfigurationName, Action<TProxy> callback);
    }
}