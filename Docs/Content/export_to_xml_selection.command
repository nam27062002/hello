#!/bin/bash

# Show initial feedback
echo "----------- EXPORTING EXCEL TO XML... ------------"
echo " "

# Aux vars
INPUT_FILES=()	# () to initialize it as an empty array
OUTPUT_DIR=xml
TOOL_EXECUTABLE=xml_content_generator.jar

# Go to script's dir
cd "$(dirname "$0")"

# Make sure export folder exists
mkdir -p $OUTPUT_DIR

# Clear previously exported files
#rm -rf $OUTPUT_DIR/*

# Find all exportable files in the current dir
# From https://askubuntu.com/questions/682095/create-bash-menu-based-on-file-list-map-files-to-numbers
unset options
unset i
while IFS= read -r -d $'\0' f; do 	# Reads the input from find null delimited (-d $'\0')
	options[i++]="$f"
done < <(find . -type f -name "*.xlsx" -print0) 	# Search only for files (-type f) and print them delimited by the null character (-print0)
unset IFS

# Show a selection menu to choose one of the files to be exported
# From https://askubuntu.com/questions/682095/create-bash-menu-based-on-file-list-map-files-to-numbers
echo "Select the file to be exported:"
select opt in "${options[@]}" "CANCEL"; do 	# Add "Cancel" option
	# Which option was selected?
	case $opt in
		# Selected option contains a valid xlsx file
		*.xlsx)
			echo ""
			echo "-------------------------------------------------------"
			echo "$opt SELECTED"
			echo "-------------------------------------------------------"
			echo ""
			INPUT_FILES+=("$opt")	# https://stackoverflow.com/questions/1951506/add-a-new-element-to-an-array-without-specifying-the-index-in-bash
			break;	# Comment this line to select multiple options
		;;

		# Cancel option selected
		"CANCEL")
			echo "Export Canceled"
			exit 1
		;;

		# Any other input
		*)
			echo "Unknown Option $opt"
			exit 1
		;;
	esac
done

# Run Java tool for each selected file
for f in ${INPUT_FILES[@]}; do
	echo "    Exporting $f..."
	java -jar $TOOL_EXECUTABLE $f $OUTPUT_DIR
done

# Show initial feedback
echo "----------- COPYING RULES TO CLIENT... -----------"
echo " "

# Aux vars
INPUT_DIR=xml
OUTPUT_DIR=../../Assets/Resources/Rules

# Prepare parameters for the copy
# rsync allows us to exclude hidden .svn folders :)
# -a Will preserve timestamps, owner, permissions, etc. Not really necessary, but feels right.
# [AOC] Unfortunately, we can't use the -u (update) parameter since git rewrites all timestamps
#       and they can't be used to detect which files have changed, as rsync does. Instead we'll 
#       just copy all the files, slower but 100% safe.
RSYNC_PARAMS="-r -a --exclude=.svn* --exclude=*.meta --exclude=.DS_Store --ignore-errors"

# Perform the import!
# Use the customized parameters var
rsync -v $RSYNC_PARAMS $INPUT_DIR/ $OUTPUT_DIR

# Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "