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
        public Customer(int id, int loyaltyPointBalance)
        {
            ID = id;
            LoyaltyPointBalance = loyaltyPointBalance;
        }

        public int ID { get; }
        public int LoyaltyPointBalance { get; }

        public override string ToString() => $"Customer[{ID}] balance {LoyaltyPointBalance} points.";
    }

## Create commands

Commands are used to update the model. Derive from `Command<M>` or `Command<M,R>` where `M` is the type of your model and `R` is the result type

* example : [SpendPoints.cs](Commands/SpendPoints.cs)
* example : [EarnPoints.cs](Commands/EarnPoints.cs)

```csharp
    public class EarnPoints : Command<LoyaltyModel, Customer>
    {
        public EarnPoints(int customerId, int points)
        {
            CustomerId = customerId;
            Points = points;
        }

        public int CustomerID { get; }
        public int Points { get; }

        // it is safe to return a live customer object linked to the Model because
        // (1) the class is serializable and a remote client will get a serialized copy
        // and (2) in this particular case Customer is immutable.
        // if you have mutable classes, then please rather return a view, e.g. CustomerBalance or CustomerView class 

        public override Customer Execute(LoyaltyModel model)
        {
            var customer = model.Customers[ID];
            var newPoints = customer.LoyaltyPointBalance + Points;
            var customerWithNewBalance = new Customer(CustomerId, newPoints);
            model.Customers[CustomerId] = customerWithNewBalance;
            return customerWithNewBalance;
        }
    }
```

## Hosting the engine

The following code will create an initial model, write it as a snapshot to disk and then return an engine ready to execute commands and queries.

```csharp
    var engine = await Engine.Start<LoyaltyDB>();
```

## Executing commands

Creating command objects and passing them to the engine for execution. The following code initialises a new customer with id[10] with 100 loyalty points. Then we execute the EarnPoints command, and finnally we write out the customer balance.

Note how the `EarnPoints` Command, returns an immutable `Customer` object with the new balance as a return value.

```csharp

await engine.Execute(new InitCustomer(10, 100));
var customer = await engine.Execute(new EarnPoints(id,100));
Console.WriteLine($"your new balance is {customer.LoyaltyPoints} points.");
```

## Executing queries

You can  write strongly typed query classes.

```csharp

// executing a query
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
