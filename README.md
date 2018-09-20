# Memstate

[Detailed Documentation](docs) | [quickstart](/src/Memstate.Docs.GettingStarted/QuickStart)

[![Join the chat at https://gitter.im/memstate/lobby](https://badges.gitter.im/DevrexLabs/memstate.svg)](https://gitter.im/memstate/lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

# What is Memstate?
In-memory event-sourced ACID-transactional replicated object graph engine. What? Can you say that again? Ok, it's an application server that keeps all your data in RAM. It runs without a database because all the transactions are recorded in a log and used to restore the state of the application at startup or when setting up a replica. You define the object model, commands and queries using C#. Besides being very simple Memstate is also very fast and will outperform any relational database.

# Why Memstate?
Your data fits in RAM. Moving bits and pieces back and forth between disk and memory is slow, difficult to maintain and error prone. Use Memstate to structure and manage your data in-memory, providing transparent persistence, high performance, high availability, concurrency control and transactions with strong ACID guarantees.

Memstate has many possible use cases but is designed primarily to handle complex OLTP workloads by replacing the datastore, data access layer and domain layer in a typical enterprise application.

The benefits are many:

* Productivity/Cost - *Way* less code to write and maintain, typically less than 50%
* Quality - strongly typed and compiled C# means less bugs and problems
* Performance - In-memory is orders of magnitude faster than reading/writing to disk
* Faster time to market, faster feedback cycles
* Time travel! - restore to any point in time to debug, run a query
* Distributed and highly available
* Full history of every single change
* Scalability - Scale out to handle massive 
* Cloud and microservices ready -  Docker, VM
* Open source
* Cross platform - Built with .NET Core and runs on Linux, Mac and Windows
* Free for commercial use - LGPL license

## So what's the catch?
Your data model needs to fit in RAM on a single server or VM. When using replication, each node has a copy of the entire model. This means Memstate can't scale beyond the limits of a VM with maximum amount of RAM. At the time of writing AWS offers 4TB instances which should be sufficient for most OLTP workloads.

As the journal grows over time, replaying billions of commands can take a significant amount of time which means restoring a node or setting up a new replica could take several hours.

**Did we mention how simple Memstate is to use?**

```csharp
        [Test]
        public async Task Most_compact_start_using_all_default_configurations()
        {
            var engine = await Engine.Start<LoyaltyDB>();
            Print(await engine.Execute(new InitCustomer(10, 10)));
            Print(await engine.Execute(new InitCustomer(20, 20)));
            Print(await engine.Execute(new TransferPoints(10, 20, 5)));
            await engine.DisposeAsync();

            // Produces the following output :)

            /*             
            Customer[10] balance 10 points.
            Customer[20] balance 20 points.
            Sent 5 points. | Sender, Customer[10] balance 5 points. | Recipient, Customer[20] balance 25 points.
            */
        }

        private void Print(object o)
        {
            Console.WriteLine(o.ToString());
        }
```

## Quickstart - getting started

[Quickstart](/src/Memstate.Docs.GettingStarted/QuickStart) | [Detailed Documentation](docs) 

## Governance, Support and Contributions
Memstate is an open source project sponsored and governed by Devrex Labs, an LLC based in Sweden.
Devrex Labs provide commercial support and consulting services for OrigoDB and Memstate. Contributions are welcome, check out the issues (or add a new one) and discuss with the core team before getting started. You will need to sign our CLA using the  [![CLA assistant](https://cla-assistant.io/readme/badge/DevrexLabs/memstate)](https://cla-assistant.io/DevrexLabs/memstate) 

## Background and Objectives
Devrex Labs also maintain OrigoDB, an in-memory database engine for .NET Framework. Memstate is a redesign based on our experience building and working with OrigoDB, taking the best parts, learning from mistakes and setting some new objectives. This section takes on a comparison with OrigoDB perspective.

* **Performance** - We're aiming at 100K commands per second. Note that this is *write* operations, bounded by disk i/o. Read operations are cpu bound and can hit the millions per second depending on how complex the model is. (OrigoDB will max out at 3K writes per second (WPS).)

* **Simplified Replication** - Memstate relies on a backing storage provider such as EventStore or PostgreSQL for message ordering. Each node simply subscribes to the ordered stream of commands from the underlying backing store and isn't aware of the other nodes. There is no primary or leader, each node can process both commands and queries.

Memstate's replication scheme has higher availability (because any node can accept writes) at the expense of some temporal inconsistency across the nodes.

* **Multi-platform support** - Memstate is a .NET Standard 2.x and runs on Linux, Mac and Windows.

* **Streaming** - We are aiming to support streaming of command/query results when the result type is `IEnumerable`. TBD

* **Reactive** - Command execution triggers sytem and user-defined events which are pushed to subscribing tcp clients.

* **Async processing model** - In order to achieve higher throughput, Memstate writes and flushes commands in batches, relying heavily on async/await for concurrency.

* **Cloud ready** - solid support for monitoring, control, observability, cloud based storage, prebaked VM images on Azure and AWS. - TBD

* **Docker support** - Memstate can run in a docker container (see Dockerfile) and we aim to provide official docker images. Did anyone say stateful microservices?

* **Smart configuration** - Using the new .NET Core configuration support, the default settings can be overriden with JSON files, environment variables or command line arguments.


# Features
## In-memory object graph
Your data fits in RAM. Define your own object model and operations (commands and queries) or use a built-in model such as redis, keyvalue, graph, relational, document, sparse array, etc. Also we have some cool geo-spatial types which you can use in your custom model.

## Event-sourced
All operations (commands) are written to persistent storage and used to restore the state of the in-memory object graph when a node starts up. The log is a complete audit trail of every single change to the system and can be used to recreate the state of the graph at any given point in time. A point-in-time snapshot can be used for queries or historical debugging.

## Real-time event notifications
Domain events emitted from within the model can be subscribed to (with optional server side filtering) by local or remote clients. This enables reactive applicatons with real time updates when data is changed.

## Configurable serialization formats
Choose JSON for readability and interoperability or a binary format for speed and smaller size. 

## ACID Transactions

* **Atomicity**   - The Command is the fundamental transactional unit. Command authors must ensure that the in-memory model is left unchanged in the case of an exception. Memstate can be configured to shut down in the case of a runtime exception.

* **Consistency** - Each command needs to be deterministic. It must depend only on it's parameters and the current state of the model. After command execution the model is in the next consistent state.

* **Isolation**  - Single writer/multiple reader model is the default. Commands execute one at a time,  thus fully serialized isolation level.

* **Durability**  - write-ahead logging of commands to a persistent storage backend. memstate is as durable as the backend. Choose from eventstore, postgres or file system.

## Configurable Storage Backend
Memstate relies on a backing storage provider for persistence and global message ordering. We currently support EventStore, PostgreSQL and plain old file system storage.

* **EventStore** - The recommended default backend, a perfect fit in terms of performance, durability, reliability, extensibility and interoperability.

* **PostgreSQL** - PostgreSQL notifications are used to push commands to the the replicas. PostgreSQL is the recommended choice if you're already running it and have your operations in place or want to use an enterprise-ready, fully managed instance on either AWS or Azure. 

* **File system** - simple append only journaling. Single memstate node only.

* **Kinesis** (AWS) - discontinued, end to end latency is too poor.

* **Kafka** - discontinued, end to end latency is too poor.

[Quickstart](/src/Memstate.Docs.GettingStarted/QuickStart) | [Detailed Documentation](docs) 