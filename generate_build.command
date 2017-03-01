#!/bin/Bash
# This script generates Android (apk) and iOS (ipa) builds with a given setup

# Bash setup
set -e  # Exit on error - see http://www.davidpashley.com/articles/writing-robust-shell-scripts/#id2382181

INITIAL_PATH="$(pwd)"

# Store script's (project's) absolute path
# To get an absolute path, dir to it and then use pwd command
cd "$(dirname $0)"
SCRIPT_PATH="$(pwd)"
echo
echo "BUILDER: Running script from path ${SCRIPT_PATH}"

# Default parameters
PROJECT_PATH="${SCRIPT_PATH}"
PROJECT_CODE_NAME=""
  # git config
BRANCH="develop"
RESET_GIT=false
COMMIT_CHANGES=false
CREATE_TAG=false
  # build config
BUILD_ANDROID=false
BUILD_IOS=true
  # versioning
FORCE_VERSION=false
INCREASE_VERSION_NUMBER=true
PROJECT_SETTINGS_PUBLIC_VERSION_IOS=false
PROJECT_SETTINGS_PUBLIC_VERSION_GGP=false
PROJECT_SETTINGS_PUBLIC_VERSION_AMZ=false

OUTPUT_DIR="${HOME}/Desktop/builds"
  # upload config
UPLOAD=false
SMB_FOLDER="BCNStudio/QA/builds"


# iOS Code Sign
PROVISIONING_PROFILE="XC Ad Hoc: com.ubisoft.hungrydragon.dev"  # Not used, just for reference. Make sure it's downloaded in the build machine (XCode->Preferences->Accounts->View Details)
SIGNING_ID="iPhone Distribution: Marie Cordon (Y3J3C97LQ8)" # NOT WORKING!!
#PROVISIONING_PROFILE_UUID="99d18f4a-2a05-4e39-a5da-370321ce140b"
PROVISIONING_PROFILE_UUID="e2a6e917-8663-4459-b97f-6ec3c7e1d3d3" # Get it by right-click on the target provisioning profile in XCode->Preferences->Accounts->View Details and choosing "Show in Finder" (the UUID is the filename of the selected profile)

# SMB Settings
SMB_USER="srv_acc_bcn_jenkins"
SMB_PASS="Lm0%2956jkR%23Tg"

USAGE="Usage: generate_build.command [-path project_path] [-code project_code] [-b branch_name=develop] [-reset true|false*] [-commit true|false*] [-tag true|false*] \
      [-android true|false*] [-ios true*|false] \
      [-version forced_version=] [-increase_version true*|false]  \
      [-iosPublic iosPublicVersion] [-ggpPublic google_play_public_version] [-amzPublic amazonPublicVersion]  \
      [-output dirpath=Desktop/builds] [-upload true|false*] [-smbOutput server_folder=]"

