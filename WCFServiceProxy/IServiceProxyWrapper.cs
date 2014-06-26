namespace EyeCatch.WCF.ServiceProxy
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper for using service proxies.
    /// </summary>
    /// <typeparam name="TProxy">The service client interface.</typeparam>
    public interface IServiceProxyWrapper<TProxy> where TProxy : class
    {
        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        Task Use(Func<TProxy, Task> callback);

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error callback.</param>
        Task Use(Func<TProxy, Task> callback, Action<Exception> error);

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">(Optional) The service endpoint configuration name. Uses <typeparamref name="TProxy" /> if not specified.</param>
        /// <param name="callback">The code block to execute.</param>
        Task Use(string endPointConfigurationName, Func<TProxy, Task> callback);

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">(Optional) The service endpoint configuration name. Uses <typeparamref name="TProxy" /> if not specified.</param>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error callback.</param>
        /// <exception cref="System.ArgumentNullException">endPointConfigurationName</exception>
        Task Use(string endPointConfigurationName, Func<TProxy, Task> callback, Action<Exception> error);
    }
}