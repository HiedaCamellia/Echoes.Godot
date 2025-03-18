# To collect code coverage, you will need the following environment setup:
#
# - A "GODOT" environment variable pointing to the Godot executable
# - ReportGenerator installed
#
#     dotnet tool install -g dotnet-reportgenerator-globaltool
#
# - A version of coverlet > 3.2.0.
#
#   As of Jan 2023, this is not yet released.
#
#   The included `nuget.config` file will allow you to install a nightly
#   version of coverlet from the coverlet nightly nuget feed.
#
#     dotnet tool install --global coverlet.console --prerelease.
#
#   You can build coverlet yourself, but you will need to edit the path to
#   coverlet below to point to your local build of the coverlet dll.
#
# If you need help with coverage, feel free to join the Chickensoft Discord.
# https://chickensoft.games

dotnet build --no-restore

coverlet `
  "./.godot/mono/temp/bin/Debug" --verbosity detailed `
  --target $env:GODOT `
  --targetargs "--run-tests --coverage --quit-on-finish" `
  --format "opencover" `
  --output "./coverage/coverage.xml" `
  --exclude-by-file "**/test/**/*.cs" `
  --exclude-by-file "**/*Microsoft.NET.Test.Sdk.Program.cs" `
  --exclude-by-file "**/Godot.SourceGenerators/**/*.cs" `
  --exclude-by-file "**/Tommy.cs" `
  --exclude-assemblies-without-sources "missingall"

# Projects included via <ProjectReference> will be collected in code coverage.
# If you want to exclude them, replace the string below with the names of
# the assemblies to ignore. e.g.,
# ASSEMBLIES_TO_REMOVE="-AssemblyToRemove1;-AssemblyToRemove2"
$ASSEMBLIES_TO_REMOVE = "-Echoes.Godot.Tests"

reportgenerator `
  -reports:"./coverage/coverage.xml" `
  -targetdir:"./coverage/report" `
  "-assemblyfilters:$ASSEMBLIES_TO_REMOVE" `
  "-classfilters:-GodotPlugins.Game.Main" `
  -reporttypes:"Html;Badges"

# Copy badges into their own folder. The badges folder should be included in
# source control so that the README.md in the root can reference the badges.

New-Item -ItemType Directory -Path ./badges -Force

# Use -Force to overwrite existing files
Move-Item -Path ./coverage/report/badge_branchcoverage.svg -Destination ./badges/branch_coverage.svg -Force
Move-Item -Path ./coverage/report/badge_linecoverage.svg -Destination ./badges/line_coverage.svg -Force


# Determine OS, open coverage accordingly.

switch -Wildcard ($env:OS)
{
    "Windows_NT" {
        Write-Output 'MS Windows'
        Start-Process "coverage/report/index.htm"
    }
    "Darwin" {
        Write-Output 'Mac OS X'
        Start-Process "coverage/report/index.htm"
    }
    "Linux" {
        Write-Output 'Linux'
        Start-Process "coverage/report/index.htm"
    }
    default {
        Write-Output 'Other OS'
    }
}
