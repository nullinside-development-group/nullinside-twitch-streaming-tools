#!/usr/bin/env bash
# If there is already a version number don't make a new one.
export TAG_VERSION=$(git tag --points-at HEAD)
if [[ -z "$TAG_VERSION" ]]; then
    # Get the latest version for the repo
    if ! export CURRENT_VERSION=$(curl "https://api.github.com/repos/nullinside-development-group/nullinside-twitch-streaming-tools/tags" | jq -r '.[0].name')
    then
        exit 1
    fi
    
    major=1
    minor=0
    build=-1
    
    # Break down the version number into it's components
    regex="([0-9]+).([0-9]+).([0-9]+)"
    if [[ $CURRENT_VERSION =~ $regex ]]; then
      major="${BASH_REMATCH[1]}"
      minor="${BASH_REMATCH[2]}"
      build="${BASH_REMATCH[3]}"
    fi
    
    # Increment the build always
    build=$(echo $build+1 | bc)
    export TAG_VERSION=${major}.${minor}.${build}
fi

docker system prune -af
docker build --build-arg="TAG_VERSION="$TAG_VERSION --build-arg="GITHUB_TOKEN="$GITHUB_NULLINSIDE_ORG_RELEASE_TOKEN --progress=plain --no-cache .
