#!/bin/bash

GIT_REGEX='^v[0-9]+\.[0-9]+\.[0-9]+$'

echo "git regex: " $GIT_REGEX

if [ "$1" != "" ]; then
    echo GIT_TAG: $1
    GIT_TAG=$1
else
    echo "Git Tag missing!"
fi

if [[ $GIT_TAG =~ $GIT_REGEX ]]; then
    echo "git tag is:" $GIT_TAG

    TAG_NUMBERS=$(echo "$GIT_TAG" | sed 's/v//' )

    echo "git tag version is:" $TAG_NUMBERS

    IFS='.' read -r -a VERSIONS <<< "$TAG_NUMBERS"

    PATCH_VERSION=${VERSIONS[2]}

    echo "patch version is:" $PATCH_VERSION

    MINOR_VERSION=${VERSIONS[1]}

    echo "minor version is:" $MINOR_VERSION

    MAJOR_VERSION=${VERSIONS[0]}

    echo "major version is: "$MAJOR_VERSION

    FULL_VERSION="$MAJOR_VERSION.$MINOR_VERSION.$PATCH_VERSION"

    echo "full version is: " $FULL_VERSION

    SOURCE_FILE="nuget-version"
    echo "Writing version to $SOURCE_FILE file"
    echo $FULL_VERSION > $SOURCE_FILE

    echo "Check the version saved in file is:" $(cat $SOURCE_FILE)
else
    echo "Invalid GIT tag! Expected format 'v[0..999*].[0..999*].[0..999*]'"
    exit -1
fi