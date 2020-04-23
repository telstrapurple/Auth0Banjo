$ToolName = "banjo.cli"

dotnet build
dotnet pack --output ./
$IsInstalled = (dotnet tool list -g | where { $_.StartsWith($ToolName)}).length -gt 0
if ($IsInstalled)
{
  Write-Host "Uninstalling $ToolName"
  dotnet tool uninstall -g $ToolName
} else 
{
  Write-Host "$ToolName is not installed, nothing to uninstall"
}
dotnet tool install -g $ToolName --add-source ./
