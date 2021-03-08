///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release/netstandard2.0");
var branchName      = "master";

Information("Branch is '{0}'", branchName);

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var isLocalBuild        = !AppVeyor.IsRunningOnAppVeyor;
var isPullRequest       = AppVeyor.Environment.PullRequest.IsPullRequest;
var solutions           = GetFiles("./**/*.sln");
var solutionDirs        = solutions.Select(solution => solution.GetDirectory());
var semVersion          = "0.0.4.000";
var version             = semVersion;
var binDir              = "./src/Cake.ProjHelpers/Cake.ProjHelpers/bin/" + configuration;
var nugetRoot           = "./nuget/";
var isMasterBranch      = branchName == "master";

var assemblyInfo        = new AssemblyInfoSettings {
                                Title                   = "Cake.ProjHelpers",
                                Description             = "Cake AddIn to embed files into .proj files",
                                Product                 = "Cake.ProjHelpers",
                                Company                 = "Ori Almog",
                                Version                 = version,
                                FileVersion             = version,
                                InformationalVersion    = semVersion,
                                Copyright               = string.Format("Copyright © Ori Almog {0}", DateTime.Now.Year),
                                CLSCompliant            = true
                            };
var nuspecFiles = new [] 
{
    new NuSpecContent {Source = @"netstandard2.0\Cake.ProjHelpers.dll"},
    new NuSpecContent {Source = @"netstandard2.0\Cake.ProjHelpers.deps.json"}
}; 
var nuGetPackSettings   = new NuGetPackSettings {
                                Id                      = assemblyInfo.Product,
                                Version                 = assemblyInfo.InformationalVersion,
                                Title                   = assemblyInfo.Title,
                                Authors                 = new[] {assemblyInfo.Company},
                                Owners                  = new[] {assemblyInfo.Company},
                                Description             = assemblyInfo.Description,
                                Summary                 = "Cake AddIn to embed files into .proj files", 
                                ProjectUrl              = new Uri("https://github.com/orialmog/Cake.ProjHelpers"),
                                IconUrl                 = new Uri("https://cdn.rawgit.com/cake-contrib/graphics/a5cf0f881c390650144b2243ae551d5b9f836196/png/cake-contrib-medium.png"),
                                LicenseUrl              = new Uri("https://github.com/orialmog/Cake.ProjHelpers/blob/master/LICENSE"),
                                Copyright               = assemblyInfo.Copyright,
                                Tags                    = new [] {"Cake", "Script", "Build", "Resources", "Embed", "Task"},
                                RequireLicenseAcceptance= false,        
                                Symbols                 = false,
                                NoPackageAnalysis       = true,
                                Files                   = nuspecFiles,
                                BasePath                = binDir, 
                                OutputDirectory         = nugetRoot
                            };

///////////////////////////////////////////////////////////////////////////////
// Output some information about the current build.
///////////////////////////////////////////////////////////////////////////////
var buildStartMessage = string.Format("Building version {0} of {1} ({2}).", version, assemblyInfo.Product, semVersion);
Information(buildStartMessage);


///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    
    Information("Cleaning {0}", nugetRoot);
    CleanDirectories(new DirectoryPath(nugetRoot).FullPath); 
       
    // Clean solution directories.
    foreach(var solutionDir in solutionDirs)
    {
        Information("Cleaning {0}", solutionDir);

        CleanDirectories(solutionDir + "/**/bin/" + configuration);
        CleanDirectories(solutionDir + "/**/obj/" + configuration);
    }
});

Task("Restore")
    .Does(() =>
{
    // Restore all NuGet packages.
    foreach(var solution in solutions)
    {
        Information("Restoring {0}", solution);
        NuGetRestore(solution);
    }
});


Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    // Build all solutions.
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);
        MSBuild(solution, settings =>  
                    .WithRestore()
                    .SetConfiguration(configuration));
    }
});

Task("Create-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (!System.IO.Directory.Exists(nugetRoot))
    {
        CreateDirectory(nugetRoot);
    }
    NuGetPack("./nuspec/Cake.ProjHelpers.nuspec", nuGetPackSettings);
}); 

 



Task("Publish-NuGet-Packages")
    .IsDependentOn("Create-NuGet-Packages")
    .WithCriteria(() => !isLocalBuild)
    .WithCriteria(() => !isPullRequest) 
    .Does(() =>
{
    var packages  = GetFiles("./nuget/*.nupkg");
    foreach (var package in packages)
    {
        Information(string.Format("Found {0}", package));

        // Push the package.
        string apiKey = EnvironmentVariable("NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("NUGET_API_KEY variable not found");
        }

        NuGetPush(package, new NuGetPushSettings {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = apiKey
            }); 
    }
}); 

Task("Default")
    .IsDependentOn("Create-NuGet-Packages");

Task("AppVeyor")
    .IsDependentOn("Publish-NuGet-Packages");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

 
