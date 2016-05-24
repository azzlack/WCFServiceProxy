namespace WCFServiceProxy.Tests
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

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
        public void Proxy_WhenWrapperWorks_ShouldReturnProxy()
        {
            var proxy = this.proxy.Proxy;

            Assert.IsTrue(proxy is IMockService);
        }

        [Test]
        public void Use_WhenWrapperWorks_ShouldReturnClientAsParameter()
        {
            var data = string.Empty;

            this.proxy.Use(
                (client) =>
                {
                    data = client.GetData();

                    Assert.IsAssignableFrom<IMockService>(client);
                });

            Assert.AreEqual("Success", data);
        }

        [Test]
        public void Use_WhenWrapperThrowsError_ShouldEnterErrorCallback()
        {
            Exception error = null;

            this.proxy.Use(
                (client) =>
                {
                    client.GetError();
                },
                (ex) =>
                {
                    error = ex;
                });

            Assert.IsInstanceOf<FaultException>(error);
        }

        [Test]
        public async void UseAsync_WhenWrapperWorks_ShouldReturnClientAsParameter()
        {
            var data = string.Empty;

            await this.proxy.Use(
                async (client) =>
                    {
                        data = await client.GetAsyncData();

                        Assert.IsAssignableFrom<IMockService>(client);
                    });

            Assert.AreEqual("Success", data);
        }

        [Test]
        public async void UseAsync_WhenWrapperThrowsError_ShouldEnterErrorCallback()
        {
            Exception error = null;

            await this.proxy.Use(
                async (client) =>
                    {
                        await client.GetAsyncError();
                    },
                (ex) =>
                    {
                        error = ex;
                    });

            Assert.IsInstanceOf<FaultException>(error);
        }

        [Test]
        public async void Configure_WhenConfigurationWorks_ShouldReturnResult()
        {
            var ran = false;

            await this.proxy.Configure(
                x =>
                x.Endpoint.Address =
                new EndpointAddress(
                    new Uri("http://localhost"),
                    x.Endpoint.Address.Identity,
                    x.Endpoint.Address.Headers)).Use(
                        async (client) =>
                            {
                                ran = true;

                                Assert.That(
                                    () => client.GetData(),
                                    Throws.Exception.InstanceOf<CommunicationException>());
                            });

            Assert.IsTrue(ran, "Client action did not run");
        }
    }
}
