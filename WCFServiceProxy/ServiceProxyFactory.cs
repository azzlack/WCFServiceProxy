namespace EyeCatch.WCF.ServiceProxy
{
    public class ServiceProxyFactory
    {
        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <typeparam name="TProxy">The type of the t proxy.</typeparam>
        /// <returns>IServiceProxyWrapper&lt;TProxy&gt;.</returns>
        public static IServiceProxyWrapper<TProxy> Create<TProxy>() where TProxy : class
        {
            return new ServiceProxyWrapper<TProxy>();
        }  
    }
}