# Memstate Overview

At it's core Memstate is application server where the state of the application is kept in memory only. Here is an overview of the main components and terminology in a memstate powered system.

## The Model
The model represents the current state and behavior of the application and is the result of applying a sequence of operations starting from some inital state.

The model is usually domain specific and defined in a .NET language, it exists only in RAM while the memstate instance is running.

## The Engine
The engine is the main processing component. It restores the state of the application by replaying the sequence of commands from the journal and then processes commands and queries until it is disposed.

## Commands and Command Processing
A command is an operation that when executed, updates the model in some way. Commands are written in a .NET language, usually C#.

The engine sends commands to the storage provider. The storage provider appends them to the command journal. When the commands have been persisted to disk, they are returned to the engine. The engine executes each command one at a time, capturing the result and returning to the caller.

## Queries and Query Processing
A query is an object that when executed reads the in-memory model and returns some kind of result. Queries are written in a .NET language, often using LINQ.

## The Kernel
The kernel is an internal component at the heart of the engine. It coordinates command and query execution and is responsible for atomicity, consistency and isolation.

## Command Journal
An append-only log containing the sequence of commands leading up to the current state of the model.

## Storage Providers
A storage provider provides read and append access to the command journal. We have a simple file system storage provider that will append serialized commands to files on a file syste,.

## Multi-instance Storage Providers
A multi-instance storage provider accepts commands from multiple memstate instances, writes them to the journal and sends them back to each instance according to a global ordering.

## Memstate Server
A tcp server listening on port 3001 accepting commands, queries and event subscriptions from a RemoteClient.