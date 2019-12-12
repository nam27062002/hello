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

# Default parameters
PROJECT_PATH="${SCRIPT_PATH}"
PROJECT_CODE_NAME="xx"
  # git config
BRANCH="develop"
CALETY_BRANCH="hungrydragon"
RESET_GIT=false
COMMIT_CHANGES=false
CREATE_TAG=false

#XCode project type
XCWORKSPACE=false

  # build config
BUILD_ANDROID=false
GENERATE_OBB=false
GENERATE_AAB=false
BUILD_IOS=false
ENVIRONMENT=false
ADDRESSABLES_MODE=Catalog
  # versioning
FORCE_VERSION=false
INCREASE_VERSION_NUMBER=false
PROJECT_SETTINGS_PUBLIC_VERSION_IOS=false
PROJECT_SETTINGS_PUBLIC_VERSION_GGP=false
PROJECT_SETTINGS_PUBLIC_VERSION_AMZ=false

INCREASE_VERSION_CODE_NUMBER=false
PROJECT_SETTINGS_VERSION_CODE_IOS=false
PROJECT_SETTINGS_VERSION_CODE_GGP=false
PROJECT_SETTINGS_VERSION_CODE_AMZ=false

OUTPUT_DIR="${HOME}/Desktop/builds"
  # upload config
UPLOAD=false
SMB_FOLDER="BCNStudio/QA/builds"


# iOS Code Sign
CODE_SIGN_IDENTITY="iPhone Distribution"
#PROVISIONING_PROFILE_UUID="86c9ccf0-d239-45aa-b867-03a91ca719f1"
PROVISIONING_PROFILE_UUID="30613b04-01be-4887-9e75-02ad893837b4"
DEVELOPMENT_TEAM="Y3J3C97LQ8"

# SMB Settings
SMB_USER="srv_acc_bcn_jenkins"
SMB_PASS="Lm0%2956jkR%23Tg"

USAGE="Usage: generate_build.command [-path project_path=script_path] [-code project_code=xx] [-b branch_name=develop] [-reset_git] [-commit] [-tag] \
      [-android][-obb][-ios][-iosTeam teamId] [-iosProvisioningId provisioningId]\
      [-version forced_version] [-increase_version]  \
      [-iosPublic iosPublicVersion] [-ggpPublic google_play_public_version] [-amzPublic amazonPublicVersion]  \
      [-increase_VCodes] [-iosVCode ios_version_code] [-ggpVCode google_play_version_code] [-amzVCode amazon_version_code]  \
      [-output dirpath=Desktop/builds] [-upload] [-smbOutput server_folder=] \
      [-env environment] [-addressablesMode addressablesMode] [-calety_branch branch]"

