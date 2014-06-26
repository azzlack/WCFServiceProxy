namespace WCFServiceProxy.Tests
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading.Tasks;

    using EyeCatch.WCF.ServiceProxy;

    using NUnit.Framework;

    [TestFixture]
    public class ServiceProxyWrapperTests
    {
        private ServiceHost serviceHost;

        private IServiceProxyWrapper<IMockService> proxy;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            this.serviceHost = new ServiceHost(typeof(MockService), new Uri("http://localhost:80/Temporary_Listen_Addresses/ServiceProxyWrapperTests"));
            
            var metadataBehavior = new ServiceMetadataBehavior
                               {
                                   HttpGetEnabled = true,
                                   MetadataExporter =
                                       {
                                           PolicyVersion = PolicyVersion.Policy15
                                       }
                               };
            this.serviceHost.Description.Behaviors.Add(metadataBehavior);

            this.serviceHost.Open();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            this.serviceHost.Close();
        }

        [SetUp]
        public void SetUp()
        {
            // Create client wrapper
            this.proxy = ServiceProxyFactory.Create<IMockService>();
        }

        [Test]
        public void Use_WhenWrapperWorks_ShouldReturnClientAsParameter()
        {
            this.proxy.Use(
                async (client) =>
                    {
                        await client.GetDataAsync();

                        Assert.IsAssignableFrom<IMockService>(client);
                    });
        }

        [Test]
        public async void Use_WhenWrapperThrowsError_ShouldReturnEnterErrorCallback()
        {
            Exception error = null;

            await this.proxy.Use(
                async (client) =>
                    {
                        await client.GetErrorAsync();
                    },
                (ex) =>
                    {
                        error = ex;
                    });

            Assert.IsNotNull(error);
        }
    }
}
