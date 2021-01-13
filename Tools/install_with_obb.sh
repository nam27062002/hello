./adb install -r $1
VERSION_CODE="$(aapt dump badging $1 | grep package | awk '{print $3}' | sed s/versionCode=//g | sed s/\'//g)"
PACKAGE_NAME="$(aapt dump badging $1 | grep package | awk '{print $2}' | sed s/name=//g | sed s/\'//g)"
echo $PACKAGE_NAME
echo $VERSION_CODE
OBB_NAME="main.${VERSION_CODE}.${PACKAGE_NAME}.obb"
./adb push "${OBB_NAME}" "/sdcard/Android/obb/${PACKAGE_NAME}/${OBB_NAME}"
