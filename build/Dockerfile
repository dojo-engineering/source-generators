FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG VERSION_SUFFIX
ARG NEXUS_URL
ARG NEXUS_KEY

RUN echo "Publishing $VERSION_SUFFIX to $NEXUS_URL"

WORKDIR /src
ADD . .
RUN dotnet restore /src/DojoGenerators.sln --configfile /src/NuGet.config
RUN dotnet build --no-restore /src/DojoGenerators.sln -c Release
RUN dotnet test --no-build /src/DojoGenerator.Tests/DojoGenerator.Tests.csproj -c Release

RUN chmod +x /src/build/publish.sh
RUN /src/build/publish.sh $VERSION_SUFFIX $NEXUS_URL $NEXUS_KEY