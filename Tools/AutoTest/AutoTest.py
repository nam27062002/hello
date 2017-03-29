#!/usr/bin/env python

import os
import sys
import xml.etree.ElementTree as ET

xcode_folder = sys.argv[1]
device_name = sys.argv[2]

scheme_file = xcode_folder + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme"

tree = ET.parse(scheme_file)
root = tree.getroot()

arguments = root.find(".//CommandLineArguments")
test_argument = ET.SubElement(arguments,"CommandLineArgument")
test_argument.set("argument", "-start_test")
test_argument.set("isEnabled", "YES")
tree.write( scheme_file )

os.system("xcodebuild test -project " + xcode_folder +"/Unity-iPhone.xcodeproj" + " -scheme Unity-iPhone -destination 'platform=iOS,name="+device_name+'")

arguments.remove(test_argument)
tree.write( scheme_file )
