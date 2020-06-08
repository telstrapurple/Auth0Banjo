# Banjo

Banjo is a CLI for executing deployment operations against an Auth0 tenant. It lets you define Auth0 resources as json files (templates) that are read and used to drive [Auth0 Management API](https://auth0.com/docs/api/management/v2) calls to create or update Auth0 resources.

Banjo is designed from the ground up to support defining and deploying Auth0 resources for different environments, for example, deploying similar sets of resources to support DEV, TEST, and STAGING environments in one Auth0 tenant and PROD in another tenant.

Banjo is built as a [DotNet Core Global Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) meaning that we can install/run it on any platform. It is ideally suited to being used in CI/CD pipelines.

| Status |   |
| ------ | - |
| Release (master) | ![Release (nuget)](https://github.com/telstrapurple/Auth0Banjo/workflows/Release%20(nuget)/badge.svg) |
| Continuous Deployment (rc) | ![Continuous deployment (rc)](https://github.com/telstrapurple/Auth0Banjo/workflows/Continuous%20deployment%20(rc)/badge.svg) |
| Build | ![Build branch](https://github.com/telstrapurple/Auth0Banjo/workflows/Build%20branch/badge.svg) |

## Getting Banjo

Banjo is published as a dotnet global tool on nuget.org, [Banjo.CLI](https://www.nuget.org/packages/Banjo.CLI/).

To install:

```
> dotnet tool install -g banjo.cli
```

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
* build the new one (using GitVersion to generate a semantic version number),
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

## Using Banjo
### Prerequisites
* You've installed Banjo as a dotnet global tool
* You have an Auth0 tenant available
* You've gone through the steps to authorise an application to access the Auth0 Management API
  * Instructions can be found at https://auth0.com/docs/api/management/v2/tokens
  * At the end you'll have your Auth0 domain (eg, `my-domain.au.auth0.com`), a client id, and client secret
* Set the Auth0 domain, client id, and client secret so Banjo can access them

There are two ways you can provide Banjo with your Auth0 domain, client id and client secret. The preferred way, and the best way for CI/CD pipelines is to use environment variables:
* `AUTH0__DOMAIN={domain}` - double underscore!
* `AUTH0__CLIENTID={client id}` - double underscore!
* `AUTH0__CLIENTSECRET={client secret}` - double underscore!

Note the double-underscore, that's important as Banjo uses the <span>ASP.NET</span> style of hierarchical configuration, see [the ASP.NET docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#environment-variables)

An alternative method that's best for local dev is to set the dotnet environment to "Development" and use [dotnet user-secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows#secret-manager).
* Tell dotnet that its [environment](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-3.1) is set to "Development", which is what tells Banjo to enable the user-secrets storage.
  * set env var `DOTNET_ENVIRONMENT=Development`
* (once off) `cd {banjo clone location}/Banjo.CLI`, then `dotnet user-secrets init`.
  * Alternatively, create an empty json document `{}` at `%APPDATA%\Microsoft\UserSecrets\Banjo.CLI\secrets.json` (Windows) or `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json` (*nix/MacOS)
* `dotnet user-secrets set Auth0:Domain {domain}`
* `dotnet user-secrets set Auth0:ClientId {client_id}`
* `dotnet user-secrets set Auth0:ClientSecret {client_secret}`

### Basic Usage

Once you've installed Banjo as a global tool and configured your Auth0 domain, client id, and client secret;
```
$> banjo process --help
Usage: banjo process [options]

Options:
  --help                                 Show help information
  -t|--template <TEMPLATES_PATH>         The path to a directory of input templates
  -o|--override <OVERRIDE_PATH>          The path to an override file
  -out|--output <PROCESSED_OUTPUT_PATH>  The output path for writing the effective templates
  -d|--dry-run                           Process the templates, plan the mutation operations to make (which may include some Auth0 API calls), but do not create/update any Auth0 resources.
  -v|--verbose                           Enable Verbose level output

$> banjo process -t {path-to-templates-directory} -o {path-to-override-file}
```

### Concepts
Banjo has two key file types;
* Template - Banjo works from a set of _template_ files, each one representing an Auth0 resource you want to be deployed. The structure of the json file depends on Auth0 resource type.
* Overrides - Banjo supports an optional _overrides_ file that can alter the contents of templates before making the API calls to create/update the Auth0 resources defined by the templates.

The template models for each resource type closely follows the output from the [Auth0 Deploy CLI tool](https://auth0.com/docs/extensions/deploy-cli), which in turn generally mirrors the Auth0 Management API model.

#### Templates
A Banjo template is a declarative file that defines what the Auth0 resource should look like, similar to an Azure ARM template or AWS CloudFormation template, except that Banjo requires one resource per file.

You point Banjo at a directory containing templates (`-t|--template {path-to-templates}`), an optional overrides file (`-o|--override {path-to-overrides-file}`), and other optional arguments. Banjo will process the templates, apply changes based on the overrides file, discover the current state of the Auth0 resources, and plan the create (POST) or update (PATCH) operations it needs to make.

Banjo currently supports the following Auth0 resources:

| Resource/API name | Alternative name                                                       | Banjo template directory      | Overrides element name |
|-------------------|------------------------------------------------------------------------|-------------------------------|------------------------|
| Clients           | Applications                                                           | {templates}/clients           | clients                |
| Resource Servers  | APIs                                                                   | {templates}/resource-servers  | resource-servers       |
| Client Grants     | n/a - Auth0 shows them as on/off sliders on the Applications->APIs tab | {templates}/grants            | grants                 |
| Connections       | Connections                                                            | {templates}/connections       | connections            |
| Pages*            | n/a - Auth0 shows them in different places depending on the page type  | {templates}/pages             | pages                  |
| Roles             | Roles                                                                  | {template}/roles              | roles                  |
| Rules             | Rules                                                                  | {template}/rules              | rules                  |
| Tenant Settings   | Settings                                                               | {templates}/template-settings | template-settings      |

* Banjo only supports the following pages: Login, Password Reset, and Multifactor. It does not (yet!) support a custom Error page.

You can find more resource-type-specific details below.

#### Overrides File
An overrides file is a json document that defines changes that Banjo should make to the templates before figuring out the API calls to make. A typical usage would be to have the same set of templates but use different overrides files per environment to change some aspect of the templated resources so it's unique for the environment, such as including the environment name in a resource name or a clients callback uri's.

The overrides file allows you to define several different ways you can override some aspect of a template;
* String token replacements - You can define simple replacement tokens, so that any occurrence of of a token gets replaced with a replacement value.
* JsonPath expressions - For more complex substitutions you can also specify a jsonpath expression to select a token in the template, and an entire json entity to replace it with

This is an example of an overrides file that shows the two ways you can define string token replacements, and a jsonpath replacement in a specific client template.
```json
{
  "replacements": [
    {
      "token": "EnvironmentName", //will allow you to use replacement tokens %%EnvironmentName%%, %%ENVIRONMENTNAME%%, or %%environmentname%%
      "value": "DEV" //literal replacement value
    },
    {
      "token": "BuildVersion", //%%BuildVersion%%, %%BUILDVERSION%%, or %%buildversion%%
      "environment-variable": "AZDO_VERSION" //use the value of the AZDO_VERSION environment variable as the replacement value
    }
  ],
  "clients": [
    {
      "name": "my-awesome-application.template.json", //applies to a client template with this filename
      "overrides": [
        {
          //Replace the allowed_origins property value with the "replacement" array.
          //The "replacement" value could be anything, an array, number, string, boolean, object, etc.
          "jsonpath": "allowed_origins",
          "replacement": [ "https://origin1.example.com", "https://my-awesome-client.azurewebsites.net" ]
        }
      ]
    }
  ],
   "resource-servers": [],
   "pages": [],
   "connections": [],
   //...and any other supported resource type
}
```

In the example overrides file above;
* anywhere in a template that the replacement tokens `%%EnvironmentName%%`, `%%ENVIRONMENTNAME%%`, or `%%environmentname%%` appear, they will be replaced with the value `DEV`. For example, `"My Awesome Application - %%ENVIRONMENTNAME%%"` would become `"My Awesome Application - DEV"`
* similarly, `%%BuildVersion%%`, `%%BUILDVERSION%%`, or `%%buildversion%%` would be replaced with the value of the `AZDO_VERSION` environment variable
* In a client template file at `{templates}/clients/my-awesome-application.template.json`, the value of the `allowed_origins` property would be completely replaced with `"replacement"` array

Note: JsonPath expressions can get very complex. The `"jsonpath"` property in the override for a specific template is generally only intended to select a single value and replace it with the `"replacement"` value. In general, you should stick to simple '.' notation path traversal paths, like `jwt_configuration.alg` or `options.mfa.active`, but if you wanted to select, say, and specific element in an array of `allowed_origins`, you could. The dry-run `-d|--dry-run` and output `-out|--output` arguments are your friend 😃

### Authoring Templates
As noted above, the template models for each resource type closely follows the output from the [Auth0 Deploy CLI tool](https://auth0.com/docs/extensions/deploy-cli), which in turn generally (but not always) mirrors the Auth0 Management API model.

A few things to note:
* Template `json` files **MUST** have the extension `.template.json`. Any file that does not end with `.template.json` will be ignored.
* Use the dry-run `-d|--dry-run` argument to not perform any create/update operations. Banjo will still make GET operations to query the current state in order to plan the operations, will not make any POST/PUT requests.
* Use the output `-out|--output {output-directory}` argument to have Banjo write the 'effective' templates to disk after applying overrides and string replacements.
* jsonpath-based overrides applied to the templates _before_ token replacements, so you can use replacement tokens _in the template-specific overrides_ if you want to.

#### Downloading existing resources as templates
The easiest way to get a template as a starting point for further customising with overrides replacements is to create the resource in the Auth0 management dashboard and then export the resources.

* Install and configure the Auth0 Deploy CLI tool, https://auth0.com/docs/extensions/deploy-cli
* Run the Auth0 Deploy CLI tool `export` command with `--format directory` to export the resources as a directory tree containing 1 file per resource

The exported .json files form the basis of your template files.

```
$> a0deploy export --config_file {path-to-a0-secrets-file} --format directory --output_folder {output-folder}
```

#### Clients
| Resource/API name | Alternative name | Banjo template directory      | Overrides element name | API Docs |
|-------------------|------------------|-------------------------------|------------------------|----------|
| Clients           | Applications     | {templates}/clients           | clients                |[GET Clients](https://auth0.com/docs/api/management/v2#!/Clients/get_clients)|

_Banjo vs a0deploy_  
No known differences. An a0deploy export will work as input to Banjo.

_Banjo vs Management API_  
No known differences. However the API response and a0deploy export includes a `jwt_config` property that's not listed in the API model documentation.

#### Resource Servers
| Resource/API name | Alternative name | Banjo template directory      | Overrides element name | API Docs |
|-------------------|------------------|-------------------------------|------------------------|----------|
| Resource Servers  | APIs             | {templates}/resource-servers  | resource-servers       | [GET Resource Servers](https://auth0.com/docs/api/management/v2#!/Resource_Servers/get_resource_servers)|

_Banjo vs a0deploy_  
No known differences. An a0deploy export will work as input to Banjo.

_Banjo vs Management API_  
No known differences.

#### Client Grants
| Resource/API name | Alternative name                                                       | Banjo template directory      | Overrides element name | API Docs|
|-------------------|------------------------------------------------------------------------|-------------------------------|------------------------|---------|
| Client Grants     | n/a - Auth0 shows them as on/off sliders on the Applications->APIs tab | {templates}/grants            | grants                 | [GET Client Grants](https://auth0.com/docs/api/management/v2#!/Client_Grants/get_client_grants)|

_Banjo vs a0deploy_  
No known differences. An a0deploy export will work as input to Banjo.

_Banjo vs Management API_  
No known structural differences.

The meaning of the `client_id` property is different.
* In Management API, `client_id` property is the id of the client.
* In Banjo and a0deploy, `client_id` is the client _name_, not its id.
  * At runtime, Banjo will use the result of [GET Clients](https://auth0.com/docs/api/management/v2#!/Clients/get_clients) to look up the id of the matching client and replace the `client_id` value in the template with the _actual_ id of the matching client.

#### Connections
| Resource/API name | Alternative name                                                       | Banjo template directory      | Overrides element name |API Docs|
|-------------------|------------------------------------------------------------------------|-------------------------------|------------------------|--------|
| Connections       | Connections                                                            | {templates}/connections       | connections            |[GET Connections](https://auth0.com/docs/api/management/v2#!/Connections/get_connections)|

_Banjo vs a0deploy_  
Mostly the same, however the treatment of `enabled_clients` is different.

a0deploy defines an array of `enabled_clients` containing the names of all the clients that the connection should be enabled for. Banjo does not use `enabled_clients`.

Instead of `enabled_clients`, Banjo uses `enabled_clients_match_conditions`, which is an array of regex expressions that client names are matched to. This allows clients that are not known at template-authoring-time to still match.

For example, suppose a connection represents an Azure AD connection, and you want the connection to be enabled for all client resources for a specific service across all environments to match, you might define this;
```json
{
  //a connection template
  //snip other properties
  "enabled_clients_match_conditions": [
    "My Awesome Web App - [A-Z]+", //matches "My Awesome Web App - DEV", "My Awesome Web App - TEST", etc
    "Some Other Backend Service - %%CLIENTID%% - [A-Z]+" //only match Some Other Backend Service instances for a given %%CLIENTID%% replacement token
  ]
}
```

At runtime, Banjo will query the available clients to populate the `enabled_clients` property that the management API needs.

Note that `enabled_clients_match_conditions` are also treated for verbatim string matches as well as regex matches, so you don't need to go to special lengths to write properly escaped regexes if you're not aiming for a regex match.

_Banjo vs Management API_  
The meaning of the `enabled_clients` property is different.
* In Management API, `enabled_clients` property is the ids of the clients the connection should be enabled for.
* In a0deploy, `enabled_clients` is the client _names_, not their ids.
* In Banjo, `enabled_clients_match_conditions` is used instead, as described above.

_Notes:_  
A connection type (username-password-database, Azure AD, GCloud, Facebook, etc) is defined by its `strategy`, and every strategy has its own set of required attributes which don't appear to be documented anywhere. The best approach is set one up manually in the dashboard, then export it with `a0deploy`.

Connections are a perfect place to use environment variable token overrides, as many connection strategies will require you to specify a sensitive secret or api key or similar. For example, an Azure Active Directory enterprise connection (`"strategy": "waad"`) requires a `"client_secret`' property, which is the client secret for your AAD App Registration.

```
$> set APP_REGISTRATION_SECRET=super_sekrit_client_secret_dont_tell_anyone
```

```json
//overrides.json
{
  "replacements": [
    {
      "token": "AppRegistrationSecret", //%%AppRegistrationSecret%%, %%APPREGISTRATIONSECRET%%, or %%appregistrationsecret%%
      "environment-variable": "APP_REGISTRATION_SECRET"
    }
  ]
}
```

```json
//{templates}/connections/aad-connection.template.json
{
  //snip lots more properties
  "strategy": "waad",
  "client_id": "not_so_secret_client_id",
  "client_secret": "%%AppRegistrationSecret%%",
  //snip lots more
}
```

#### Pages
| Resource/API name | Alternative name                                                       | Banjo template directory      | Overrides element name |API Docs|
|-------------------|------------------------------------------------------------------------|-------------------------------|------------------------|--------|
| Pages*            | n/a - Auth0 shows them in different places depending on the page type  | {templates}/pages             | pages                  |n/a

_Banjo vs a0deploy_  
No known differences. An a0deploy export will work as input to Banjo.

_Banjo vs Management API_  
Pages are handled _very_ differently in Banjo and a0deploy compared to the Management API. a0deploy (and by extension, Banjo) defines a unified model of the pages, while the management api puts the pages in different parts of the API depending on the page type.

In Banjo and a0deploy, pages are defined as a json file + html file. For example;
```
{templates}/pages
                 /loginpage.template.json
                 /loginpage.html
```

```json
//{pagename}.template.json
{
  "name": "login", //"login", "password_reset", or "guardian_multifactor". Any other value will error
  "enabled": true,
  "html": "./loginpage.html"
}
```

The filenames don't matter, as long as the html filename is the same as the value of the `"html"` property in the `{pagename}.template.json` file.

Banjo supports the following file types (the `"name"` property):
* `login` - Management Dashboard -> Universal Login -> Login. In the API, it's the `custom_login_page` property on the global client.
* `password_reset` - Management Dashboard -> Universal Login -> Password Reset. In the API, it's the `change_password` property on the Tenant Settings (which is not documented on the API docs).
* `guardian_multifactor` - Management Dashboard -> Universal Login -> Multifactor. In the API, it's the `guardian_mfa_page` property on the Tenant Settings (which is not documented on the API docs).

You can only have one of each page type, so don't, for instance, define 2 pairs of `login` page template files. (Banjo will let you do it, but the second one Banjo processes will win)

#### Roles
| Resource/API name | Alternative name                                                       | Banjo template directory      | Overrides element name |API Docs|
|-------------------|------------------------------------------------------------------------|-------------------------------|------------------------|--------|
| Roles             | Roles                                                                  | {template}/roles              | roles                  |[GET Roles](https://auth0.com/docs/api/management/v2#!/Roles/get_roles)

_Banjo vs a0deploy_  
No known differences. An a0deploy export will work as input to Banjo.

_Banjo vs Management API_  
No known differences.

#### Rules
| Resource/API name | Alternative name                                                       | Banjo template directory      | Overrides element name |API Docs|
|-------------------|------------------------------------------------------------------------|-------------------------------|------------------------|--------|
| Rules             | Rules                                                                  | {template}/rules              | rules                  |[GET Rules](https://auth0.com/docs/api/management/v2#!/Rules/get_rules)|

_Banjo vs a0deploy_  
No known differences. An a0deploy export will work as input to Banjo.

_Banjo vs Management API_  
Although the Management API defines a single Rules resource type (see [GET Rules](https://auth0.com/docs/api/management/v2#!/Rules/get_rules)), it includes the script content in the HTTP request/response bodies.

Similar to Pages, Rules consist of a json template file and a `.js` script file.

```
{templates}/rules
                 /my-rule.template.json
                 /my-rule.js
```

```json
//{pagename}.template.json
{
  "enabled": true,
  "script": "./my-rule.js",
  "name": "My Empty Rule %%NAME%%",
  "order": 2,
  "stage": "login_success"
}
```

The filenames don't matter, as long as the js filename is the same as the value of the `"script"` property in the `{rule}.template.json` file.

#### Tenant Settings
| Resource/API name | Alternative name                                                       | Banjo template directory      | Overrides element name |API Docs|
|-------------------|------------------------------------------------------------------------|-------------------------------|------------------------|--------|
| Tenant Settings   | Settings                                                               | {templates}/template-settings | template-settings      |[GET Tenant Settings](https://auth0.com/docs/api/management/v2#!/Tenants/get_settings)|

_Banjo vs a0deploy_  
No known differences. An a0deploy export will work as input to Banjo.

_Banjo vs Management API_  
No known differences.

You can only have one Tenant Settings template file, as there is only one tenant settings entity per tenant.


## Release process

The **Continuous deployment (rc)** worfkflow is configured for continuous deployment. All pushes/merges to `master` automatically upload a release candidate (with an -rcNNNN suffix) that is available in the Github package feed for this project. These should not be considered stable.

The **Release (nuget)** workflow is triggered when a Release is created. The `master` branch is configured for continuous delivery with release candidatess (as above) and a manual choice when to release. When a Release is tagged and published, the workflow is run and will automatically uploaded the new package to Nuget.


## License

Copyright (C) 2020 Telstra Purple

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
