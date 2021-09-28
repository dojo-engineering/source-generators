#!/bin/bash
echo "Fetch git tags"
$(git fetch -t --depth=5000)

echo "Getting git tag version"
GIT_TAG=$(git describe --tags --always --long --match 'v?.?')

echo "git tag is:" $GIT_TAG

MINOR_VERSION=$(echo $GIT_TAG | grep -P '(?<=v\d.\d-)\d+(?=-[a-z]+[0-9]+)' -o)

echo "minor version is:" $MINOR_VERSION

MAJOR_VERSION=$(echo $GIT_TAG | grep -P '(?<=v)(\d.\d)' -o)

echo "major version is: " $MAJOR_VERSION

FULL_VERSION="$MAJOR_VERSION.$MINOR_VERSION"

echo "full version is: " $FULL_VERSION

SOURCE_FILE="nuget-version"
echo "Writing version to $SOURCE_FILE file"
echo $FULL_VERSION > $SOURCE_FILE

echo "Check the version saved in file is:" $(cat $SOURCE_FILE)