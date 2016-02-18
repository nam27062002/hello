#!/bin/bash

echo "//---------------------------------------------//"
echo "//          EXPORTING EXCEL TO XML...          //"
echo "//---------------------------------------------//"
echo " "

# Aux vars
INPUT_FILE=HungryDragonContent.xlsx
OUTPUT_DIR=xml
TOOL_EXECUTABLE=XML_Content_Generator_v3_2.jar

# Go to script's dir
cd "$(dirname "$0")"

# Make sure export folder exists
mkdir -p $OUTPUT_DIR

# Run Java tool
java -jar $TOOL_EXECUTABLE $INPUT_FILE $OUTPUT_DIR

# Git Auto-commit
# TODO!!

# SVN Auto-commit
# "C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe" /command:commit /path:"$OUTPUT_DIR/" /logmsg:"Rules: Content - Auto-commit." /closeonend:1

echo "//---------------------------------------------//"
echo "//                    PROFIT!!                 //"
echo "//---------------------------------------------//"
echo " "
