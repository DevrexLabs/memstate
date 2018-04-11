var target = Argument("target", "Default");

Task("Default")
  .IsDependentOn("Build");
  

Task("Build")
  .Does(() =>{
    DotNetBuild("./src/memstate.sln");
  });

Task("SystemTest")
  .Does(() =>{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = "Release"
    };

    DotNetCoreTest("./System.Test/System.Test.csproj", settings);
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
