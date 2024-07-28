#!/bin/bash

# Set variables
DOCKERFILE_PATH="./Dockerfile"
IMAGE_NAME="bsamaha/coinbase-websocket-client"
VERSION_PATTERN="^ENV VERSION=([0-9]+\.[0-9]+\.[0-9]+)$"

# Function to increment version
increment_version() {
    local version=$1
    local update_type=$2
    local major minor patch

    IFS='.' read -r major minor patch <<< "$version"
    case $update_type in
        major)
            major=$((major + 1))
            minor=0
            patch=0
            ;;
        minor)
            minor=$((minor + 1))
            patch=0
            ;;
        patch)
            patch=$((patch + 1))
            ;;
    esac
    echo "$major.$minor.$patch"
}

# Read current version from Dockerfile
current_version=$(grep -E "$VERSION_PATTERN" "$DOCKERFILE_PATH" | sed -E "s/$VERSION_PATTERN/\1/")

if [[ -z "$current_version" ]]; then
    echo "No valid version found in Dockerfile. Setting initial version to 1.0.0"
    new_version="1.0.0"
else
    # Ask for update type
    while true; do
        read -p "Is this a major, minor, or patch update? " update_type
        case $update_type in
            major|minor|patch) break;;
            *) echo "Please enter 'major', 'minor', or 'patch'.";;
        esac
    done

    # Increment version
    new_version=$(increment_version "$current_version" "$update_type")
fi

# Remove any existing VERSION lines
sed -i '/VERSION=/d' "$DOCKERFILE_PATH"

# Add the new ENV VERSION line
echo "ENV VERSION=$new_version" >> "$DOCKERFILE_PATH"

# Build Docker image with specific version tag
docker build -t "${IMAGE_NAME}:${new_version}" .

# Tag the image as latest
docker tag "${IMAGE_NAME}:${new_version}" "${IMAGE_NAME}:latest"

# Push both version-specific and latest tags to DockerHub
docker push "${IMAGE_NAME}:${new_version}"
docker push "${IMAGE_NAME}:latest"

echo "Successfully updated version to $new_version, tagged as latest, and pushed both to DockerHub"