# Parse parameters
for ((i=1;i<=$#;i++));
do
    PARAM_NAME=${!i}
    if [ "$PARAM_NAME" == "--help" ] ; then
        echo $USAGE
        exit 1
    elif [ "$PARAM_NAME" == "-path" ] ; then
        ((i++))
        PROJECT_PATH=${!i}
    elif [ "$PARAM_NAME" == "-code" ] ; then
        ((i++))
        PROJECT_CODE_NAME=${!i}

    elif [ "$PARAM_NAME" == "-b" ] ; then
        ((i++))
        BRANCH=${!i}
    elif [ "$PARAM_NAME" == "-reset" ] ; then
        ((i++))
        RESET_GIT=${!i}
    elif [ "$PARAM_NAME" == "-commit" ]; then
        ((i++))
        COMMIT_CHANGES=${!i}
    elif [ "$PARAM_NAME" == "-tag" ]; then
        ((i++))
        CREATE_TAG=${!i}

    elif [ "$PARAM_NAME" == "-android" ]; then
        ((i++))
        BUILD_ANDROID=${!i}
    elif [ "$PARAM_NAME" == "-ios" ]; then
        ((i++))
        BUILD_IOS=${!i}
    elif [ "$PARAM_NAME" == "-version" ] ; then
        ((i++))
        FORCE_VERSION=${!i}
    elif [ "$PARAM_NAME" == "-increase_version" ]; then
        ((i++))
        INCREASE_VERSION_NUMBER=${!i}

    elif [ "$PARAM_NAME" == "-iosPublic" ]; then
        ((i++))
        PROJECT_SETTINGS_PUBLIC_VERSION_IOS=${!i}
    elif [ "$PARAM_NAME" == "-ggpPublic" ] ; then
        ((i++))
        PROJECT_SETTINGS_PUBLIC_VERSION_GGP=${!i}
    elif [ "$PARAM_NAME" == "-amzPublic" ]; then
        ((i++))
        PROJECT_SETTINGS_PUBLIC_VERSION_AMZ=${!i}

    elif [ "$PARAM_NAME" == "-output" ] ; then
        ((i++))
        OUTPUT_DIR=${!i}

    elif [ "$PARAM_NAME" == "-upload" ] ; then
        ((i++))
        UPLOAD=${!i}
    elif [ "$PARAM_NAME" == "-smbOutput" ] ; then
        ((i++))
        SMB_FOLDER=${!i}
    else
        echo "Unknown parameter ${PARAM_NAME}"
        echo $USAGE
        exit 1
    fi
done;

# Flow Control
print_step()
{
  echo "STEP ${1} of ${TOTAL_STEPS}"
}

print_builder()
{
  echo
  print_step $CURRENT_STEP
  CURRENT_STEP=$((CURRENT_STEP+1))
  echo "BUILDER: " + $1
}

# Calculate num of stps
TOTAL_STEPS=7;
if $RESET_GIT; then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi
if [ $FORCE_VERSION ] || [ $INCREASE_VERSION_NUMBER ] ; then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi
if $BUILD_ANDROID;then
  TOTAL_STEPS=$((TOTAL_STEPS+2));
fi
if $BUILD_IOS;then
  TOTAL_STEPS=$((TOTAL_STEPS+4));
fi
if $COMMIT_CHANGES;then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
  if $CREATE_TAG;then
    TOTAL_STEPS=$((TOTAL_STEPS+1));
  fi
fi
if $UPLOAD;then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi

echo "TOTAL STEPS " + ${TOTAL_STEPS}
CURRENT_STEP=0

# Other internal vars
UNITY_APP="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
DATE="$(date +%Y%m%d)"
START_TIME=$(date +%s)

# Initialize default Unity parameters
UNITY_PARAMS="-batchmode -projectPath \"${PROJECT_PATH}\" -logfile -nographics -quit"

# Move to project path
cd "${PROJECT_PATH}"

if $RESET_GIT: then
  print_builder "Reset Git"
  # Update git
  # Revert changes to modified files.
  git reset --hard

  # Remove untracked files and directories.
  git clean -fd
fi
# Change branch
git fetch
git checkout "${BRANCH}"

# Update branch
print_builder "Pulling Branch ${BRANCH}"
git pull origin "${BRANCH}"

# Update calety
print_builder "Updating Calety"
cd Calety
git pull
cd ..

if $FORCE_VERSION: then
  print_builder "Force Version ${FORCE_VERSION}"
  "${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.SetInternalVersion -version FORCE_VERSION
fi

# Increase internal version number
if $INCREASE_VERSION_NUMBER && !$FORCE_VERSION; then
    print_builder "Increasing internal version number..."
    #set +e  # For some unknown reason, in occasions the Builder.IncreaseMinorVersionNumber causes an error, making the script to stop - Disable exitOnError for this single instruction
    "${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.IncreaseMinorVersionNumber | grep "BUILDER"
    #set -e
fi

# Read internal version number
# Unity creates a tmp file outputVersion.txt with the version number in it. Read from it and remove it.
print_builder "Reading internal version number..."
"${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.OutputVersion | grep "BUILDER"
VERSION_ID="$(cat outputVersion.txt)"
echo $VERSION_ID
rm -f "outputVersion.txt"

// Set public version
PUBLIC_VERSION_PARAMS=""
if $BUILD_IOS;then
  if !$PROJECT_SETTINGS_PUBLIC_VERSION_IOS;then
      PROJECT_SETTINGS_PUBLIC_VERSION_IOS="${VERSION_ID}"
  fi
	PUBLIC_VERSION_PARAMS += "${PUBLIC_VERSION_PARAMS} -ios ${PROJECT_SETTINGS_PUBLIC_VERSION_IOS}";
fi

if $BUILD_ANDROID;then
  if !$PROJECT_SETTINGS_PUBLIC_VERSION_GGP;then
      PROJECT_SETTINGS_PUBLIC_VERSION_GGP="${VERSION_ID}"
  fi
  if !$PROJECT_SETTINGS_PUBLIC_VERSION_AMZ;then
      PROJECT_SETTINGS_PUBLIC_VERSION_AMZ="${VERSION_ID}"
  fi
  PUBLIC_VERSION_PARAMS += "${PUBLIC_VERSION_PARAMS} -ggp ${PROJECT_SETTINGS_PUBLIC_VERSION_GGP}";
  PUBLIC_VERSION_PARAMS += "${PUBLIC_VERSION_PARAMS} -amz ${PROJECT_SETTINGS_PUBLIC_VERSION_AMZ}";
fi
print_builder "Settign public version numbers";
"${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.SetPublicVersion "${PUBLIC_VERSION_PARAMS}"| grep "BUILDER"

# Make sure output dir is exists
mkdir -p "${OUTPUT_DIR}"    # -p to create all parent hierarchy if needed (and to not exit with error if folder already exists)

# Increase build unique code
print_builder "Increasing Build Code"
"${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.IncreaseVersionCodes | grep "BUILDER"

# Generate Android build
if $BUILD_ANDROID; then
  print_builder "BUILDER: Generating APKs..."

  # Make sure output dir exists
  mkdir -p "${OUTPUT_DIR}/apks/"

  # Do it!
  "${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.GenerateAPK -buildTarget android -outputDir "${OUTPUT_DIR}" | grep "BUILDER"

  // Unity creates a tmp file androidBuildVersion.txt with the android build version number in it. Read from it and remove it.
	print_builder "BUILDER: Reading internal andoird build version number";
  "${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.OutputAndroidBuildVersion | grep "BUILDER"
	ANDROID_BUILD_VERSION=$(cat \"${PROJECT_PATH}/androidBuildVersion.txt\")"
	rm -f ""\"${PROJECT_PATH}/androidBuildVersion.txt\"";
	STAGE_APK_FILE="${PROJECT_CODE_NAME}_${VERSION_ID}_\"$(DATE)\"_b${ANDROID_BUILD_VERSION}.apk";
fi

# Generate iOS build
if $BUILD_IOS; then
    # Make sure output dirs exist
    mkdir -p "${OUTPUT_DIR}/archives/"
    mkdir -p "${OUTPUT_DIR}/ipas/"

    # Generate XCode project
    print_builder "Generating XCode Project"
    "${UNITY_APP}" "${UNITY_PARAMS}" -executeMethod Builder.GenerateXcode -buildTarget ios -outputDir "${OUTPUT_DIR}" | grep "BUILDER"

    # Stage target files
    # BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "$SCRIPT_PATH/xcode/Info.plist")
    ARCHIVE_FILE="${PROJECT_CODE_NAME}_${VERSION_ID}.xcarchive"
    STAGE_IPA_FILE="${PROJECT_CODE_NAME}_${VERSION_ID}_${DATE}.ipa"
    PROJECT_NAME="${OUTPUT_DIR}/xcode/Unity-iPhone.xcodeproj"

    # Generate Archive
    print_builder "Cleaning XCode build"
    xcodebuild clean -project "${PROJECT_NAME}" -configuration Release -alltargets

    print_builder "Archiving"
    # Since the archiving process has a lot of verbose (and XCode doesn't allow us to regulate it), show only the relevant lines
    xcodebuild archive -project "${PROJECT_NAME}" -configuration Release -scheme "Unity-iPhone" -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" PROVISIONING_PROFILE="${PROVISIONING_PROFILE_UUID}"

    # Generate IPA file
    print_builder "Exporting IPA"
    rm -f "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}"    # just in case
    xcodebuild -exportArchive -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" -exportPath "${OUTPUT_DIR}/ipas/" -exportOptionsPlist "${OUTPUT_DIR}/xcode/Info.plist"
    mv -f "${OUTPUT_DIR}/ipas/Unity-iPhone.ipa" "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}"
fi

# Commit project changes
if $COMMIT_CHANGES;then
  print_builder "Committing changes"
  git add "${SCRIPT_PATH}/Assets/Resources/Singletons/GameSettings.asset"
  git add "${SCRIPT_PATH}/Assets/Resources/CaletySettings.asset"
  git add "${SCRIPT_PATH}/ProjectSettings/ProjectSettings.asset"
  git commit -m "Automatic Build. Version ${VERSION_ID}."
  git push origin "${BRANCH}"

  # Create Git tag
  if $CREATE_TAG; then
      print_builder "Pushing Tag ${VERSION_ID}"
      set +e  # Don't exit script on error
      git tag "${VERSION_ID}"
      git push origin "${VERSION_ID}"
      set -e
  fi
fi

if $UPLOAD;then
  # Upload to Samba server
  print_builder "Sending to server"

  # Mount the server into a tmp folder
  mkdir -p server
  mount -t smbfs "//${SMB_USER}:${SMB_PASS}@ubisoft.org/${SMB_FOLDER}" server

  # Copy IPA
  if $BUILD_IOS; then
      cp "${OUTPUT_DIR}/ipas/${STAGE_IPA_FILE}" "server/"
  fi

  # Copy APK
  if $BUILD_ANDROID; then
      cp "${OUTPUT_DIR}/apks/${STAGE_APK_FILE}" "server/"
  fi

  # Unmount server and remove tmp folder
  umount server
  rmdir server
fi

# Done!
END_TIME=$(date "+%s")
DIFF=$(echo "$END_TIME - $START_TIME" | bc)

print_builder "Done in $(date -j -u -f "%s  ${DIFF} +%Hh %Mm %Ss")!" # -j to not actually set the date, -u for UTC time, -f to specify input format (%s -> seconds)

cd "${INITIAL_PATH}"
