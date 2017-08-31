
APK=`ls $1 | grep apk`
APK="${1}/${APK}"
echo "Installing APK"
platform-tools/adb install -r "${APK}"
BUILD_TOOLS_FOLDER=`ls build-tools | tail -1`
APPT="build-tools/${BUILD_TOOLS_FOLDER}/aapt"

echo "Installing OBB"
VERSION_CODE="$($APPT dump badging $APK | grep package | awk '{print $3}' | sed s/versionCode=//g | sed s/\'//g)"
PACKAGE_NAME="$($APPT dump badging $APK | grep package | awk '{print $2}' | sed s/name=//g | sed s/\'//g)"
OBB_NAME="main.${VERSION_CODE}.${PACKAGE_NAME}.obb"

platform-tools/adb push "${1}/${OBB_NAME}" "/sdcard/Android/obb/${PACKAGE_NAME}/${OBB_NAME}"
