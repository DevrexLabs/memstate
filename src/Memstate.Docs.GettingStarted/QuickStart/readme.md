# Quickstart Guide (getting started)

[QuickStart Guide](../QuickStart)  | [Modelling](../Modelling) | [Built in Models](../BuiltInModels) | [Configuration](../Configuration) | [Storage](../Storage) | [Client API](../ClientAPI) | [Security](../Security)

Here's a complete guide to get you started with developing your first Memstate application!
The following topics are covered:

* Create a project
* Adding the library to your project
* Define the in-memory model
* Create commands and queries
* Hosting the engine
* Executing commands
* Executing queries

## Creating a project for your custom domain model
Normally you would create a separate class library where you define the domain model. But for the quick start create a simple console application for either .NET Core or .NET Framework.

## Add a reference to Memstate libraries
Add a reference to  [Memstate.All nuget package](http://nuget.org/List/Packages/Memstate.All)

> install-package Memstate.All

## Define the in-memory model
Define a root class to serve as the model and any supporting types such as entities. An instance of this class will be kept in-memory and represents the state of your application.

* example : [LoyaltyDB.cs](LoyaltyDB.cs)

```csharp
    public class LoyaltyModel
    {
        public LoyaltyModel() {}
        public IDictionary<int, Customer> Customers { get; } = new Dictionary<int, Customer>();
    }
```

* example : [Customer.cs](Customer.cs)

```csharp
    public class Customer
    {
    }
```

## Create commands

Commands are used to update the model. Derive from `Command<M>` or `Command<M,R>` where `M` is the type of your model and `R` is the result type

* example : [SpendPoints.cs](Commands/SpendPoints.cs)
* example : [EarnPoints.cs](Commands/EarnPoints.cs)

```csharp
    public class EarnPoints : Command<LoyaltyModel, Customer>
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

        public override Customer Execute(LoyaltyModel model)
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
  var engine = await new EngineBuilder(settings).BuildAsync<LoyaltyModel>();
```

## Executing commands

Create a command object and pass it to the engine for execution:

```csharp
// The engine will execute the command against the model and persist to the command journal.

int id = 1;
var earnPointsCommand = new EarnPoints(id,100);
var customer = await engine.Execute(earnPointsCommand);
Console.WriteLine($"your new balance is {customer.LoyaltyPoints} points.");
```

## Executing queries

You can  write strongly typed query classes.

```csharp

// executing a query
[Serializable]
public class Top10Customers : Query<LoyaltyModel, Customer[]>
{
    public override Customer[] Execute(LoyaltyModel model) {
        return model.Customers
            .OrderByDescending(c => c.Value.LoyaltyPointBalance)
            .Take(10).Select(c => c.Value).ToArray();
    }
}

Customer[] customers = await engine.Execute(new Top10Customers());
```

## Transactions

(TBD) tests and documentation currently in progress.

## Summary

We've covered the absolute basics here, but essentially there's not much more to developing than defining the model, and writing commands and queries. We used explicit transactions, an anemic model and the transaction script pattern. Next, you might wan't to check out [implicit transactions](../../modeling/proxy), where commands and queries are derived from methods on the model eliminating the need to explicitly author commands and queries.

* For a full end to end working example see [QuickStartTests.cs](QuickStartTests.cs)
