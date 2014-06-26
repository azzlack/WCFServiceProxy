WCF/SOAP Service Proxy
=====================================

Service proxy for consuming WCF/SOAP services.

This proxy solves some common problems when dealing with SOAP-based Windows Communication Foundation services.

Features:
- Auto-closing of client
- Automatic client and channel disposing
- Handles all types of exceptions
- Error tracing

This completely replaces the "Add Service Reference" functionality. 
That means your WCF service must implement an interface, and the same interface must bu available for your client application. 

### How to use
This wrapper is intended to mimic a `using` statement.

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