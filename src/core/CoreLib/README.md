# CoreLib

Includes core functionality to all platform services like service bus, azure blog storage and Cosmos Db.

## Using Azure DevOps Artifacts

It is a best practice to publish your shared libraries to centralized packages repository.

[Azure DevOps Artifacts]() offers secure enterprise grade private (or public) packages repository.

In this workshop, I've published CoreLib package to public artifacts repository.

To add the custom package to your code using Visual Studio, you can follow the online documentation.

In Visual Studio Code, you can use the following ```dotnet``` command:

```shell

dotnet add package CoreLib -s https://gbb-appinnovation.pkgs.visualstudio.com/IntelligentExperiences.OnContainers/_packaging/Mo.Packages/nuget/v3/index.json


```