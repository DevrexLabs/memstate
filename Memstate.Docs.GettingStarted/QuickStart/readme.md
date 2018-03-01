QuickStart Guide (getting started) | [Modelling](LoyaltyDB.cs)


# Quickstart Guide (getting started)

Here's a complete guide to get you started with developing your first Memstate application!
The following topics are covered:

* Adding the library to your project
* Define the in-memory model
* Create commands and queries
* Hosting the engine
* Executing commands
* Executing queries

## Add the library
The Memstate.Core library is a single assembly. Grab the latest Memstate.Core.dll from the [download page](/download) and add as a reference to your .NET project. Or install the [nuget package](http://nuget.org/List/Packages/Memstate) by typing `Install-Package Memstate -Version 0.1.0-alpha` in visual studio's package manager console.

## Define the in-memory model

Create a class that derives from `Model` and add members to hold data, usually collections. Mark the class and any referenced types with the `Serializable` attribute. An instance of this class is your in-memory database.

* example : [LoyaltyDB.cs](LoyaltyDB.cs)

```csharp
    [Serializable]
    public class LoyaltyDB
    {
        public LoyaltyDB() {}
        public IDictionary<int, Customer> Customers { get; } = new Dictionary<int, Customer>();
    }
```

## Create commands

Commands are used to update the model. Derive from `Command<M>` or `Command<M,R>` where `M` is the type of your model and `R` is the result type

* example : [SpendPoints.cs](Commands/SpendPoints.cs)
* example : [EarnPoints.cs](Commands/EarnPoints.cs)

```csharp
    public class EarnPoints : Command<LoyaltyDB, Customer>
    {
        public EarnPoints()
        {
        }

        public EarnPoints(int id, int points)
        {
            ID = id;
            Points = points;
        }

        public int ID { get; }
        public int Points { get; }

        // it is safe to return a live customer object linked to the Model because
        // (1) the class is serializable and a remote client will get a serialized copy
        // and (2) in this particular case Customer is immutable.
        // if you have mutable classes, then please rather return a view, e.g. CustomerBalance or CustomerView class 

        public override Customer Execute(LoyaltyDB model)
        {
            var customer = model.Customers[ID];
            var newPoints = customer.LoyaltyPointBalance + Points;
            var customerWithNewBalance = new Customer(ID, newPoints);
            model.Customers[ID] = customerWithNewBalance;
            return customerWithNewBalance;
        }
    }
```

## Hosting the engine

`new EngineBuilder(settings).BuildAsync<T>()` will create an initial model, write it as a snapshot to disk and then return an engine ready to execute commands and queries.

```csharp
  var settings = new MemstateSettings { StreamName = "LoyaltyDbFile" };
  var db = await new EngineBuilder(settings).BuildAsync<LoyaltyDB>().ConfigureAwait(false);
```

## Executing commands

Create a command object and pass it to the engine for execution:

```csharp
// The engine will execute the command against the model and persist to the command journal.

int id =1;
var customer = await db.ExecuteAsync(new EarnPoints(id, 100));
Console.WriteLine($"your new balance is {customer.LoyaltyPoints} points.");

// or
var customer = db.Execute(new EarnPoints(id, 100));
```

## Executing queries

You can  write strongly typed query classes.

```csharp

// executing a query
[Serializable]
public class Top10Customers : Query<LoyaltyDB, Customer[]>
{
    public override Customer[] Execute(LoyaltyDB db) {
        return db.Customers
            .OrderByDescending(c => c.Value.LoyaltyPointBalance)
            .Take(10).Select(c => c.Value).ToArray();
    }
}

Customer[] customers = engine.Execute(new Top10Customers());
```

## Transactions

(TBD) tests and documentation currently in progress.

## Summary

We've covered the absolute basics here, but essentially there's not much more to developing than defining the model, and writing commands and queries. We used explicit transactions, an anemic model and the transaction script pattern. Next, you might wan't to check out [implicit transactions](../../modeling/proxy), where commands and queries are derived from methods on the model eliminating the need to explicitly author commands and queries.

* For a full end to end working example see [QuickStartTests.cs](QuickStartTests.cs)