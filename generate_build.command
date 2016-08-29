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
OUTPUT_DIR="~/Desktop/$GAME_NAME_builds"

# iOS Code Sign
PROVISIONING_PROFILE="XC Ad Hoc: com.ubisoft.hungrydragon.dev"
PROVISIONING_PROFILE_UUID="99d18f4a-2a05-4e39-a5da-370321ce140b"
SIGNING_ID="iPhone Distribution: Marie Cordon (Y3J3C97LQ8)" # NOT WORKING!!

# SMB Settings
SMB_USER="srv_acc_bcn_jenkins"
SMB_PASS="Lm0%2956jkR%23Tg"
SMB_FOLDER="BCNStudio/QA/HungryDragon/builds"

# Other internal vars
UNITY_APP="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
DATE="$(date +%Y%m%d)"
USAGE="Usage: generate_build.sh [-b branch_name] [-android true|false] [-ios true|false] [-increase_version true|false] [-tag true|false] [-output dirpath]"

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

# Store script's relative path
RELATIVE_PATH="$(dirname $0)"
cd $RELATIVE_PATH

# Go to script's (project's) path
SCRIPT_PATH="$(pwd)"
echo
echo "BUILDER: Running script at path ${SCRIPT_PATH}"
cd "${SCRIPT_PATH}"

# Update git
# Revert changes to modified files.
#git reset --hard

# Remove untracked files and directories.
git clean -fd

# Change branch
git fetch
git checkout $BRANCH

# Update branch
git pull origin $BRANCH

# Update calety
cd Calety
git pull
cd "${SCRIPT_PATH}"

# Increase internal version number
if $INCREASE_VERSION_NUMBER; then
    echo
    echo "BUILDER: Increasing internal version number"
    "${UNITY_APP}" -batchmode -executeMethod Builder.IncreaseMinorVersionNumber -projectPath {$SCRIPT_PATH} -quit -buildTarget ios
fi

# Increase build unique code
echo
echo "BUILDER: Increasing Build Code"
"${UNITY_APP}" -batchmode -executeMethod Builder.IncreaseVersionCodes -projectPath $SCRIPT_PATH -quit -buildTarget ios

# Read internal version number
# Unity creates a tmp file outputVersion.txt with the version number in it. Read from it and remove it.
echo
echo "BUILDER: Reading internal version number"
"${UNITY_APP}" -batchmode -executeMethod Builder.OutputVersion -projectPath $SCRIPT_PATH -quit -buildTarget ios
VERSION_ID="$(cat outputVersion.txt)"
rm "outputVersion.txt"

# Make sure output dir is exists
mkdir -p "${OUTPUT_DIR}"    # -p to create all parent hierarchy if needed

# Generate Android build
if $BUILD_ANDROID; then
    echo
    echo "BUILDER: Generating APKs"

    # Make sure output dir exists
    mkdir "${OUTPUT_DIR}/apks/"
    
    # Do it!
    "${UNITY_APP}" -batchmode -executeMethod Builder.GenerateAPK -projectPath $SCRIPT_PATH -quit -buildTarget android -outputDir $OUTPUT_DIR
fi

# Generate iOS build
if $BUILD_IOS; then
    # Generate XCode project
    echo
    echo "BUILDER: Generating XCode Project"
    "${UNITY_APP}" -batchmode -executeMethod Builder.GenerateXcode -projectPath $SCRIPT_PATH -quit -buildTarget ios -outputDir $OUTPUT_DIR

    # Make sure output dirs exist
    mkdir "${OUTPUT_DIR}/archives/"
    mkdir "${OUTPUT_DIR}/ipas/"

    # Stage target files
    # BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "$SCRIPT_PATH/xcode/Info.plist")
    ARCHIVE_FILE="${GAME_NAME}_${VERSION_ID}.xcarchive"
    STAGE_IPA_FILE="${GAME_NAME}_${VERSION_ID}_${DATE}.ipa"
    PROJECT_NAME="${OUTPUT_DIR}/xcode/Unity-iPhone.xcodeproj"

    # Generate Archive
    echo
    echo "BUILDER: Archiving"
    xcodebuild clean -project $PROJECT_NAME -configuration Release -alltargets 
    xcodebuild archive -project $PROJECT_NAME -configuration Release -scheme "Unity-iPhone" -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" PROVISIONING_PROFILE="${PROVISIONING_PROFILE_UUID}"

    # Generate IPA file
    echo
    echo "BUILDER: Exporting IPA"
    rm "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}"    # just in case
    xcodebuild -exportArchive -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" -exportPath "${OUTPUT_DIR}/ipas/" -exportOptionsPlist "${OUTPUT_DIR}/xcode/Info.plist"
    mv "${OUTPUT_DIR}/ipas/Unity-iPhone.ipa" "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}"
fi

# Commit project changes
echo
echo "BUILDER: Committing changes"
git add "${SCRIPT_PATH}/Assets/Resources/Singletons/GameSettings.asset"
git add "${SCRIPT_PATH}/Assets/Resources/CaletySettings.asset"
git commit -m "Automatic Build. Version ${VERSION_ID}."
git push origin ${BRANCH}

# Create Git tag
if $CREATE_TAG; then
    git tag ${VERSION_ID}
    git push origin ${VERSION_ID}
fi

# Upload to Samba server
echo
echo "BUILDER: Sending to server"

# Mount the server into a tmp folder
mkdir server
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
