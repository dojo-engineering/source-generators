# Contributing to source-generators

## Development workflow

1. Create a feature branch from main.
2. Implement your changes.
3. Run local checks before opening a PR:
   - dotnet build Dojo.Generators.sln
   - dotnet test Dojo.Generators.Tests/Dojo.Generators.Tests.csproj
4. Open a pull request and wait for CI/build checks.

## Version publishing (tag-based)

This repository publishes NuGet packages from Git tags.

### How release version is decided

- The release pipeline expects a Git tag in this format: vX.Y.Z
- The script build/writeversion.sh validates the tag and writes X.Y.Z into nuget-version.
- If the tag does not match the format, publish fails.

### What triggers publishing

Publishing is driven by the Cloud Build pipeline in build/cloudbuild.yaml.

Pipeline steps:

1. Read TAG_NAME and run build/writeversion.sh.
2. Download NuGet.config.
3. Build Docker image from build/Dockerfile.
4. Inside Docker, run tests and then execute build/publish.sh.

### How packages are built and pushed

build/publish.sh performs the package release:

1. Reads version from /src/nuget-version.
2. Builds a full package version as: version + version suffix.
3. Runs:
   - dotnet build -p:Version=<resolved-version>
   - dotnet pack -p:Version=<resolved-version>
4. Pushes generated packages to Nexus NuGet hosted feed:
   - Dojo.AutoGenerators
   - Dojo.OpenApiGenerator
   - Dojo.Generators.Core
   - Dojo.Generators.Abstractions

The target feed URL is built as:

- <NEXUS_URL>/repository/nuget-hosted/

### Suffix behavior

- VERSION_SUFFIX is passed into Docker from Cloud Build.
- publish.sh appends suffix to X.Y.Z (for example X.Y.Z-preview.1).
- If suffix is -release, publish.sh converts it to empty suffix, producing a stable X.Y.Z package version.

### Required CI secrets/variables

Cloud Build requires:

- TAG_NAME (release tag)
- \_NEXUS (Nexus base URL)
- \_VERSION_SUFFIX (optional version suffix)
- NEXUS_KEY (secret used by dotnet nuget push)

## Release checklist

1. Ensure main is green (build and tests passing).
2. Merge the PR that should be released.
3. Create and push a SemVer tag:
   - git tag vX.Y.Z
   - git push origin vX.Y.Z
4. Verify Cloud Build completed successfully.
5. Confirm new package versions are available in Nexus feed.

## Notes

- nuget-version is generated/overwritten by the tag processing step in CI.
- Directory.Build.props contains a static Version value, but release publishing uses the explicit Version passed during build/pack.
