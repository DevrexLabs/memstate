#tool "nuget:?package=xunit.runner.console"

var target = Argument("target", "Default");

Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("Test");


Task("Build")
  .Does(() =>{
    DotNetBuild("./memstate.sln");
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() =>{
    XUnit2("./**/*.Tests.dll");
  });
RunTarget(target);
