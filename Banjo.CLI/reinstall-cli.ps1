$ToolName = "banjo.cli"

dotnet tool restore
$GitVersion = dotnet gitversion | ConvertFrom-Json
dotnet build -p:VersionPrefix=$($GitVersion.NuGetVersion)
#dotnet pack --output ./ # GeneratePackageOnBuild is already set
$IsInstalled = (dotnet tool list -g | where { $_.StartsWith($ToolName)}).length -gt 0
if ($IsInstalled)
{
  Write-Host "Uninstalling $ToolName"
  dotnet tool uninstall -g $ToolName
} else 
{
  Write-Host "$ToolName is not installed, nothing to uninstall"
}
dotnet tool install -g $ToolName --add-source ./ --version $($GitVersion.NuGetVersion)
