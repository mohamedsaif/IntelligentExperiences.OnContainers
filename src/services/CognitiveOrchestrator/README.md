# Cognitive Orchestrator

This is a regular Azure Function C# project triggered by Azure Service Bus topic.

## Quick Tips

### Creating new Azure Function in VS Code

You can leverage the Azure Functions VS Code extension to easily create new function. 

You can also use Azuer Functions Core tools to do as well through ```func new```


### Adding Docker Support

To generate a docker file on your existing Azure Function project, just run the following command (make sure you are in the Function project root directory):

```bash

func init . --docker-only

```