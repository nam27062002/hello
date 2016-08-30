#!/bin/Bash
# This script generates Android (apk) and iOS (ipa) builds with a given setup

# Bash setup
set -e  # Exit on error - see http://www.davidpashley.com/articles/writing-robust-shell-scripts/#id2382181

# Default parameters
BRANCH="develop"
BUILD_ANDROID=false
BUILD_IOS=true
INCREASE_VERSION_NUMBER=true
CREATE_TAG=true
GAME_NAME="hd"
OUTPUT_DIR="${HOME}/Desktop/${GAME_NAME}_builds"

# iOS Code Sign
PROVISIONING_PROFILE="XC Ad Hoc: com.ubisoft.hungrydragon.dev"  # Not used, just for reference. Make sure it's downloaded in the build machine (XCode->Preferences->Accounts->View Details)
#PROVISIONING_PROFILE_UUID="99d18f4a-2a05-4e39-a5da-370321ce140b"
PROVISIONING_PROFILE_UUID="e2a6e917-8663-4459-b97f-6ec3c7e1d3d3" # Get it by right-click on the target provisioning profile in XCode->Preferences->Accounts->View Details and choosing "Show in Finder" (the UUID is the filename of the selected profile)
SIGNING_ID="iPhone Distribution: Marie Cordon (Y3J3C97LQ8)" # NOT WORKING!!

# SMB Settings
SMB_USER="srv_acc_bcn_jenkins"
SMB_PASS="Lm0%2956jkR%23Tg"
SMB_FOLDER="BCNStudio/QA/HungryDragon/builds"

# Other internal vars
UNITY_APP="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
DATE="$(date +%Y%m%d)"
USAGE="Usage: generate_build.sh [-b branch_name] [-android true|false] [-ios true|false] [-increase_version true|false] [-tag true|false] [-output dirpath]"
START_TIME=$(date +%s)

