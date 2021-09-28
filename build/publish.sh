#!/bin/bash
echo "running publish.sh"

VERSION_FILE="/src/nuget-version"

if [ "$1" != "" ]; then
    echo VERSION_SUFFIX: $1
    VERSION_SUFFIX=$1
    if [ "$1" == "-release" ]; then
        VERSION_SUFFIX=''
    fi
fi

if [ "$2" != "" ]; then
    echo NEXUS: $2
    NEXUS=$2/repository/nuget-hosted/
fi

if [ -f "$VERSION_FILE" ]; then
    echo "$VERSION_FILE File exists"

    VERSION=$(cat $VERSION_FILE)
    echo "Version value to be added to NuGet is: " $VERSION
    if [ -z $VERSION ]; then
        echo "Error. No version being saved in the previous step. Confirm that you have assigned a 'v.X.Y' tag to the repository branch."
        exit 1
    else
        echo "Version $VERSION was found. Start packaging."

        NUGET_FULL_VER="$VERSION$VERSION_SUFFIX"
        echo "Targeting version: $NUGET_FULL_VER"

        dotnet pack --no-build -p:NuspecFile=package.nuspec --output nupkgs -p:Version=$NUGET_FULL_VER
        dotnet nuget push /src/nupkgs/DojoGenerator.$NUGET_FULL_VER.nupkg -k $3 -s $NEXUS

        # dotnet nuget push /src/nupkgs/DojoGenerator.Attributes.$NUGET_FULL_VER.nupkg -k $3 -s $NEXUS

    fi
else
    echo "Error. No file was created in the previous step which contains version for Nuget. The file is: " $VERSION_FILE
fi