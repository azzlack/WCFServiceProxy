namespace EyeCatch.WCF.ServiceProxy
{
    using System;
    using System.ServiceModel;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper for using service proxies.
    /// </summary>
    /// <typeparam name="TProxy">The service client interface.</typeparam>
    public interface IServiceProxyWrapper<TProxy> where TProxy : class
    {
        /// <summary>
        /// Gets the original proxy.
        /// </summary>
        /// <value>The original proxy.</value>
        TProxy Proxy { get; }

        /// <summary>
        /// Configures the wrapper for the spefified proxy.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        /// <returns>The configured wrapper.</returns>
        IServiceProxyWrapper<TProxy> Configure(Action<ChannelFactory<TProxy>> action);
 
        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        void Use(Action<TProxy> action);

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        /// <param name="error">The error.</param>
        void Use(Action<TProxy> action, Action<Exception> error);

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        Task Use(Func<TProxy, Task> callback);

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error action.</param>
        Task Use(Func<TProxy, Task> callback, Action<Exception> error);

        /// <summary>
        /// Runs the specified action and returns a value.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        Task<T> Return<T>(Func<TProxy, Task<T>> action) where T : class;

        /// <summary>
        /// Runs the specified action and returns a value.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        /// <param name="error">The error action.</param>
        Task<T> Return<T>(Func<TProxy, Task<T>> action, Action<Exception> error) where T : class;
    }
}