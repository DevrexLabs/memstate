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

Task("Clean")
  .Does(()=>{
    var directoriesToClean = GetDirectories("./**/bin/Debug");
    CleanDirectories(directoriesToClean);
  });

Task("Publish")
  .IsDependentOn("Build")
  .Does(() =>{
    NuGetPush(GetFiles("./**/Memstate.*.nupkg"), new NuGetPushSettings {
       Source = "https://www.nuget.org/api/v2/package"
    });
  });
RunTarget(target);
