#!/usr/bin/python

import os
import sys
import codecs
import fnmatch
from os.path import isfile, join
from collections import Counter

#################################################### CONSTANTS

# Constants
LOCALIZATION_FILES_DIR = "../../Assets/Resources/Localization"

#################################################### PARAMS CHECKING

# Check params amount
if len(sys.argv) != 3:
	print "\nUsage: missing_chars.py language text_to_check"
	sys.exit(2)

# Check language param
allFiles = os.listdir(LOCALIZATION_FILES_DIR)
locFiles = []
for f in allFiles:
	if(fnmatch.fnmatch(f, "*.txt")):
		locFiles.append(f)

lang = sys.argv[1]
try:
	i = locFiles.index(lang + ".txt")
except ValueError:
	print "\nUnknown language " + lang
	print "Valid languages: "
	for f in locFiles:
		print "\t" + os.path.splitext(f)[0]	# Filename without extension
	print
	sys.exit(2)

#################################################### SCRIPT

# Parse file
encoding = 'utf_8'
langFilePath = LOCALIZATION_FILES_DIR + "/" + locFiles[i]
with codecs.open(langFilePath, 'r', encoding) as fin:
	# Read file content into a string
	langFileContent = fin.read();

	# Counter() creates a hash collection with the content of the file
	# This will basically create a collection with a single entry for each unique character in the string
	# See https://docs.python.org/2/library/collections.html#collections.Counter
	# See http://rahmonov.me/posts/python-collections-counter/
	langHash = Counter(langFileContent)

	# Iterate input text and check whether each character exists in the language hash
	missingChars = []
	textToCheck = sys.argv[2].decode("utf-8")	# decode text properly!
	for c in textToCheck:
		output = "Checking " + c + "... "
		if langHash.get(c) == None:
			missingChars.append(c)
			output += "MISS!"
		else:
			output += "OK!"
		print output

	print "DONE!"
	print str(len(missingChars)) + "/" + str(len(textToCheck)) + " charactes missing"
	if len(missingChars) > 0:
		print "Missing Characters:"
		for c in missingChars:
			print c