# Parse parameters
for ((i=1;i<=$#;i++));
do
    PARAM_NAME=${!i}
    if [ "$PARAM_NAME" == "--help" ] ; then
        echo $USAGE
        echo ""
        exit 0
    elif [ "$PARAM_NAME" == "-path" ] ; then
        ((i++))
        PROJECT_PATH=${!i}
    elif [ "$PARAM_NAME" == "-code" ] ; then
        ((i++))
        PROJECT_CODE_NAME=${!i}

    elif [ "$PARAM_NAME" == "-b" ] ; then
        ((i++))
        BRANCH=${!i}
    elif [ "$PARAM_NAME" == "-reset_git" ] ; then
        RESET_GIT=true
    elif [ "$PARAM_NAME" == "-commit" ]; then
        COMMIT_CHANGES=true
    elif [ "$PARAM_NAME" == "-tag" ]; then
        CREATE_TAG=true

    elif [ "$PARAM_NAME" == "-android" ]; then
        BUILD_ANDROID=true
    elif [ "$PARAM_NAME" == "-obb" ]; then
        GENERATE_OBB=true
    elif [ "$PARAM_NAME" == "-aab" ]; then
        GENERATE_AAB=true        
    elif [ "$PARAM_NAME" == "-ios" ]; then
        BUILD_IOS=true
    elif [ "$PARAM_NAME" == "-iosTeam" ] ; then
        ((i++))
        DEVELOPMENT_TEAM=${!i}
    elif [ "$PARAM_NAME" == "-iosProvisioningId" ] ; then
        ((i++))
        PROVISIONING_PROFILE_UUID=${!i}

    elif [ "$PARAM_NAME" == "-version" ] ; then
        ((i++))
        FORCE_VERSION=${!i}
    elif [ "$PARAM_NAME" == "-increase_version" ]; then
        INCREASE_VERSION_NUMBER=true

    elif [ "$PARAM_NAME" == "-iosPublic" ]; then
        ((i++))
        PROJECT_SETTINGS_PUBLIC_VERSION_IOS=${!i}
    elif [ "$PARAM_NAME" == "-ggpPublic" ] ; then
        ((i++))
        PROJECT_SETTINGS_PUBLIC_VERSION_GGP=${!i}
    elif [ "$PARAM_NAME" == "-amzPublic" ]; then
        ((i++))
        PROJECT_SETTINGS_PUBLIC_VERSION_AMZ=${!i}

    elif [ "$PARAM_NAME" == "-increase_VCodes" ]; then
        INCREASE_VERSION_CODE_NUMBER=true
    elif [ "$PARAM_NAME" == "-iosVCode" ]; then
        ((i++))
        PROJECT_SETTINGS_VERSION_CODE_IOS=${!i}
    elif [ "$PARAM_NAME" == "-ggpVCode" ] ; then
        ((i++))
        PROJECT_SETTINGS_VERSION_CODE_GGP=${!i}
    elif [ "$PARAM_NAME" == "-amzVCode" ]; then
        ((i++))
        PROJECT_SETTINGS_VERSION_CODE_AMZ=${!i}

    elif [ "$PARAM_NAME" == "-xcworkspace"] ; then
        XCWORKSPACE=true

    elif [ "$PARAM_NAME" == "-output" ] ; then
        ((i++))
        OUTPUT_DIR=${!i}

    elif [ "$PARAM_NAME" == "-upload" ] ; then
        UPLOAD=true
    elif [ "$PARAM_NAME" == "-smbOutput" ] ; then
        ((i++))
        SMB_FOLDER=${!i}
    elif [ "$PARAM_NAME" == "-env" ] ; then
        ((i++))
        ENVIRONMENT=${!i}
    elif [ "$PARAM_NAME" == "-addressablesMode" ] ; then
        ((i++))
        ADDRESSABLES_MODE=${!i}
    elif [ "$PARAM_NAME" == "-calety_branch" ] ; then
        ((i++))
        CALETY_BRANCH=${!i}
    else
        echo "BUILDER: Unknown parameter ${PARAM_NAME}"
        echo "BUILDER: ${USAGE}"
        exit 1
    fi
done;

# Flow Control
print_builder() {
  echo
  echo "STEP ${CURRENT_STEP} of ${TOTAL_STEPS}"
  CURRENT_STEP=$((CURRENT_STEP+1))
  echo "BUILDER: ${1}"
}

# Calculate num of stps
TOTAL_STEPS=6;
if $RESET_GIT; then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi
if [ "$ENVIRONMENT" != false ]; then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi
if [ "$FORCE_VERSION" != false ]; then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi
if [ "$INCREASE_VERSION_NUMBER" == true ]; then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi

if [ "$INCREASE_VERSION_CODE_NUMBER" == true ]; then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi

if [ "$BUILD_IOS" == true -a "$PROJECT_SETTINGS_PUBLIC_VERSION_IOS" != false ] || ( "$BUILD_ANDROID" == true  && [ "$PROJECT_SETTINGS_PUBLIC_VERSION_GGP" != false -o "$PROJECT_SETTINGS_PUBLIC_VERSION_AMZ" != false ] );then
  TOTAL_STEPS=$((TOTAL_STEPS+1));
fi

if [ "$BUILD_IOS" == true -a "$PROJECT_SETTINGS_VERSION_CODE_IOS" != false ] || ( "$BUILD_ANDROID" == true  && [ "$PROJECT_SETTINGS_VERSION_CODE_GGP" != false -o "$PROJECT_SETTINGS_VERSION_CODE_AMZ" != false ] );then
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
echo "TOTAL STEPS ${TOTAL_STEPS}"
CURRENT_STEP=1

# Other internal vars
UNITY_APP="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
DATE="$(date +%Y%m%d)"
START_TIME=$(date +%s)

# Initialize default Unity parameters
UNITY_PARAMS="-batchmode -projectPath \"${PROJECT_PATH}\" -logfile -nographics -quit"

# Move to project path
cd "${PROJECT_PATH}"

cd Calety
if $RESET_GIT; then
  git reset --hard  
  git clean -fd  
fi
# Update calety
print_builder "Updating Calety"
git checkout "${CALETY_BRANCH}"
git pull
cd ..

if $RESET_GIT; then
  print_builder "Reset Git"
  # Update git
  # Revert changes to modified files.
  git reset --hard

  # Remove untracked files and directories.
  git clean -fd

  git submodule update
fi

# Change branch
git fetch
git checkout "${BRANCH}"

# Update branch
print_builder "Pulling Branch ${BRANCH}"
git pull origin "${BRANCH}"

print_builder "Custom Builder Action"
eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.CustomAction"

print_builder "Setting addressables mode"
eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.SetAddressablesMode -addressablesMode ${ADDRESSABLES_MODE}"

if [ "$ENVIRONMENT" != false ]; then
    print_builder "Setting environment";
    eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.SetEnvironment -env ${ENVIRONMENT}"
fi

eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.OutputEnvironment"
ENVIRONMENT="$(cat environment.txt)"
echo "Environment: ${ENVIRONMENT}"
rm -f "environment.txt"

if [ "$FORCE_VERSION" != false ]; then
  print_builder "Force Version ${FORCE_VERSION}"
  eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.SetInternalVersion -version ${FORCE_VERSION}"
fi

# Increase internal version number
if [ "$INCREASE_VERSION_NUMBER" == true ]; then
    print_builder "Increasing internal version number..."
    #set +e  # For some unknown reason, in occasions the Builder.IncreaseMinorVersionNumber causes an error, making the script to stop - Disable exitOnError for this single instruction
    eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.IncreaseMinorVersionNumber"
    #set -e
fi

# Read internal version number
# Unity creates a tmp file outputVersion.txt with the version number in it. Read from it and remove it.
print_builder "Reading internal version number..."
eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.OutputVersion"
VERSION_ID="$(cat outputVersion.txt)"
echo $VERSION_ID
rm -f "outputVersion.txt"

# Set public version
PUBLIC_VERSION_PARAMS=""
if $BUILD_IOS;then
  if [ "$PROJECT_SETTINGS_PUBLIC_VERSION_IOS" != false ];then
    PUBLIC_VERSION_PARAMS="${PUBLIC_VERSION_PARAMS} -ios ${PROJECT_SETTINGS_PUBLIC_VERSION_IOS}"
  fi
fi

if $BUILD_ANDROID;then
  if [ "$PROJECT_SETTINGS_PUBLIC_VERSION_GGP" != false ];then
    PUBLIC_VERSION_PARAMS="${PUBLIC_VERSION_PARAMS} -ggp ${PROJECT_SETTINGS_PUBLIC_VERSION_GGP}"
  fi
  if [ "$PROJECT_SETTINGS_PUBLIC_VERSION_AMZ" != false ];then
    PUBLIC_VERSION_PARAMS="${PUBLIC_VERSION_PARAMS} -amz ${PROJECT_SETTINGS_PUBLIC_VERSION_AMZ}"
  fi
fi

if [ "$PUBLIC_VERSION_PARAMS" != "" ]; then
    print_builder "Settign public version numbers";
    eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.SetPublicVersion ${PUBLIC_VERSION_PARAMS}"
fi
# Make sure output dir is exists
mkdir -p "${OUTPUT_DIR}"    # -p to create all parent hierarchy if needed (and to not exit with error if folder already exists)

# Increase build unique code
if [ "$INCREASE_VERSION_CODE_NUMBER" != false ]; then
    print_builder "Increasing Version Code"
    eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.IncreaseVersionCodes"
fi

# Set version code
VERSION_CODE_PARAMS=""
if $BUILD_IOS;then
  if [ "$PROJECT_SETTINGS_VERSION_CODE_IOS" != false ];then
    VERSION_CODE_PARAMS="${VERSION_CODE_PARAMS} -ios ${PROJECT_SETTINGS_VERSION_CODE_IOS}"
  fi
fi

if $BUILD_ANDROID;then
  if [ "$PROJECT_SETTINGS_VERSION_CODE_GGP" != false ];then
    VERSION_CODE_PARAMS="${VERSION_CODE_PARAMS} -ggp ${PROJECT_SETTINGS_VERSION_CODE_GGP}"
  fi
  if [ "$PROJECT_SETTINGS_VERSION_CODE_AMZ" != false ];then
    VERSION_CODE_PARAMS="${VERSION_CODE_PARAMS} -amz ${PROJECT_SETTINGS_VERSION_CODE_AMZ}"
  fi
fi

if [ "$VERSION_CODE_PARAMS" != "" ]; then
    print_builder "Settign version code numbers";
    eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.SetVersionCode ${VERSION_CODE_PARAMS}"
fi


print_builder "Updating version number..."
eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.updateVersionNumbers"


eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.OutputBundleIdentifier"
PACKAGE_NAME="$(cat bundleIdentifier.txt)"
rm -f "bundleIdentifier.txt"

# Generate Android build
if $BUILD_ANDROID; then
  print_builder "BUILDER: Generating APKs..."

  # Make sure output dir exists
  mkdir -p "${OUTPUT_DIR}/apks/"

  # Do it!
  eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.GenerateAPK -buildTarget android -outputDir \"${OUTPUT_DIR}/apks/\" -obb ${GENERATE_OBB} -aab ${GENERATE_AAB} -code ${PROJECT_CODE_NAME} -addressablesMode ${ADDRESSABLES_MODE}"

  # Unity creates a tmp file androidBuildVersion.txt with the android build version number in it. Read from it and remove it.
	print_builder "BUILDER: Reading internal android build version number";
  eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.OutputAndroidBuildVersion"
	ANDROID_BUILD_VERSION="$(cat androidBuildVersion.txt)"
	rm -f "androidBuildVersion.txt";
  APK_NAME="${PROJECT_CODE_NAME}_${VERSION_ID}_${DATE}_b${ANDROID_BUILD_VERSION}_${ENVIRONMENT}_${ADDRESSABLES_MODE}"

  if $GENERATE_AAB; then
  APK_FILE="${APK_NAME}.aab"
  else
  APK_FILE="${APK_NAME}.apk"
  fi

  APK_OUTPUT_DIR="${OUTPUT_DIR}/apks/${APK_NAME}"
  mkdir -p "${APK_OUTPUT_DIR}"
  mv "${OUTPUT_DIR}/apks/${APK_FILE}" "${APK_OUTPUT_DIR}/"

  if $GENERATE_OBB; then
    OBB_FILE="main.${ANDROID_BUILD_VERSION}.${PACKAGE_NAME}.obb"
    mv "${OUTPUT_DIR}/apks/${APK_NAME}.main.obb" "${APK_OUTPUT_DIR}/${OBB_FILE}"
  fi
fi

# Generate iOS build
if $BUILD_IOS; then
    # Make sure output dirs exist
    mkdir -p "${OUTPUT_DIR}/archives/"
    mkdir -p "${OUTPUT_DIR}/ipas/"

    # Generate XCode project
    print_builder "Generating XCode Project"
    eval "${UNITY_APP} ${UNITY_PARAMS} -executeMethod Builder.GenerateXcode -buildTarget ios -outputDir \"${OUTPUT_DIR}\""

    # Stage target files
    # BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "$SCRIPT_PATH/xcode/Info.plist")
    ARCHIVE_FILE="${PROJECT_CODE_NAME}_${VERSION_ID}_${ENVIRONMENT}.xcarchive"
    IPA_NAME="${PROJECT_CODE_NAME}_${VERSION_ID}_${DATE}_${ENVIRONMENT}_${ADDRESSABLES_MODE}"
    IPA_FILE="${IPA_NAME}.ipa"
    PROJECT_NAME="${OUTPUT_DIR}/xcode/Unity-iPhone"

    # Generate Archive
    # print_builder "Cleaning XCode build"
    # xcodebuild clean -project "${PROJECT_NAME}" -configuration Release -alltargets

    security unlock-keychain -p Ubisoft001

    print_builder "Archiving"
    rm -rf "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}"    # just in case
    sed -i "" "s|ProvisioningStyle = Automatic;|ProvisioningStyle = Manual;|" "${PROJECT_NAME}/project.pbxproj" # for archive to work we need it to be manual
    
    if $XCWORKSPACE; then
      print_builder "XCode project type: .xcworkspace"
      xcodebuild clean archive -workspace "${PROJECT_NAME}.xcworkspace" -configuration Release -scheme "Unity-iPhone" -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" DEVELOPMENT_TEAM="${DEVELOPMENT_TEAM}" PROVISIONING_PROFILE="${PROVISIONING_PROFILE_UUID}" CODE_SIGN_IDENTITY="${CODE_SIGN_IDENTITY}"
    else
      print_builder "XCode project type: .xcodeproj"
      xcodebuild clean archive -project "${PROJECT_NAME}.xcodeproj" -configuration Release -scheme "Unity-iPhone" -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" DEVELOPMENT_TEAM="${DEVELOPMENT_TEAM}" PROVISIONING_PROFILE="${PROVISIONING_PROFILE_UUID}" CODE_SIGN_IDENTITY="${CODE_SIGN_IDENTITY}"
    fi

    # Generate IPA file
    print_builder "Exporting IPA"
    rm -f "${OUTPUT_DIR}/ipas/${IPA_FILE}"    # just in case

    echo "<?xml version=\"1.0\" encoding=\"UTF-8\"?> \
      <!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"> \
      <plist version=\"1.0\"> \
      <dict> \
        <key>method</key> \
        <string>ad-hoc</string> \
        <key>provisioningProfiles</key> \
        <dict> \
          <key>${PACKAGE_NAME}</key> \
          <string>${PROVISIONING_PROFILE_UUID}</string> \
        </dict> \
        <key>teamId</key> \
        <string>${DEVELOPMENT_TEAM}</string> \
      </dict> \
      </plist>" > build.plist

    xcodebuild -exportArchive -archivePath "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}" -exportPath "${OUTPUT_DIR}/ipas/" -exportOptionsPlist "build.plist"
    mv -f "${OUTPUT_DIR}/ipas/Unity-iPhone.ipa" "${OUTPUT_DIR}/ipas/${IPA_FILE}"
fi

if $UPLOAD;then
  # Upload to Samba server
  print_builder "Sending to server"

  # Mount the server into a tmp folder
  # If the temp dir already exists, try to unmount and delete it first
  SMB_MOUNT_DIR="server"
  if [ -d "$SMB_MOUNT_DIR" ]; then
    set +e  # Dont exit script on error (in case the server is not actually mounted but the directory exists anyway)
    diskutil unmount "${SMB_MOUNT_DIR}"
    rmdir "${SMB_MOUNT_DIR}"
    set -e
  fi

  # Now mount the server!
  mkdir -p "${SMB_MOUNT_DIR}"
  mount -t smbfs "//${SMB_USER}:${SMB_PASS}@ubisoft.org/${SMB_FOLDER}" "${SMB_MOUNT_DIR}"

  # In order to keep the server organized, replicate the branches structure on it
  SMB_PATH="${SMB_MOUNT_DIR}/${BRANCH}"

  # Copy IPA
  if $BUILD_IOS; then

      CURRENT_PATH="$(pwd)"
      cd "${OUTPUT_DIR}/archives/"
      cp -r "${ARCHIVE_FILE}/dSYMs" "dSYMs"
      zip -r "${ARCHIVE_FILE}.zip" "dSYMs"
      rm -rf "dSYMs"
      cd "${CURRENT_PATH}" 

      mkdir -p "${SMB_PATH}"
      cp "${OUTPUT_DIR}/ipas/${IPA_FILE}" "${SMB_PATH}/"      
      cp "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}.zip" "${SMB_PATH}/"
      rm "${OUTPUT_DIR}/archives/${ARCHIVE_FILE}.zip"            
    
  fi

  # Copy APK
  if $BUILD_ANDROID; then
  	  # If generating OBBs, create a single folder with the APK + the OBBs
  	  SMB_PATH_APK="${SMB_PATH}"
  	  if $GENERATE_OBB; then
	    SMB_PATH_APK="${SMB_PATH}/${APK_NAME}"
  	  fi

  	  # Make sure path exists
  	  mkdir -p "${SMB_PATH_APK}"

  	  # Copy APK
  	  cp "${APK_OUTPUT_DIR}/${APK_FILE}" "${SMB_PATH_APK}/"

  	  # Copy OBBs
      if $GENERATE_OBB; then
        cp "${APK_OUTPUT_DIR}/${OBB_FILE}" "${SMB_PATH_APK}/"
      fi
  fi

  # Unmount server and remove tmp folder
  diskutil unmount "${SMB_MOUNT_DIR}"
  rmdir "${SMB_MOUNT_DIR}"
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
      set +e  # Dont exit script on error
      git tag "release/${VERSION_ID}"
      git push origin "release/${VERSION_ID}"
      set -e
  fi
fi

# Done!
END_TIME=$(date +%s)
DIFF=$(echo "$END_TIME - $START_TIME" | bc)
DIFF=$(date -j -u -f %s ${DIFF} +%Hh:%Mm:%Ss)
print_builder "Done in ${DIFF}!" # -j to not actually set the date, -u for UTC time, -f to specify input format (%s -> seconds)

cd "${INITIAL_PATH}"
exit 0
