namespace EyeCatch.WCF.ServiceProxy.Factories
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Text;

    using EyeCatch.WCF.ServiceProxy.EventArguments;

    /// <summary>
    /// Factory for generating WCF service proxies.
    /// </summary>
    /// <remarks>
    /// Generated proxies always closes/disposes gracefully.
    /// </remarks>
    /// <typeparam name="TProxy">The service interface.</typeparam>
    public class ServiceProxyFactory<TProxy> : IServiceProxyFactory<TProxy> where TProxy : class
    {
        /// <summary>
        /// The proxy instance
        /// </summary>
        private static readonly ServiceProxyFactory<TProxy> Singleton = new ServiceProxyFactory<TProxy>();

        /// <summary>
        /// Prevents a default instance of the <see cref="ServiceProxyFactory&lt;TProxy&gt;"/> class from being created.
        /// </summary>
        private ServiceProxyFactory()
        {
        }

        /// <summary>
        /// The [error occured] event
        /// </summary>
        public static event EventHandler<ServiceProxyErrorEventArgs> ErrorOccured;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static ServiceProxyFactory<TProxy> Instance
        {
            get
            {
                return Singleton;
            }
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        public void Use(Action<TProxy> callback)
        {
            var channelFactory = new ChannelFactory<TProxy>(typeof(TProxy).Name);

            this.Run(channelFactory, callback);
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">
        /// (Optional) The service endpoint configuration name. Uses <typeparamref name="TProxy"/> if not specified.
        /// </param>
        /// <param name="callback">
        /// The code block to execute.
        /// </param>
        public void Use(string endPointConfigurationName, Action<TProxy> callback)
        {
            if (string.IsNullOrEmpty(endPointConfigurationName))
            {
                throw new ArgumentNullException("endPointConfigurationName");
            }

            var channelFactory = new ChannelFactory<TProxy>(endPointConfigurationName);

            this.Run(channelFactory, callback);
        }

        /// <summary>
        /// Called when [error occured].
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="exception">The exception.</param>
        public virtual void OnErrorOccured(Type service, Exception exception)
        {
            var message = new StringBuilder();
            message.AppendLine(exception.Message);
            message.AppendLine(exception.StackTrace);

            if (exception.InnerException != null)
            {
                message.AppendLine(exception.InnerException.Message);
                message.AppendLine(exception.InnerException.StackTrace);
            }

            // Trace error
            Trace.TraceError(message.ToString());

            // Throw event if there is a handler registered
            if (ErrorOccured != null)
            {
                ErrorOccured(this, new ServiceProxyErrorEventArgs(typeof(TProxy), exception));
            }
        }

        /// <summary>
        /// Runs the specified channel factory.
        /// </summary>
        /// <param name="channelFactory">The channel factory.</param>
        /// <param name="callback">The callback.</param>
        private void Run(ChannelFactory<TProxy> channelFactory, Action<TProxy> callback)
        {
            var proxy = (IClientChannel)channelFactory.CreateChannel();

            var success = false;

            try
            {
                callback((TProxy)proxy);
                proxy.Close();

                success = true;
            }
            catch (CommunicationObjectAbortedException ex)
            {
                /*
                 * Object should be discarded if this is reached.   
                 * Debugging discovered the following exception here: 
                 * "Connection can not be established because it has been aborted"  
                 */

                this.OnErrorOccured(typeof(TProxy), ex);

                throw;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                this.OnErrorOccured(typeof(TProxy), ex);

                proxy.Abort();
            }
            catch (MessageSecurityException ex)
            {
                this.OnErrorOccured(typeof(TProxy), ex);

                throw;
            }
            catch (ChannelTerminatedException ex)
            {
                this.OnErrorOccured(typeof(TProxy), ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (ServerTooBusyException ex)
            {
                this.OnErrorOccured(typeof(TProxy), ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (EndpointNotFoundException ex)
            {
                this.OnErrorOccured(typeof(TProxy), ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (FaultException ex)
            {
                this.OnErrorOccured(typeof(TProxy), ex);

                proxy.Abort();
            }
            catch (CommunicationException ex)
            {
                this.OnErrorOccured(typeof(TProxy), ex);

                proxy.Abort();
            }
            catch (TimeoutException ex)
            {
                /* Sample error found during debug:  
                 * 
                 * The message could not be transferred within the allotted timeout of  
                 * 00:01:00. There was no space available in the reliable channel's  
                 * transfer window. The time allotted to this operation may have been a  
                 * portion of a longer timeout. 
                 */

                this.OnErrorOccured(typeof(TProxy), ex);

                proxy.Abort();
            }
            catch (ObjectDisposedException ex)
            {
                // TODO: Handle this duplex callback exception. Occurs when client disappears.   
                // Source: http://stackoverflow.com/questions/1427926/detecting-client-death-in-wcf-duplex-contracts/1428238#1428238 
                this.OnErrorOccured(typeof(TProxy), ex);
            }
            finally
            {
                if (!success)
                {
                    proxy.Abort();
                }
            }
        }
    }
}