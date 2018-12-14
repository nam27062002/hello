#!/bin/bash
# This script generates Android (apk) and iOS (ipa) builds with a given setup
# Test

# Bash setup
set -e  # Exit on error - see http://www.davidpashley.com/articles/writing-robust-shell-scripts/#id2382181

INITIAL_PATH="$(pwd)"

# Store script's (project's) absolute path
# To get an absolute path, dir to it and then use pwd command
cd "$(dirname "$0")"
SCRIPT_PATH="$(pwd)"
echo
echo "BUILDER: Running script from path ${SCRIPT_PATH}"

BRANCH="release/1.19"

# Production
./generate_build.command -code "hd" -b "${BRANCH}" -reset_git -commit -tag -android -obb -upload -env "production" -smbOutput "BCNStudio/QA/HungryDragon/builds" #-increase_version
# Stage QC
./generate_build.command -code "hd" -b "${BRANCH}" -reset_git -commit -tag -android -upload -env "stage_qc" -smbOutput "BCNStudio/QA/HungryDragon/builds" #-increase_version
# Test
# ./generate_build.command -code "hd" -b "${BRANCH}" -android
cd "${INITIAL_PATH}"
exit 0
