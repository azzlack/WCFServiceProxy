namespace WCFServiceProxy.Tests
{
    using System.ServiceModel;
    using System.Threading.Tasks;

    public class MockService : IMockService
    {
        public async Task<string> GetDataAsync()
        {
            return await Task.Run(
                async () =>
                    {
                        await Task.Delay(1000);

                        return "Success";
                    });
        }

        public Task<string> GetErrorAsync()
        {
            throw new FaultException();
        }
    }

    [ServiceContract(ConfigurationName = "IMockService")]
    public interface IMockService
    {
        [OperationContract]
        Task<string> GetDataAsync();

        [OperationContract]
        Task<string> GetErrorAsync();
    }
}