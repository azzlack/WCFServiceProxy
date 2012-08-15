namespace EyeCatch.WCF.ServiceProxy.EventArguments
{
    using System;

    /// <summary>
    /// Event arguments for service proxy errors
    /// </summary>
    public class ServiceProxyErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyErrorEventArgs" /> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="exception">The exception.</param>
        public ServiceProxyErrorEventArgs(Type service, Exception exception)
        {
            this.Service = service;
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public Type Service { get; private set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; private set; }
    }
}