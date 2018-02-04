# Quickstart

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

Example : [QuickStartClasses\CustomerModel.cs](QuickStartClasses/CustomerModel.cs)

## Create commands
Commands are used to update the model. Derive from `Command<M>` or `Command<M,R>` where `M` is the type of your model and `R` is the result type

Example : [QuickStartClasses\EarnPoints.cs](QuickStartClasses/EarnPoints.cs)
Example : [QuickStartClasses\SpendPoints.cs](QuickStartClasses/SpendPoints.cs)

## Hosting the engine
`Engine.For<M>()` will create an initial model, write it as a snapshot to disk and then return an engine ready to execute commands and queries.

```csharp
IEngine<TaskModel> engine = Engine.For<CustomerModel>();
```

## Executing commands
Create a command object and pass it to the engine for execution:

```csharp
// The engine will execute the command against the model and persist to the command journal.

var id = new CustomerID(1);
await model.ExecuteAsync(new EarnPoints(id, 100));
// or
model.Execute(new EarnPoints(id, 100));
```

## Executing queries
You can use either ad-hoc linq queries passed as lambdas to the engine or you can write strongly typed query classes.

```csharp
// can't serialize lambdas, need local engine
var localEngine = (ILocalEngine<TaskModel>) engine;

//ad-hoc lambda query
var tasksDue = localEngine.Execute(db => db.Tasks
  .Where(t => DateTime.Today > t.DueBy)
  .OrderByDesc(t => t.DueDy).ToArray());

[Serializable]
public class TasksDueBefore : Memstate.Core.Query<TaskModel, IEnumerable<Task>>
{
  public DateTime DueDate{get;set;}

  public IEnumerable<Task> override Execute(TaskModel model)
  {
    return model.Tasks.Where(t => DueDate > t.DueBy).ToArray();
  }
}
// executing the strongly typed query
var query = new TasksDueBefore{DueDate = DateTime.Today.AddDays(1)};
IEnumerable<Task> tasksDue = engine.Execute(query);
```

## Summary
We've covered the absolute basics here, but essentially there's not much more to developing than defining the model, and writing commands and queries. We used explicit transactions, an anemic model and the transaction script pattern. Next, you might wan't to check out [implicit transactions](../../modeling/proxy), where commands and queries are derived from methods on the model eliminating the need to explicitly author commands and queries.
