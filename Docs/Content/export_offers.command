#!/bin/bash

# Show initial feedback
echo "----------- EXPORTING EXCEL TO XML... ------------"
echo " "

# Aux vars
INPUT_FILES=HungryDragonContent_Offers.xlsx
OUTPUT_DIR=xml
TOOL_EXECUTABLE=xml_content_generator.jar

# Go to script's dir
cd "$(dirname "$0")"

# Make sure export folder exists
mkdir -p $OUTPUT_DIR

# Clear previously exported files
#rm -rf $OUTPUT_DIR/*

# Run Java tool for each selected file
for f in $INPUT_FILES
do
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