
xcode_folder=$1

echo $xcode_folder
INITIAL_PATH="$(pwd)"
# Store script's (project's) absolute path
# To get an absolute path, dir to it and then use pwd command
cd "$(dirname "$0")"
SCRIPT_PATH="$(pwd)"

python ModifyProject.py "${xcode_folder}"


open -a Xcode "${xcode_folder}/Unity-iPhone.xcodeproj"
sleep 10s
osascript AutoRun.applescript
cd "${INITIAL_PATH}"
