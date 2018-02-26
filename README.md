# Memstate

[![Join the chat at https://gitter.im/DevrexLabs/memstate](https://badges.gitter.im/DevrexLabs/memstate.svg)](https://gitter.im/DevrexLabs/memstate?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
In-memory event-sourced ACID-transactional distributed object graph engine. Memstate is written in C# for .NET Standard 1.6. Memstate runs either embedded in your process or as a separate server process.

[![CLA assistant](https://cla-assistant.io/readme/badge/DevrexLabs/memstate)](https://cla-assistant.io/DevrexLabs/memstate)

# Why?
Your data fits in RAM. Moving it back and forth between disk and memory is a pointless anachronism. Use Memstate to structure and manage your data in-memory, providing transparent persistence, concurrency control and transactions with strong ACID guarantees.

Memstate has many possible use cases but is designed primarily to handle complex OLTP workloads by replacing the datastore, data access and business logic layer in a typical enterprise application. It's also a great fit for stateful microservices.

The benefits of using Memstate are huge:
* Productivity/Cost - *Way* less code to write and maintain, typically less than 50%
* Quality - strongly typed and compiled C# means less bugs and problems
* Performance - In-memory is orders of magnitude faster than reading/writing to disk
* Faster time to market, faster feedback cycles
* Point in time debugging
* Time travel queries
* Distributed and highly available
* Open source
* Free for commercial use

**Did we mention how simple Memstate is to use?**

```csharp
    // host db engine using default settings - database will be save to disk as "DemoDatabase.journal"  
    var settings = new MemstateSettings { StreamName = "DemoDatabase" };
    var db = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>();

    int id = 1;
    await db.ExecuteAsync(new InitCustomerIfNotExist(id, 10));
    var customer = await db.ExecuteAsync(new EarnPoints(id, 20));
    Console.WriteLine($"your new balance is {customer.LoyaltyPoints} points."); 

    var top10 = await db.ExecuteAsync(new Top10Customers());
    top10.ToList().ForEach(Console.WriteLine);
```

## Quickstart - getting started

[Quick start getting started sample code here...](/Memstate.Examples.GettingStarted)

## Governance, Support and Contributions
Memstate is an open source project sponsored and governed by Devrex Labs, an LLC based in Sweden.
Devrex Labs provide commercial support and consulting services for OrigoDB and Memstate. Contributions are welcome, check out the issues (or add a new one) and discuss with the core team before getting submitting your PR.

## Background and Objectives
Devrex Labs also maintain OrigoDB, an in-memory database engine for .NET Framework. Memstate is a redesign based on our experience building and working with OrigoDB, taking the best parts, learning from mistakes and setting some new objectives.

* **Performance** - We're aiming at 100K WPS, which we almost reached our POC. Note that this is *write* operations, bounded by disk i/o. Read operations are cpu bound and in the millions per second depending on how complex the model is. (OrigoDB will max out at 3K writes per second (WPS).)

* **Simplified Replication** - Memstate relies on a distributed backing store for message ordering such as EventStore, Kafka or Kinesis. Each node simply subscribes to the stream of commands from the underlying backing store and isn't aware of the other nodes. There is no primary, each node can process both commands and queries. (Compared to OrigoDB Server which has it's own replicated state machine implementation with a designated primary and a number of replicas. There is no (solid) leader election or consensus algorithm in place, changing server roles is a manual process. )

    Memstate's new replication scheme has higher availability (because any node can accept writes) at the expense of some consistency: 

* **Server and Engine integrated in same project** - Memstate server and engine are now in the same libary and tested together. With OrigoDB the server is a commercial product based on the engine. It ships with the engine version it was compiled against, a source of many headaches. 

* **Multi-platform support** - Memstate is a .NET Standard 1.6 library. OrigoDB runs on .NET Framework only after dropping support for mono a few years ago. 

* **Interoperability** - Origo supports protobuf and newtonsoftjson and it can also expose an http api endpoint to the model and we hope to take this even further. OrigoDB relies heavily on `BinaryFormatter`, which is the default wire and disk format. 

* **Streaming** - We are aiming to support streaming when the result type is `IEnumerable`. OrigoDB results needed to be fully transferred to the client before processing could begin. 

* **Reactive** - Command execution triggers sytem and user-defined events which are pushed to subscribing tcp clients.

* **Async processing model** - In order to achieve higher throughput, Memstate writes and flushes commands in batches, using heavily on async/await all the way to the core. OrigoDB `Engine.Execute(Command)` is a blocking call that will serialize, append and flush the command to the log before executing. This imposes an ultimate limit on the number of commands per second processed. Internally, the OrigoDB engine is thread safe but does not spawn any threads or tasks of it's own. 

* **Cloud ready** - solid support for monitoring, control, observability, cloud based storage, prebaked VM images on Azure and AWS.

* **Docker support** - Memstate should readily run in a container and we aim to provide official docker images

* **Smart configuration** - Using the new .NET Core configuration support, we have built in default settings which can be overriden with JSON files, environment variables and the command line, in that order.

Ideally, memstate will at some point replace OrigoDB under .NET Standard 2.0

# Features
## In-memory object graph
Your data fits in RAM, you might not need a database. Define your own object model and operations or use a built-in model such as redis, keyvalue, graph, relational, document, sparse array, etc. Also we have some cool geo-spatial types which you can use in your custom model.

## Event-sourced
All operations (commands) are written to persistent storage and used to restore the state of the in-memory object graph when a node starts up. The log is a complete audit trail of every single change to the system and can be used to recreate the state of the graph at any given point in time. This point in time snapshot can be used for queries or historical debugging.

## ACID Transactions
* **Durability**  - write-ahead logging of commands to a persistent storage backend. memstate is as durable as the backend.

* **Isolation**   - configurable. Single writer/multiple reader model is the default, thus fully serialized isolation level.

* **Consistency** - TBD

* **Atomicity**   - Guaranteed for built-in models. In the face of command exceptions OrigoDB will assume the model is corrupt, discard it and recreate from the log. Memstate will simply throw exceptions back to the client and carry on as if nothing happened. It is up to the command author to guarantee that the model remains unchanged if the command fails.

## Storage backend
Distribution in a memstate cluster requires a message streaming backend. The backend provides a global ordering of commands. 

* **EventStore** - the recommended default backend, a perfect fit in terms of performance, durability, reliability, extensibility and interoperability.

* **PostgreSQL** - (in progress) We use PostgreSQL notifications to push commands to the nodes. This is promising because it will allow you to use RDS on AWS.

* **File system** - simple append only journaling. Single memstate node only.

* **Kinesis** (AWS) - discontinued, end to end latency is too poor.

* **Kafka** (evaluating) 


