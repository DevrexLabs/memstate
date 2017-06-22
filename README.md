
# Memstate
In-memory event-sourced ACID-transactional distributed object graph engine. Memstate is written in C# for .NET Standard 1.4. Memstate runs either embedded in your process or as a separate server process.

## UNDER CONSTRUCTION

Memstate is under construction but will be used in a larger project with a real deadline mid September 2017. We will be setting up a continous release pipeline for nuget packages and docker images during June/July 2017 and then work towards a stable 1.0 release during September.

## Governance, Support and Contributions
Memstate is an open source project sponsored and governed by Devrex Labs, a LLC based in Sweden.
Devrex Labs provides commercial support and consulting services. Contributions are welcome, check out the issues or submit a feature request.

## Background and Objectives
We also maintain OrigoDB, an in-memory database engine for .NET Framework.

Memstate is a redesign based on our experience building and working with OrigoDB, taking the best parts and setting some new objectives:
* Performance - OrigoDB does 3K TPS, we're now aiming at 100K per node
* Simplified replication - OrigoDB Server has it's own replicated state machine implementation, memstate will rely on some distributed backing store for message ordering such as EventStore, Kafka or Kinesis
* Server and Engine integrated in same project
* Better multi-platform support - moving to to .NET Standard (Core)
* Interoperability - Besides the native .NET client we are supporting JSON over HTTP and JSON Web Sockets
* Reactive - real-time push event notifications
* Cloud ready - monitoring, control, service discovery, cloud based storage
* Docker support - we will provide official docker images


# Features
## In-memory object graph
Your data fits in RAM, you might not need a database. Define your own object model and operations or use a built-in model such as redis, keyvalue, graph, relational or document, sparse array, etc

## Event-sourced
All operations (commands) are written to persistent storage and used to restore the state of the in-memory object graph when a node starts up. The log is a complete audit trail of every single change to the system and can be used to recreate the state of the graph at any given point in time. This point in time snapshot can be used for queries or historical debugging.

## ACID Transactions
* Durability  - write-ahead logging of commands to a persistent storage backend
* Isolation   - Commands are applied linearly to the graph
* Consistency - TBD
* Atomicity   - Guaranteed for built-in models

## Messaging backend
Distribution in a memstate cluster requires a message streaming backend. The backend provides a global ordering of commands.

* EventStore
* Kinesis (AWS)
* Kafka (planned)

## Permanent Storage Backend

* EventStore
* DynamoDB (AWS)
* File system (planned)
* RDBMS (planned)

