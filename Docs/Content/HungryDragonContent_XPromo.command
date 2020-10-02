#!/bin/bash

# Show initial feedback
echo "----------- EXPORTING EXCEL TO XML... ------------"
echo " "

# Aux vars
EXCEL_TO_EXPORT=HungryDragonContent_XPromo.xlsx
TMP_DIR=tmp
OUTPUT_DIR=xml
PROJECT_DIR=../../Assets/Resources/Rules
TOOL_EXECUTABLE=xml_content_generator.jar

# Go to script's dir
cd "$(dirname "$0")"

# ---------------------------------------------------------------------------
# 1. Export Excel tables into xml files in a TMP folder
# ---------------------------------------------------------------------------

# Make sure export folder exists
mkdir -p $TMP_DIR

# Clear previously exported files, if any
rm -rf $TMP_DIR/*

# Run Java tool for target Excel file
echo "    Exporting $EXCEL_TO_EXPORT..."
java -jar $TOOL_EXECUTABLE $EXCEL_TO_EXPORT $TMP_DIR

# ---------------------------------------------------------------------------
# 2. Copy xml files to both output dir and project dir
# ---------------------------------------------------------------------------

# Make sure both folders exists
mkdir -p $OUTPUT_DIR
mkdir -p $PROJECT_DIR

# Prepare parameters for the copy
# rsync allows us to exclude hidden .svn folders and other patterns :)
# -a Will preserve timestamps, owner, permissions, etc. Not really necessary, but feels right.
# [AOC] Unfortunately, we can't use the -u (update) parameter since git rewrites all timestamps
#       and they can't be used to detect which files have changed, as rsync does. Instead we'll 
#       just copy all the files, slower but 100% safe.
RSYNC_PARAMS="-r -a --exclude=.svn* --exclude=*.meta --exclude=.DS_Store --ignore-errors"

# Perform the copy!
# Use the customized parameters var
rsync -v $RSYNC_PARAMS $TMP_DIR/ $OUTPUT_DIR
rsync -v $RSYNC_PARAMS $TMP_DIR/ $PROJECT_DIR

# ---------------------------------------------------------------------------
# 3. Clean temp files
# ---------------------------------------------------------------------------

# Clear temporal folder and files
rm -rf $TMP_DIR

# ---------------------------------------------------------------------------
# 4. Update Version Control
# ---------------------------------------------------------------------------

# Git Auto-commit
# TODO!!

# SVN Auto-commit
# TODO!!

# ---------------------------------------------------------------------------
# DONE!
# ---------------------------------------------------------------------------

# Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "