///////////////////////////////////////////////////////////////////////////////
// TOOLS / ADDINS
///////////////////////////////////////////////////////////////////////////////

#tool paket:?package=GitVersion.CommandLine
#tool paket:?package=vswhere
#tool paket:?package=xunit.runner.console
#addin paket:?package=Cake.Figlet
#addin paket:?package=Cake.Paket

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
{
    target = "Default";
}

var configuration = Argument("configuration", "Release");
if (string.IsNullOrWhiteSpace(configuration))
{
    configuration = "Release";
}

var verbosity = Argument("verbosity", Verbosity.Normal);
if (string.IsNullOrWhiteSpace(configuration))
{
    verbosity = Verbosity.Normal;
}

///////////////////////////////////////////////////////////////////////////////
// PREPARATION
///////////////////////////////////////////////////////////////////////////////

var local = BuildSystem.IsLocalBuild;

// Set build version
if (local == false
    || verbosity == Verbosity.Verbose)
{
    GitVersion(new GitVersionSettings { OutputType = GitVersionOutput.BuildServer });
}
GitVersion gitVersion = GitVersion(new GitVersionSettings { OutputType = GitVersionOutput.Json });

var latestInstallationPath = VSWhereProducts("*", new VSWhereProductSettings { Version = "[\"15.0\",\"16.0\"]" }).FirstOrDefault();
var msBuildPath = latestInstallationPath.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");

var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var branchName = gitVersion.BranchName;
var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("dev", branchName);
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", branchName);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;

// Directories and Paths
var solution = "SimpleMusicPlayer.sln";
var publishDir = "./Publish";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    // Executed BEFORE the first task.

    if (!IsRunningOnWindows())
    {
        throw new NotImplementedException("SimpleMusicPlayer will only build on Windows because it's not possible to target WPF and Windows Forms from UNIX.");
    }

    Information(Figlet("SimpleMusicPlayer"));

    Information("Informational   Version: {0}", gitVersion.InformationalVersion);
    Information("SemVer          Version: {0}", gitVersion.SemVer);
    Information("AssemblySemVer  Version: {0}", gitVersion.AssemblySemVer);
    Information("MajorMinorPatch Version: {0}", gitVersion.MajorMinorPatch);
    Information("NuGet           Version: {0}", gitVersion.NuGetVersion);
    Information("IsLocalBuild           : {0}", local);
    Information("Branch                 : {0}", branchName);
    Information("Configuration          : {0}", configuration);
    Information("MSBuildPath            : {0}", msBuildPath);
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    //.ContinueOnError()
    .Does(() =>
{
    var directoriesToDelete = GetDirectories("./**/obj").Concat(GetDirectories("./**/bin")).Concat(GetDirectories("./**/Publish"));
    DeleteDirectories(directoriesToDelete, new DeleteDirectorySettings { Recursive = true, Force = true });
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    PaketRestore();

    var msBuildSettings = new MSBuildSettings { ToolPath = msBuildPath, ArgumentCustomization = args => args.Append("/m") };
    MSBuild(solution, msBuildSettings
            .UseToolVersion(MSBuildToolVersion.VS2015)
            //.SetConfiguration(configuration)
            .SetVerbosity(Verbosity.Minimal)
            .WithTarget("restore")
            );
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var msBuildSettings = new MSBuildSettings { ArgumentCustomization = args => args.Append("/m") };
    MSBuild(solution, msBuildSettings
            .UseToolVersion(MSBuildToolVersion.VS2015) // for now
            .SetMaxCpuCount(0)
            .SetConfiguration(configuration)
            .SetVerbosity(Verbosity.Normal)
            //.WithRestore() only with cake 0.28.x            
            .WithProperty("AssemblyVersion", gitVersion.AssemblySemVer)
            .WithProperty("FileVersion", gitVersion.AssemblySemFileVer)
            .WithProperty("InformationalVersion", gitVersion.InformationalVersion)
            );
});

Task("Zip")
    .WithCriteria(() => !isPullRequest)
    .Does(() =>
{
    EnsureDirectoryExists(Directory(publishDir));
    Zip("./bin/" + configuration, publishDir + $"/SimpleMusicPlayer-{configuration}-v{gitVersion.NuGetVersion}.zip");
});

Task("Tests")
    //.WithCriteria(() => !local)
    .Does(() =>
{
    XUnit2(
        "./SimpleMusicPlayer.Tests/bin/" + configuration + "/**/*.Tests.dll",
        new XUnit2Settings { ToolTimeout = TimeSpan.FromMinutes(5) }
    );
});

///////////////////////////////////////////////////////////////////////////////
// TASK TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

Task("appveyor")
    .IsDependentOn("Build")
    .IsDependentOn("Tests")
    .IsDependentOn("Zip");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);