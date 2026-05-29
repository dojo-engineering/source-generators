# Dojo.OpenApiGenerator

A Roslyn source generator that reads OpenAPI 3.0.x specification files at compile time and generates abstract ASP.NET Core controller base classes, model POCOs, and API versioning infrastructure. The OpenAPI schema is the single source of truth — the generated code is always in sync with the spec.

## Installation

```cmd
dotnet add package Dojo.OpenApiGenerator
```

Configure the package reference in your `.csproj` so it acts as an analyzer only and is not included as a runtime dependency:

```xml
<PackageReference Include="Dojo.OpenApiGenerator" Version="x.x.x">
  <IncludeAssets>all</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

## Usage

### 1. Add OpenAPI schema files

Place your OpenAPI 3.0.x spec files (`.json`, `.yaml`, or `.yml`) inside an `OpenApiSchemas/` folder at the root of the project that references the generator:

```
MyWebApi/
  OpenApiSchemas/
    my-api.yaml
    another-api.json
```

The generator automatically discovers all schema files in that folder during build.

### 2. (Optional) Configure the generator

Add an `autoApiGeneratorSettings.json` file to the project root (see [Configuration](#configuration) below).

### 3. Implement the generated abstract controller

For each schema file the generator produces an abstract controller base class named `{Title}ControllerBase`. Inherit from it and implement the abstract action methods:

```csharp
// Generated (do not edit):
// public abstract class HelloWorldControllerBase : ControllerBase
// {
//     protected abstract Task<ActionResult<HelloFromSource>> HelloFromSourceAsync(
//         long number, string message, CancellationToken cancellationToken);
// }

[ApiController]
public class HelloWorldController : HelloWorldControllerBase
{
    protected override async Task<ActionResult<HelloFromSource>> HelloFromSourceAsync(
        long number, string message, CancellationToken cancellationToken)
    {
        return Ok(new HelloFromSource { /* ... */ });
    }
}
```

### 4. Register generated infrastructure

Call the generated `AddApiConfigurator` extension method in your `Startup.cs` / `Program.cs` to register API versioning and (optionally) the API Explorer for Swagger:

```csharp
services.AddApiConfigurator();
```

## Generated Artifacts

| Artifact                       | Description                                                                                                                                          |
| ------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `{Title}ControllerBase`        | Abstract controller in `{Namespace}.Generated.Controllers[.V{Version}]`. One per schema file (or per tag if `OrganizeControllersByTags` is enabled). |
| Model POCOs                    | Strongly-typed request/response classes in the project namespace, with data-annotation validation attributes derived from the schema constraints.    |
| `ApiConstants`                 | String constants for every API version discovered across all schemas.                                                                                |
| `ApiVersions`                  | Typed class exposing the same version values.                                                                                                        |
| `ApiConfigurator`              | `IServiceCollection` extension that wires up API versioning and the API Explorer.                                                                    |
| `InheritedApiVersionAttribute` | Custom attribute used internally to propagate version mappings to the generated controllers.                                                         |

### Model validation attributes

The generator maps OpenAPI schema constraints to `System.ComponentModel.DataAnnotations` attributes automatically:

| OpenAPI constraint | Generated attribute |
| ------------------ | ------------------- |
| `required: true`   | `[Required]`        |
| `minLength`        | `[MinLength(n)]`    |
| `maxLength`        | `[MaxLength(n)]`    |
| `format: email`    | `[EmailAddress]`    |

## Configuration

Create `autoApiGeneratorSettings.json` in the project root (next to the `.csproj` file). All fields are optional.

```json
{
  "VersionParameterName": "version",
  "IncludeVersionParameterInActionSignature": false,
  "OpenApiSupportedVersionsExtension": "x-supported-api-versions",
  "DateTimeVersionFormat": "yyyy-MM-dd",
  "ApiAuthorizationPoliciesExtension": "x-authorization-policies",
  "GenerateApiExplorer": true,
  "OrganizeControllersByTags": false
}
```

| Setting                                    | Type     | Default | Description                                                                                                                                  |
| ------------------------------------------ | -------- | ------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `VersionParameterName`                     | `string` | `null`  | Name of the route/query parameter that carries the API version.                                                                              |
| `IncludeVersionParameterInActionSignature` | `bool`   | `false` | When `true`, the version parameter is added to every generated action method signature.                                                      |
| `OpenApiSupportedVersionsExtension`        | `string` | `null`  | Name of the OpenAPI extension used to declare which versions support a given path (see [OpenAPI Extensions](#openapi-extensions)).           |
| `DateTimeVersionFormat`                    | `string` | `null`  | .NET date format string (e.g. `"yyyy-MM-dd"`) used to parse and format date-based API versions.                                              |
| `ApiAuthorizationPoliciesExtension`        | `string` | `null`  | Name of the OpenAPI extension used to declare authorization policies on a path or operation (see [OpenAPI Extensions](#openapi-extensions)). |
| `GenerateApiExplorer`                      | `bool`   | `true`  | When `false`, the API Explorer (Swagger/OpenAPI UI support) is not registered by `ApiConfigurator`.                                          |
| `OrganizeControllersByTags`                | `bool`   | `false` | When `true`, a separate controller base class is generated for each operation tag instead of one per schema file.                            |

## OpenAPI Extensions

The generator supports two custom OpenAPI extensions that can be applied at the path or operation level.

### `x-supported-api-versions`

Declares the list of API versions that expose a given path. The value must be an array of strings.

```yaml
paths:
  /orders/{id}:
    get:
      x-supported-api-versions:
        - "1.0"
        - "2022-04-07"
