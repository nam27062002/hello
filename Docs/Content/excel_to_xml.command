#!/bin/bash

# Show initial feedback
echo "----------- EXPORTING EXCEL TO XML... ------------"
echo " "

# Aux vars
INPUT_FILES=*.xlsx
OUTPUT_DIR=xml
TOOL_EXECUTABLE=xml_content_generator.jar

# Go to script's dir
cd "$(dirname "$0")"

# Make sure export folder exists
mkdir -p $OUTPUT_DIR

# Clear previously exported files
rm -rf $OUTPUT_DIR/*

# Run Java tool for each selected file
for f in $INPUT_FILES
do
	echo "    Exporting $f..."
	java -jar $TOOL_EXECUTABLE $f $OUTPUT_DIR
done

# Git Auto-commit
# TODO!!

# SVN Auto-commit
# "C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe" /command:commit /path:"$OUTPUT_DIR/" /logmsg:"Rules: Content - Auto-commit." /closeonend:1

# Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "
