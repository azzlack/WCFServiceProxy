namespace EyeCatch.WCF.ServiceProxy
{
    using System;
    using System.Diagnostics;
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
        /// The [error occured] event
        /// </summary>
        public event EventHandler<ServiceProxyErrorEventArgs> ErrorOccured;

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        public void Use(Action<TProxy> callback)
        {
            this.Use(typeof(TProxy).Name, callback);
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error.</param>
        public void Use(Action<TProxy> callback, Action<Exception> error)
        {
            this.Use(typeof(TProxy).Name, callback, error);
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">(Optional) The service endpoint configuration name. Uses <typeparamref name="TProxy" /> if not specified.</param>
        /// <param name="callback">The code block to execute.</param>
        public void Use(string endPointConfigurationName, Action<TProxy> callback)
        {
            this.Use(endPointConfigurationName, callback, (ex) => this.OnErrorOccured(typeof(TProxy), ex));
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">(Optional) The service endpoint configuration name. Uses <typeparamref name="TProxy" /> if not specified.</param>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error callback.</param>
        /// <exception cref="System.ArgumentNullException">endPointConfigurationName</exception>
        public void Use(string endPointConfigurationName, Action<TProxy> callback, Action<Exception> error)
        {
            if (string.IsNullOrEmpty(endPointConfigurationName))
            {
                throw new ArgumentNullException("endPointConfigurationName");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            var channelFactory = new ChannelFactory<TProxy>(endPointConfigurationName);

            this.Run(channelFactory, callback, error);
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        public async Task Use(Func<TProxy, Task> callback)
        {
            await this.Use(typeof(TProxy).Name, callback);
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error callback.</param>
        public async Task Use(Func<TProxy, Task> callback, Action<Exception> error)
        {
            await this.Use(typeof(TProxy).Name, callback, error);
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">(Optional) The service endpoint configuration name. Uses <typeparamref name="TProxy" /> if not specified.</param>
        /// <param name="callback">The code block to execute.</param>
        public async Task Use(string endPointConfigurationName, Func<TProxy, Task> callback)
        {
            await this.Use(endPointConfigurationName, callback, (ex) => this.OnErrorOccured(typeof(TProxy), ex));
        }

        /// <summary>
        /// Using statement for proxy.
        /// </summary>
        /// <param name="endPointConfigurationName">(Optional) The service endpoint configuration name. Uses <typeparamref name="TProxy" /> if not specified.</param>
        /// <param name="callback">The code block to execute.</param>
        /// <param name="error">The error callback.</param>
        /// <exception cref="System.ArgumentNullException">endPointConfigurationName</exception>
        public async Task Use(string endPointConfigurationName, Func<TProxy, Task> callback, Action<Exception> error)
        {
            if (string.IsNullOrEmpty(endPointConfigurationName))
            {
                throw new ArgumentNullException("endPointConfigurationName");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            var channelFactory = new ChannelFactory<TProxy>(endPointConfigurationName);

            await this.Run(channelFactory, callback, error);
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
            if (this.ErrorOccured != null)
            {
                this.ErrorOccured(this, new ServiceProxyErrorEventArgs(typeof(TProxy), exception));
            }
        }

        /// <summary>
        /// Runs the specified channel factory.
        /// </summary>
        /// <param name="channelFactory">The channel factory.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="error">The error callback.</param>
        private void Run(ChannelFactory<TProxy> channelFactory, Action<TProxy> callback, Action<Exception> error)
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

                error(ex);

                throw;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                error(ex);

                proxy.Abort();
            }
            catch (MessageSecurityException ex)
            {
                error(ex);

                throw;
            }
            catch (ChannelTerminatedException ex)
            {
                error(ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (ServerTooBusyException ex)
            {
                error(ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (EndpointNotFoundException ex)
            {
                error(ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (FaultException ex)
            {
                error(ex);

                proxy.Abort();
            }
            catch (CommunicationException ex)
            {
                error(ex);

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

                error(ex);

                proxy.Abort();
            }
            catch (ObjectDisposedException ex)
            {
                // TODO: Handle this duplex callback exception. Occurs when client disappears.   
                // Source: http://stackoverflow.com/questions/1427926/detecting-client-death-in-wcf-duplex-contracts/1428238#1428238 

                error(ex);
            }
            finally
            {
                if (!success)
                {
                    proxy.Abort();
                }
            }
        }

        /// <summary>
        /// Runs the specified channel factory.
        /// </summary>
        /// <param name="channelFactory">The channel factory.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="error">The error callback.</param>
        private async Task Run(ChannelFactory<TProxy> channelFactory, Func<TProxy, Task> callback, Action<Exception> error)
        {
            var proxy = (IClientChannel)channelFactory.CreateChannel();

            var success = false;

            try
            {
                await callback((TProxy)proxy);

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

                error(ex);

                throw;
            }
            catch (CommunicationObjectFaultedException ex)
            {
                error(ex);

                proxy.Abort();
            }
            catch (MessageSecurityException ex)
            {
                error(ex);

                throw;
            }
            catch (ChannelTerminatedException ex)
            {
                error(ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (ServerTooBusyException ex)
            {
                error(ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (EndpointNotFoundException ex)
            {
                error(ex);

                proxy.Abort(); // Possibly retry? 
            }
            catch (FaultException ex)
            {
                error(ex);

                proxy.Abort();
            }
            catch (CommunicationException ex)
            {
                error(ex);

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

                error(ex);

                proxy.Abort();
            }
            catch (ObjectDisposedException ex)
            {
                // TODO: Handle this duplex callback exception. Occurs when client disappears.   
                // Source: http://stackoverflow.com/questions/1427926/detecting-client-death-in-wcf-duplex-contracts/1428238#1428238 

                error(ex);
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