# Parse parameters
for ((i=1;i<=$#;i++)); 
do
    PARAM_NAME=${!i}
    if [ "$PARAM_NAME" == "--help" ] ; then
        echo $USAGE
        exit 1
    elif [ "$PARAM_NAME" == "-b" ] ; then
        ((i++))
        BRANCH=${!i}
    elif [ "$PARAM_NAME" == "-android" ]; then
        ((i++))
        BUILD_ANDROID=${!i}
    elif [ "$PARAM_NAME" == "-ios" ]; then
        ((i++))
        BUILD_IOS=${!i}
    elif [ "$PARAM_NAME" == "-increase_version" ]; then
        ((i++))
        INCREASE_VERSION_NUMBER=${!i}
    elif [ "$PARAM_NAME" == "-tag" ]; then
        ((i++))
        CREATE_TAG=${!i}
    elif [ "$PARAM_NAME" == "-output" ] ; then
        ((i++))
        OUTPUT_DIR=${!i}
    else 
        echo "Unknown parameter ${PARAM_NAME}"
        echo $USAGE
        exit 1
    fi
done;

# Store script's (project's) absolute path
# To get an absolute path, dir to it and then use pwd command
cd "$(dirname $0)"
SCRIPT_PATH="$(pwd)"
echo
echo "BUILDER: Running script from path ${SCRIPT_PATH}"

# Update git
# Revert changes to modified files.
git reset --hard

# Remove untracked files and directories.
git clean -fd

# Change branch
git fetch
git checkout "${BRANCH}"

# Update branch
git pull origin "${BRANCH}"

# Update calety
cd Calety
git pull
cd "${SCRIPT_PATH}"

# Increase internal version number
if $INCREASE_VERSION_NUMBER; then
    echo
    echo "BUILDER: Increasing internal version number..."
    #set +e  # For some unknown reason, in occasions the Builder.IncreaseMinorVersionNumber causes an error, making the script to stop - Disable exitOnError for this single instruction
    "${UNITY_APP}" -batchmode -executeMethod Builder.IncreaseMinorVersionNumber -projectPath "${SCRIPT_PATH}" -quit
    #set -e
fi

# Increase build unique code
echo
echo "BUILDER: Increasing Build Code..."
"${UNITY_APP}" -batchmode -executeMethod Builder.IncreaseVersionCodes -projectPath "${SCRIPT_PATH}" -quit

# Read internal version number
# Unity creates a tmp file outputVersion.txt with the version number in it. Read from it and remove it.
echo
echo "BUILDER: Reading internal version number..."
"${UNITY_APP}" -batchmode -executeMethod Builder.OutputVersion -projectPath "${SCRIPT_PATH}" -quit
VERSION_ID="$(cat outputVersion.txt)"
echo $VERSION_ID
rm -f "outputVersion.txt"

# Make sure output dir is exists
mkdir -p "${OUTPUT_DIR}"    # -p to create all parent hierarchy if needed (and to not exit with error if folder already exists)

# Generate Android build
if $BUILD_ANDROID; then
    echo
    echo "BUILDER: Generating APKs..."

    # Make sure output dir exists
    mkdir -p "${OUTPUT_DIR}/apks/"
    
    # Do it!
    "${UNITY_APP}" -batchmode -executeMethod Builder.GenerateAPK -projectPath "${SCRIPT_PATH}" -quit -buildTarget android -outputDir "${OUTPUT_DIR}"
fi

# Generate iOS build
if $BUILD_IOS; then
    # Generate XCode project
    echo
    echo "BUILDER: Generating XCode Project..."
    "${UNITY_APP}" -batchmode -executeMethod Builder.GenerateXcode -projectPath "${SCRIPT_PATH}" -quit -buildTarget ios -outputDir "${OUTPUT_DIR}"

    # Make sure output dirs exist
    mkdir -p "${OUTPUT_DIR}/archives/"
    mkdir -p "${OUTPUT_DIR}/ipas/"

    # Stage target files
    # BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "$SCRIPT_PATH/xcode/Info.plist")
    ARCHIVE_FILE="${GAME_NAME}_${VERSION_ID}.xcarchive"
    STAGE_IPA_FILE="${GAME_NAME}_${VERSION_ID}_${DATE}.ipa"
    PROJECT_NAME="${OUTPUT_DIR}/xcode/Unity-iPhone.xcodeproj"

    # Generate Archive
    echo
    echo "BUILDER: Cleaning XCode build..."
    xcodebuild clean -project "${PROJECT_NAME}" -configuration Release -alltargets 

    echo
    echo "BUILDER: Archiving..."
    # Since the archiving process has a lot of verbose (and XCode doesn't allow us to regulate it), show only the relevant lines
    xcodebuild archive -project "${PROJECT_NAME}" -configuration Release -scheme "Unity-iPhone" -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" PROVISIONING_PROFILE="${PROVISIONING_PROFILE_UUID}" | egrep '^(/.+:[0-9+:[0-9]+:.(error|warning):|fatal|===|fail)' # From http://stackoverflow.com/questions/2244637/how-to-filter-the-xcodebuild-command-line-output
    
    # Generate IPA file
    echo
    echo "BUILDER: Exporting IPA..."
    rm -f "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}"    # just in case
    xcodebuild -exportArchive -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" -exportPath "${OUTPUT_DIR}/ipas/" -exportOptionsPlist "${OUTPUT_DIR}/xcode/Info.plist"
    mv -f "${OUTPUT_DIR}/ipas/Unity-iPhone.ipa" "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}"
fi

# Commit project changes
echo
echo "BUILDER: Committing changes"
git add "${SCRIPT_PATH}/Assets/Resources/Singletons/GameSettings.asset"
git add "${SCRIPT_PATH}/Assets/Resources/CaletySettings.asset"
git commit -m "Automatic Build. Version ${VERSION_ID}."
git push origin "${BRANCH}"

# Create Git tag
if $CREATE_TAG; then
    set +e  # Don't exit script on error
    git tag "${VERSION_ID}"
    git push origin "${VERSION_ID}"
    set -e
fi

# Upload to Samba server
echo
echo "BUILDER: Sending to server..."

# Mount the server into a tmp folder
mkdir -p server
mount -t smbfs "//${SMB_USER}:${SMB_PASS}@ubisoft.org/${SMB_FOLDER}" server

# Copy IPA
if $BUILD_IOS; then
    cp "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}" "server/"
fi

# Copy APK
if $BUILD_ANDROID; then
    cp "${OUTPUT_DIR}/*.apk" "server/"
fi

# Unmount server and remove tmp folder
umount server
rmdir server

# Done!
END_TIME=$(date "+%s")
DIFF=$(echo "$END_TIME - $START_TIME" | bc)
echo
echo "BUILDER: Done in $(date -j -u -f "%s" $DIFF "+%Hh %Mm %Ss")!" # -j to not actually set the date, -u for UTC time, -f to specify input format (%s -> seconds)
echo
