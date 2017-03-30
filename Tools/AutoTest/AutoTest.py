#!/usr/bin/env python

import os
import sys
import xml.etree.ElementTree as ET

xcode_folder = sys.argv[1]
device_id = sys.argv[2]
team = "Marie\ Cordon"
provisioning = "4335285c-fb22-40cd-a059-1085b36beb71"

scheme_file = xcode_folder + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme"

tree = ET.parse(scheme_file)
root = tree.getroot()

arguments = root.find(".//CommandLineArguments")
test_argument = ET.SubElement(arguments,"CommandLineArgument")
test_argument.set("argument", "-start_test")
test_argument.set("isEnabled", "YES")
tree.write( scheme_file )


# Build call

xcode_build_call = "xcodebuild clean install -alltargets -project " + xcode_folder + "/Unity-iPhone.xcodeproj" + " PROVISIONING_PROFILE="+provisioning+" -destination 'platform=iOS,id=" + device_id + "'"
print xcode_build_call
# os.system( xcode_build_call )

# Test call
xcode_test_call = "xcodebuild test -project " + xcode_folder + "/Unity-iPhone.xcodeproj" + " DEVELOPMENT_TEAM=" + team + " -scheme Unity-iPhone -destination 'platform=iOS,id=" + device_id + "'"
print xcode_test_call
os.system( xcode_test_call )

arguments.remove(test_argument)
tree.write( scheme_file )
