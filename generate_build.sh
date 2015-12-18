#!/bin/Bash
# This scripts generates xcode_stage and xcode_prod projects and generates the ipa files

BRANCH="feature/builder_script"
BUILD_ANDROID=false
BUILD_IOS=true
INCREASE_VERSION_NUMBER=false
CREATE_TAG=false
GAME_NAME="Dragon"

# iOS Code Sign
PROVISIONING_PROFILE="iOS Team Provisioning Profile: *"
PROVISIONING_PROFILE_UUID="ed92bf60-ed3c-463e-aaa0-91b4d854b238"
SIGNING_ID="iPhone Distribution: Marie Cordon (Y3J3C97LQ8)" # NOT WORKING!!

# SMB Settings
SMB_USER="srv_acc_bcn_jenkins"
SMB_PASS="Lm0%2956jkR%23Tg"
SMB_FOLDER="BCNStudio/QA/HungryDragon"

USAGE="usage: generate_build.sh [-b branch_name] [-android true|false] [-ios true|false] [-increase_version true|false] [-tag true|false]"

for ((i=1;i<=$#;i++)); 
do
    PARAM_NAME=${!i}
    if [ "$PARAM_NAME" == "--help" ] ; then
        echo $USAGE
        exit 1
    elif [ "$PARAM_NAME" == "-b" ] ; then
        ((i++))
        echo ${i}
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
    else 
        echo "Unknown parameter ${PARAM_NAME}"
        echo $USAGE
        exit 1
    fi
done;


SCRIPT_PATH=$(pwd)/"$(dirname $0)"
echo $SCRIPT_PATH
cd ${SCRIPT_PATH}

# UPDATE GIT
# Revert changes to modified files.
git reset --hard
# Remove all untracked files and directories.
git clean -fdx
# Chante branch
git fetch
git checkout $BRANCH
success=$?
if [[ $success -ne 0 ]]; then
    echo "Something went wrong. Exiting"
    exit 1
fi
#update branch
git pull origin $BRANCH

if $INCREASE_VERSION_NUMBER; then
echo "Increasing version numbers"
#Increase Version Number
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -executeMethod Builder.IncreasePatchVersionNumber -projectPath $SCRIPT_PATH -quit -buildTarget ios
#Increase Android Version Code
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -executeMethod Builder.IncreaseAndroidVersionCode -projectPath $SCRIPT_PATH -quit -buildTarget ios
fi

if $BUILD_ANDROID; then
    #GENERATE APKS
    echo "Generating APKs"
    rm "${SCRIPT_PATH}/ss*.apk"    # just in case
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -executeMethod Builder.GenerateAPK -projectPath $SCRIPT_PATH -quit -buildTarget android
fi

if $BUILD_IOS; then
    #GENERATE XCODE PROJECTS
    echo "Generating xCode Projects"
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -executeMethod Builder.GenerateXcode -projectPath $SCRIPT_PATH -quit -buildTarget ios

    # GENERATE ARCHIVES AND IPAS
    mkdir "${SCRIPT_PATH}/archives/"
    mkdir "${SCRIPT_PATH}/ipas/"

    #STAGE
    echo "Archiving"
    BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "$SCRIPT_PATH/xcode/Info.plist")
    ARCHIVE_FILE="${GAME_NAME}_${BUNDLE_ID}.xcarchive"
    STAGE_IPA_FILE="${GAME_NAME}_${BUNDLE_ID}.ipa"
    PROJECT_NAME="${SCRIPT_PATH}/xcode/Unity-iPhone.xcodeproj"

    xcodebuild clean -project $PROJECT_NAME -configuration Release -alltargets 
    xcodebuild archive -project $PROJECT_NAME -configuration Release -scheme "Unity-iPhone" -archivePath "${SCRIPT_PATH}/archives/${ARCHIVE_FILE}" PROVISIONING_PROFILE="${PROVISIONING_PROFILE_UUID}"
    rm "${SCRIPT_PATH}/ipas/${STAGE_IPA_FILE}"    # just in case
    xcodebuild -exportArchive -archivePath "${SCRIPT_PATH}/archives/${ARCHIVE_FILE}" -exportPath "${SCRIPT_PATH}/ipas/" -exportOptionsPlist "${SCRIPT_PATH}/xcode/Info.plist"
    mv "${SCRIPT_PATH}/ipas/*.ipa" "${SCRIPT_PATH}/ipas/${STAGE_IPA_FILE}"
fi

# commit project changes
echo "Committing changes"
#git add "${SCRIPT_PATH}/Assets/Resources/AndroidVersionCode.txt"
#git add "${SCRIPT_PATH}/Assets/Resources/version.txt"
#git add "${SCRIPT_PATH}/ProjectSettings/ProjectSettings.asset"
#git commit -m "Version Changed To ${BUNDLE_ID} and Android Version Code Increased"
#git push origin ${BRANCH}

if $CREATE_TAG; then
    # GENERATE TAG
    git tag $BUNDLE_ID
    # Share tag
    git push origin $BUNDLE_ID
fi

# SEND TO SAMBA SERVER

echo "Sending To Server"
mkdir server
mount -t smbfs "//${SMB_USER}:${SMB_PASS}@ubisoft.org/${SMB_FOLDER}" server

if $BUILD_IOS; then
    cp "${SCRIPT_PATH}/ipas/${STAGE_IPA_FILE}" "server/"
fi
if $BUILD_ANDROID; then
    cp "${SCRIPT_PATH}/*.apk" "server/"
fi
umount server
rmdir server
