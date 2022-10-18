FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
ARG VERSION_SUFFIX
ARG NEXUS_URL
ARG NEXUS_KEY

RUN echo "Publishing $VERSION_SUFFIX to $NEXUS_URL"

WORKDIR /src
ADD . .
RUN dotnet restore /src/Dojo.Generators.sln --configfile /src/NuGet.config
RUN dotnet build --no-restore /src/Dojo.Generators.sln -c Release
RUN dotnet test --no-build /src/Dojo.Generators.Tests/Dojo.Generators.Tests.csproj -c Release

RUN chmod +x /src/build/publish.sh
RUN /src/build/publish.sh $VERSION_SUFFIX $NEXUS_URL $NEXUS_KEY