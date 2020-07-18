## Change log

Key changes from version to version.

## Filestorage broken
Note: File Storage is currently broken, it will most probably deadlock.

## v2 Branch
* Removed `Config.Current` and `Config.Reset()`, config now needs to be passed around.
* Replaced custom configuration implementation with open source Fig
* Simplified Storage Provider API - Removed Subscription and SubscriptionSource types.
* Redesigned Engine start procedure
* Introduced Control Commands for engine to engine communication and synchronization
* Removed EngineBuilder, pushed replay into Engine.Start() method
* Engine states: NotStarted, S tarted, Running, Stopped, Disposing, Disposed.
* Engine.StateTransitioned event
* Added Azure Table Storage Provider

# v 0.7
* Built for .NET Standard 2.0 / .NET Core 3.1
* Can process up to 100k commands / s (a 30x improvement over OrigoDB)
* No sharding
* No snapshots
* Server and cluster open source
* No server to server synchronization, relies on underlying storage 

# OrigoDB
Predecessor to memstate, established 2008.
* Runs on .NET Framework 
* Supports sharding using `PartitionClient`
* Can process up to 3000 commands/s (i/o bound)
* Load from latest snapshot and subsequent commands
* Server: OrigoDB enterprise uses server to server replication, with a single primary and one or more readonly sync or async replicas.
