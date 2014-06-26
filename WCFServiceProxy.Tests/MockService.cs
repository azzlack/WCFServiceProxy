namespace WCFServiceProxy.Tests
{
    using System.ServiceModel;
    using System.Threading.Tasks;

    public class MockService : IMockService
    {
        public string GetData()
        {
            return "Success";
        }

        public async Task<string> GetAsyncData()
        {
            return await Task.FromResult("Success");
        }

        public string GetError()
        {
            throw new FaultException();
        }

        public Task<string> GetAsyncError()
        {
            throw new FaultException();
        }
    }

    [ServiceContract(ConfigurationName = "IMockService")]
    public interface IMockService
    {
        [OperationContract]
        string GetData();

        [OperationContract]
        Task<string> GetAsyncData();

        [OperationContract]
        string GetError();

        [OperationContract]
        Task<string> GetAsyncError();
    }
}