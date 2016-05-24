namespace EyeCatch.WCF.ServiceProxy
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper for using service proxies.
    /// </summary>
    /// <typeparam name="TProxy">The service interface.</typeparam>
    internal class ServiceProxyWrapper<TProxy> : IServiceProxyWrapper<TProxy> where TProxy : class
    {
        /// <summary>
        /// The base channel factory
        /// </summary>
        private readonly ChannelFactory<TProxy> channelFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyWrapper{TProxy}"/> class.
        /// </summary>
        public ServiceProxyWrapper()
            : this(typeof(TProxy).Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyWrapper{TProxy}"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        public ServiceProxyWrapper(string endpointConfigurationName)
        {
            this.channelFactory = new ChannelFactory<TProxy>(endpointConfigurationName);
        }

        /// <summary>
        /// The [error occured] event
        /// </summary>
        public event EventHandler<ServiceProxyErrorEventArgs> ErrorOccured;

        /// <summary>
        /// Gets the original proxy.
        /// </summary>
        /// <value>The original proxy.</value>
        public TProxy Proxy
        {
            get
            {
                return this.channelFactory.CreateChannel();
            }
        }

        /// <summary>
        /// Configures the wrapper for the spefified proxy.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        /// <returns>The configured wrapper.</returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public IServiceProxyWrapper<TProxy> Configure(Action<ChannelFactory<TProxy>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            // Configure channel
            action(this.channelFactory);

            return this;
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        public void Use(Action<TProxy> action)
        {
            this.Run(action, null);
        }

        /// <summary>
        /// Using statement for action.
        /// </summary>
        /// <param name="action">The code block to execute.</param>
        /// <param name="error">The error action.</param>
        /// <exception cref="System.ArgumentNullException">endPointConfigurationName</exception>
        public void Use(Action<TProxy> action, Action<Exception> error)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            this.Run(action, error);
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        /// <returns>Task.</returns>
        public Task Use(Func<TProxy, Task> callback)
        {
            return this.Use(callback, (ex) => this.OnErrorOccured(typeof(TProxy), ex));
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error action.</param>
        /// <returns>Task.</returns>
        public Task Use(Func<TProxy, Task> callback, Action<Exception> error)
        {
            return this.Run(callback, error);
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

            // Throw event if there is a handler registered
            if (this.ErrorOccured != null)
            {
                this.ErrorOccured(this, new ServiceProxyErrorEventArgs(typeof(TProxy), exception));
            }
        }

        /// <summary>
        /// Runs the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="error">The action to run when an error is encountered.</param>
        private void Run(Action<TProxy> action, Action<Exception> error)
        {
            Task.Run(() => this.Run(async (client) => action(client), error).ConfigureAwait(false)).Wait();
        }

        /// <summary>
        /// Runs the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="error">The action to run when an error is encountered.</param>
        private async Task Run(Func<TProxy, Task> action, Action<Exception> error) 
        {
            var channel = (IClientChannel)this.channelFactory.CreateChannel();

            var success = false;

            try
            {
                await action((TProxy)channel);

                channel.Close();

                success = true;
            }
            catch (CommunicationObjectAbortedException ex)
            {
                /*
                 * Object should be discarded if this is reached.   
                 * Debugging discovered the following exception here: 
                 * "Connection can not be established because it has been aborted"  
                 */

                error(ex);

                throw;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                error(ex);

                channel.Abort();
            }
            catch (MessageSecurityException ex)
            {
                error(ex);

                throw;
            }
            catch (ActionNotSupportedException ex)
            {
                error(ex);

                channel.Abort();
            }
            catch (ServerTooBusyException ex)
            {
                error(ex);

                channel.Abort(); // Possibly retry? 
            }
            catch (EndpointNotFoundException ex)
            {
                error(ex);

                channel.Abort(); // Possibly retry? 
            }
            catch (FaultException ex)
            {
                error(ex);

                channel.Abort();
            }
            catch (CommunicationException ex)
            {
                error(ex);

                channel.Abort();
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

                error(ex);

                channel.Abort();
            }
            catch (ObjectDisposedException ex)
            {
                // TODO: Handle this duplex action exception. Occurs when client disappears.   
                // Source: http://stackoverflow.com/questions/1427926/detecting-client-death-in-wcf-duplex-contracts/1428238#1428238 

                error(ex);
            }
            finally
            {
                if (!success)
                {
                    channel.Abort();
                }
            }
        }
    }
}