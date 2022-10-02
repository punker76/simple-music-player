///////////////////////////////////////////////////////////////////////////////
// TOOLS / ADDINS
///////////////////////////////////////////////////////////////////////////////

#module nuget:?package=Cake.DotNetTool.Module&version=0.5.0
#tool dotnet:?package=GitVersion.Tool&version=5.6.3

#tool xunit.runner.console&version=2.4.1
#tool vswhere&version=2.8.4

#addin nuget:?package=Cake.Figlet&version=1.4.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument("verbosity", Verbosity.Minimal);

///////////////////////////////////////////////////////////////////////////////
// PREPARATION
///////////////////////////////////////////////////////////////////////////////

var repoName = "SimpleMusicPlayer";
var isLocal = BuildSystem.IsLocalBuild;

// Set build version
if (isLocal == false || verbosity == Verbosity.Verbose)
{
    GitVersion(new GitVersionSettings { OutputType = GitVersionOutput.BuildServer });
}
GitVersion gitVersion = GitVersion(new GitVersionSettings { OutputType = GitVersionOutput.Json });

var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var branchName = gitVersion.BranchName;
var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", branchName);
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("main", branchName);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;

var latestInstallationPath = VSWhereLatest(new VSWhereLatestSettings { IncludePrerelease = true });
var msBuildPath = latestInstallationPath.Combine("./MSBuild/Current/Bin");
var msBuildPathExe = msBuildPath.CombineWithFilePath("./MSBuild.exe");

if (FileExists(msBuildPathExe) == false)
{
    throw new NotImplementedException("You need at least Visual Studio 2019 to build this project.");
}

// Directories and Paths
var solution = "./src/SimpleMusicPlayer.sln";
var publishDir = "./Publish";
var testResultsDir = Directory("./TestResults");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    if (!IsRunningOnWindows())
    {
        throw new NotImplementedException($"{repoName} will only build on Windows because it's not possible to target WPF and Windows Forms from UNIX.");
    }

    Information(Figlet(repoName));

    Information("Informational   Version: {0}", gitVersion.InformationalVersion);
    Information("SemVer          Version: {0}", gitVersion.SemVer);
    Information("AssemblySemVer  Version: {0}", gitVersion.AssemblySemVer);
    Information("MajorMinorPatch Version: {0}", gitVersion.MajorMinorPatch);
    Information("NuGet           Version: {0}", gitVersion.NuGetVersion);
    Information("IsLocalBuild           : {0}", isLocal);
    Information("Branch                 : {0}", branchName);
    Information("Configuration          : {0}", configuration);
    Information("MSBuildPath            : {0}", msBuildPath);
});

Teardown(ctx =>
{
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .ContinueOnError()
    .Does(() =>
{
    var directoriesToDelete = GetDirectories("./**/obj")
        .Concat(GetDirectories("./**/bin"))
        .Concat(GetDirectories("./**/Publish"));
    DeleteDirectories(directoriesToDelete, new DeleteDirectorySettings { Recursive = true, Force = true });
});

Task("Restore")
    .Does(() =>
{
    NuGetRestore(solution, new NuGetRestoreSettings { MSBuildPath = msBuildPath.ToString() });
});

Task("Build")
    .Does(() =>
{
    var msBuildSettings = new MSBuildSettings {
        Verbosity = verbosity
        , ToolPath = msBuildPathExe
        , Configuration = configuration
        , ArgumentCustomization = args => args.Append("/m").Append("/nr:false") // The /nr switch tells msbuild to quite once it’s done
        , BinaryLogger = new MSBuildBinaryLogSettings() { Enabled = isLocal }
    };
    MSBuild(solution, msBuildSettings
            .SetMaxCpuCount(0)
            .WithProperty("Version", isReleaseBranch ? gitVersion.MajorMinorPatch : gitVersion.NuGetVersion)
            .WithProperty("AssemblyVersion", gitVersion.AssemblySemVer)
            .WithProperty("FileVersion", gitVersion.AssemblySemFileVer)
            .WithProperty("InformationalVersion", gitVersion.InformationalVersion)
            );
});

Task("Zip")
    .Does(() =>
{
    EnsureDirectoryExists(Directory(publishDir));
    Zip("./src/SimpleMusicPlayer/bin/" + configuration, publishDir + $"/SimpleMusicPlayer-{configuration}-v{gitVersion.NuGetVersion}.zip");
});

Task("Tests")
    .ContinueOnError()
    .Does(() =>
{
    CleanDirectory(testResultsDir);

    var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
            NoRestore = true,
            Logger = "trx",
            ResultsDirectory = testResultsDir,
            Verbosity = DotNetCoreVerbosity.Normal
        };

    DotNetCoreTest("./src/SimpleMusicPlayer.Tests/SimpleMusicPlayer.Tests.csproj", settings);
});

///////////////////////////////////////////////////////////////////////////////
// TASK TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Tests");

Task("appveyor")
    .IsDependentOn("Default")
    .IsDependentOn("Zip");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);