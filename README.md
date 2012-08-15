# WCF Service Proxy #

---

Service proxy for consuming WCF services.

This proxy solves some common problems when dealing with Windows Communication Foundation services.

Features:
- Auto-closing of client
- Automatic client and channel disposing
- Handles all types of exceptions
- Error tracing

This completely replaces the "Add Service Reference" functionality. 
That means your WCF service must implement an interface, and the same interface must bu available for your client application. 