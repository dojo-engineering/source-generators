FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder
ARG VERSION_SUFFIX
ARG NEXUS_URL
ARG NEXUS_KEY

RUN echo "Publishing $VERSION_SUFFIX to $NEXUS_URL"

WORKDIR /src
ADD . .
RUN dotnet restore /src/Dojo.Generators.sln --configfile /src/NuGet.config

RUN dotnet build --no-restore /src/Dojo.Generators.Core/Dojo.Generators.Core.csproj -c Release
RUN dotnet build --no-restore /src/Dojo.Generators.Abstractions/Dojo.Generators.Abstractions.csproj -c Release
RUN dotnet build --no-restore /src/Dojo.AutoGenerators/Dojo.AutoGenerators.csproj -c Release
RUN dotnet build --no-restore /src/Dojo.OpenApiGenerator/Dojo.OpenApiGenerator.csproj -c Release
#RUN dotnet build --no-restore /src/TestApp/TestApp.csproj -c Release
#RUN dotnet build --no-restore /src/Dojo.OpenApiGenerator.TestWebApi/Dojo.OpenApiGenerator.TestWebApi.csproj -c Release

RUN dotnet build --no-restore /src/Dojo.Generators.Tests/Dojo.Generators.Tests.csproj -c Release
RUN dotnet test --no-build /src/Dojo.Generators.Tests/Dojo.Generators.Tests.csproj -c Release

RUN chmod +x /src/build/publish.sh
RUN /src/build/publish.sh $VERSION_SUFFIX $NEXUS_URL $NEXUS_KEY