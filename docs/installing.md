# Installing
Install stable memstate releases from nuget using:

```install-package memstate.all``

The bundle includes the core library and modules such as storage providers and serializers. If you wish to choose only a select set of modules, they can be installed independently. The current list of modules is:

* Memstate.Core - the core engine, server and client library
* Memstate.EventStore - Storage provider for EventStore v4
* Memstate.Postgres - Storage provider
* Memstate.JsonNet - Serializer based on Newtonsoft JSON
* Memstate.Wire - Binary serializer based on Wire by @RogerAlsing
* Memstate.Azure - Storage provider for Azure Table Storage
* Memstate.Pravega - Storage provider for Pravega

We use appveyor for continuous integration and create nuget packages for every commit to master. Access the latest packages using the following url: https://ci.appveyor.com/nuget/memstate

## Versioning
Packages with the same versions are developed, tested and released together as a unit. Don't mix versions.
