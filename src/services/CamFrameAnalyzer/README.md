# CamFrameAnalyzer

As the project uses a custom feed to consume tailored and none-public packages, you can find a [nuget.config](nuget.config) file adding Azure DevOps Artifacts as source.

You can use the following command to add custom package if you are using VS Code:

```shell

dotnet add package CoreLib -s https://ORGANIZATION.pkgs.visualstudio.com/PROJECT/_packaging/Mo.Packages/nuget/v3/index.json

```

In Visual Studio, the experience is a bit easier. Just go to the settings and add a new custom NuGet source which will then allow you to use the normal **Manage NuGet Packages** project right click action super simple. Just change the search in option from the drop down list in the top right.

