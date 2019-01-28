import sys
import codecs
from collections import Counter

# Check params
if len(sys.argv) != 3:
	print "\nUsage: unique_chars.py input_file output_file"
	sys.exit(2)

# Parse file
encoding = 'utf_8'
with codecs.open(sys.argv[1], 'r', encoding) as fin:
	# Read file content into a string
	fileContentString = fin.read();

	# Counter() creates a hash collection with the content of the file
	# This will basically create a collection with a single entry for each unique character in the string
	# See https://docs.python.org/2/library/collections.html#collections.Counter
	contentHash = Counter(fileContentString)
	
	# Write into file
	with codecs.open(sys.argv[2], 'w', encoding) as fout:
		for key in contentHash:
			print key
			fout.write(key + '\n')

	# Summary
	print "\nUnique characters in " + sys.argv[1] + ": " + str(len(contentHash)) + "\n"