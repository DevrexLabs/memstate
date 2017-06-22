
# Memstate
In-memory event-sourced ACID-transactional distributed object graph engine. Memstate is written in C# for .NET Standard 1.4. Memstate runs either embedded in your process or as a separate server process.

## Contributions and governance
Memstate is an open source project sponsored and governed by Devrex Labs, a LLC based in Sweden. Devrex Labs provides commercial support and consulting services. Contributions are welcome, check out the issues or submit a feature request.


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

