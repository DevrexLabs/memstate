# Running memstate

Memstate can run in a number or modes:

* Server mode
* Docker mode
* Embedded Engine mode
* Embedded Server mode

## Server mode
Memstate Server can run in a standalone process. You connect to the server using a RemoteClient.

TODO - starting the server, adding application specific assemblies, choosing 32

## Docker mode
Memstate Server can run in a docker container. You can use the image at devrexlabs/memstate

## Embedded Engine
In embedded engine mode Memstate runs within your application process. It can be a web, console, windows or Windows Service application. Your application communicates with the engine using a direct LocalClient connection.

## Embedded Server
Embedded Server mode the server runs within your process, similar to Embedded Engine mode. You can connect directly to the engine from within the application using a LocalClient connection but also connect to server endpoints from outside the application.

# Multiple instances
Using a supported backing storage provider, you can run multiple instances of Memstate using any combination of modes. Each instance will have a copy of the in-memory application state model and can process both commands and queries.a

Storage providers that support multiple instances:
* Event Store - [Documentation](eventstore.md)
* PostgreSQL - [Documentation](postgres.md)

# Choosing 32-bit or 64-bit mode
The maximum amount of memory available to a 32-bit process is 4GB, in practice it is usually lower. If your in-memory model is larger than a few GB then you should choose 64-bit.