```

Set the extension name via `OpenApiSupportedVersionsExtension` in `autoApiGeneratorSettings.json`.

### `x-authorization-policies`

Applies one or more ASP.NET Core authorization policies to the generated controller or action as `[Authorize(Policy = "...")]` attributes.

```yaml
paths:
  /orders/{id}:
    get:
      x-authorization-policies:
        - read
        - orders.admin
```

Set the extension name via `ApiAuthorizationPoliciesExtension` in `autoApiGeneratorSettings.json`.

## Shared Models

To define model types that are reused across multiple schema files, create a dedicated schema file with `info.title` set to `shared`:

```yaml
openapi: 3.0.1
info:
  title: shared
  version: ""
components:
  schemas:
    Money:
      type: object
      properties:
        amount:
          type: number
        currency:
          type: string
```

The generator processes this file first and makes the resulting model classes available to all other schemas in the same project.

## Benefits

- **Contract-first**: the OpenAPI schema drives the code — no drift between documentation and implementation.
- **No controller boilerplate**: route attributes, action signatures, response type attributes, versioning, and authorization are all generated from the spec.
- **Always in sync**: adding or changing a path in the schema immediately updates the generated abstract class, surfacing breaking changes as compile errors.
- **Built-in API versioning**: integrates with `Microsoft.AspNetCore.Mvc.Versioning` via generated attributes and the `ApiConfigurator` extension.
- **Validation from schema constraints**: `minLength`, `maxLength`, `required`, and `email` constraints become data-annotation attributes on model properties with no extra code.
- **Tag-based organization**: optionally split a large API into focused controllers, one per operation tag.

## Limitations

- **OpenAPI 3.0.x only**: OpenAPI 2.0 (Swagger) and OpenAPI 3.1 formats are not supported.
- **Abstract controllers require implementation**: the generator produces abstract base classes — you must create a concrete subclass to provide the business logic.
- **Schema files must be present at build time**: the generator reads spec files from the filesystem during compilation. Missing files cause the generator to silently skip generation for that project.
- **No external `$ref` resolution**: references to types defined in separate files outside the `OpenApiSchemas/` folder are not resolved.
- **Visual Studio 2019 syntax highlighting**: generated code may cause Roslyn-related highlighting issues in VS 2019. See the [workaround](http://stevetalkscode.co.uk/debug-source-generators-with-vs2019-1610).
