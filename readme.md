# Banjo
Banjo is a CLI for executing deployment operations against an Auth0 tenant.

## Build and install
If you want to install the Banjo CLI as a global tool that's built from your own local sources, there's a bit of command line-fu needed.
(see [this blog post](https://www.meziantou.net/2018/06/11/how-to-publish-a-dotnet-global-tool-with-dotnet-core-2-1))

Individual commands are below, but there's an all-in-one PowerShell script that builds, packages, and installs (or updates) the new build;
```powershell
# from the Banjo.CLI project directory

.\reinstall-cli.ps1
```

You should now be able to run the tool from anywhere with
```
banjo {arguments}
```

List the current installed global tools. You're looking for `banjo.cli`
```
dotnet tool list -g
```

### The scripted steps
The manual build process involve;
* uninstall the tool if it exists,
* build the new one,
* publish locally,
* then install from local

```
# from the Banjo.CLI project directory

dotnet tool uninstall -g banjo.cli
dotnet build
dotnet pack --output ./
dotnet tool install -g banjo.cli --add-source ./
```
Or to update an already installed tool from locally built source
```
dotnet tool update -g banjo.cli --add-source ./
```

## Downloading existing resources as templates
* Install and configure the Auth0 Deploy CLI tool, https://auth0.com/docs/extensions/deploy-cli
* Run the Auth0 Deploy CLI tool `export` command with `--format directory` to export the resources as a directory tree containing 1 file per resource

The exported .json files form the basis of your template files.

```powershell
a0deploy export --config_file {path-to-a0-secrets-file} --format directory --output_folder {output-folder}
```