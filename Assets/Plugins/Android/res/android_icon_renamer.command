#!/bin/bash

# Declare some constants
declare -a FOLDERS=("mipmap-hdpi" "mipmap-mdpi" "mipmap-xhdpi" "mipmap-xxhdpi" "mipmap-xxxhdpi")
declare -a SOURCE_FILES=("ic_launcher_background.png"   "ic_launcher_foreground.png" "ic_launcher_round.png" "ic_launcher.png")
declare -a DESTINATION_FILES=("app_icon_background.png" "app_icon_foreground.png"    "app_icon_round.png"    "app_icon.png")

# Go to script's dir
cd "$(dirname "$0")"

# Iterate folders of different resolutions
for folder in "${FOLDERS[@]}"; do
	# Go to folder
	echo "$folder"
	cd $folder
	pwd

	# Rename files
	for ((i=0;i<${#SOURCE_FILES[@]};++i)); do
		from="${SOURCE_FILES[i]}"
		to="${DESTINATION_FILES[i]}"
		
		echo "    Renaming $from into $to"

		rm -f $to
		mv $from $to
		rm $from
	done

	# Go back to root folder
	echo ""
	cd ..
done

# Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "