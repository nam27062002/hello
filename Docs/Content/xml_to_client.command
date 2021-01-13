#!/bin/bash

# Show initial feedback
echo "----------- COPYING RULES TO CLIENT... -----------"
echo " "

# Aux vars
INPUT_DIR=xml
OUTPUT_DIR=../../Assets/Resources/Rules

# Go to script's dir
cd "$(dirname "$0")"

# Prepare parameters for the copy
# rsync allows us to exclude hidden .svn folders :)
# -a Will preserve timestamps, owner, permissions, etc. Not really necessary, but feels right.
# [AOC] Unfortunately, we can't use the -u (update) parameter since git rewrites all timestamps
#       and they can't be used to detect which files have changed, as rsync does. Instead we'll 
#       just copy all the files, slower but 100% safe.
RSYNC_PARAMS="-r -a --exclude=.svn* --exclude=*.meta --exclude=.DS_Store --ignore-errors"

# Anticipate which files will be deleted and ask for confirmation
# The -n parameter will run only a simulation of what would be changed
TO_DELETE_LIST=$(rsync $RSYNC_PARAMS --delete -n -v $INPUT_DIR/ $OUTPUT_DIR | grep deleting)

# If there are files to delete, ask for confirmation
# @see http://tldp.org/LDP/abs/html/comparison-ops.html
if [ -n "$TO_DELETE_LIST" ]; then
	# Replace all matches of 'deleting' with '' and give feedback to the user
	# @see http://tldp.org/LDP/abs/html/x23170.html
	echo "The following files are in the target directory but not on the source:"
	echo "$TO_DELETE_LIST" | sed "s/deleting//g"
	echo "Delete files?"
	select YN in "Yes" "No"; do
		case $YN in
			Yes) 
				# Add the --delete flag to the params
				RSYNC_PARAMS="$RSYNC_PARAMS --delete"
				break;;

				No) 
				# Nothing to do, just perform the import afterwards
				break;;
		esac
	done
else
	# Nothing to do, just perform the import afterwards
	echo "No files will be deleted, performing the import!"
fi

# Perform the import!
# Use the customized parameters var
rsync -v $RSYNC_PARAMS $INPUT_DIR/ $OUTPUT_DIR

# Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "
