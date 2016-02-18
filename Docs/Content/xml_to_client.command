#!/bin/bash

echo "//---------------------------------------------//"
echo "//          COPYING RULES TO GAME...           //"
echo "//---------------------------------------------//"
echo " "

# Aux vars
INPUT_DIR=xml
OUTPUT_DIR=../../Assets/Resources/Rules

# Go to script's dir
cd "$(dirname "$0")"

# Perform the copy. rsync allows us to exclude hidden .svn folders :)
rsync -r -v --exclude=.svn* $INPUT_DIR/* $OUTPUT_DIR/

echo "//---------------------------------------------//"
echo "//                    PROFIT!!                 //"
echo "//---------------------------------------------//"
echo " "
