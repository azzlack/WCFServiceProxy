WCF/SOAP Service Proxy
=====================================

Service proxy for consuming WCF/SOAP services.

[![NuGet Version](https://img.shields.io/nuget/v/WCFServiceProxy.svg)](https://www.nuget.org/packages/WCFServiceProxy)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WCFServiceProxy.svg)](https://www.nuget.org/packages/WCFServiceProxy)

This proxy solves some common problems when dealing with SOAP-based Windows Communication Foundation services.

Features:
- Auto-closing of client
- Automatic client and channel disposing
- Handles all types of exceptions
- Error tracing

This library works with the "Add Service Reference" functionality, as well as using a custom interface.

### How to use the factory
```csharp
// The simplest way to use it
ServiceProxyFactory.Create<IMyServiceClientInterface>().Use(
	async (client) => {
		await client.GetDataAsync(); // Do your magic here
	});

// Custom error callback
ServiceProxyFactory.Create<IMyServiceClientInterface>().Use(
	async (client) => {
		await client.GetDataAsync(); // Call the service endpoints here
	}, (exception) => {
		throw exception; // Do something if an error occurs
	});
```

### Dependency Injection support
```csharp
// Using SimpleInjector
// ErFrChannel is the interface for the service. Usually the same as the name attribute in your config file.
container.RegisterWebApiRequest<IServiceProxyWrapper<ErFrChannel>>(ServiceProxyFactory.Create<ErFrChannel>);
```
