#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import sys
import xml.etree.ElementTree as ET

xcode_folder = sys.argv[1]
device_id = sys.argv[2]

team_id = "Y3J3C97LQ8"

project_file = xcode_folder + "/Unity-iPhone.xcodeproj/project.pbxproj"
# SIGNING TO MANUAL
# Read in the file
with open(project_file, 'r') as file :
  original_project_file = file.read()
# Replace the target string
# filedata = filedata.replace('ProvisioningStyle = Automatic;', 'ProvisioningStyle = Manual;')
filedata = original_project_file.replace('DEVELOPMENT_TEAM = "";', 'DEVELOPMENT_TEAM = '+team_id+';')
# Write the file out again
with open(project_file, 'w') as file:
  file.write(filedata)

team = "Marie\ Cordon"
signing_certificate = "'iPhone Developer: Sergi Berjano (9BYU47EHEA)'"
provisioning = "8a073c3d-0331-4e14-b0d0-3a5dc9491a5d"

scheme_file = xcode_folder + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme"

# ADD TEST ARGUMENT
tree = ET.parse(scheme_file)
root = tree.getroot()

arguments = root.find(".//CommandLineArguments")
test_argument = ET.SubElement(arguments,"CommandLineArgument")
test_argument.set("argument", "-start_test")
test_argument.set("isEnabled", "YES")
tree.write( scheme_file )
# END ADD TEST ARGUMENT

# code_sign_attribute = " CODE_SIGN_IDENTITY=" + signing_certificate
# provision_attribute = " PROVISIONING_PROFILE=" + provisioning
# dev_team_attribute = " DEVELOPMENT_TEAM=" + team

# Build call
# xcode_build_call = "xcodebuild clean install -alltargets -project " + xcode_folder + "/Unity-iPhone.xcodeproj" + " -destination 'platform=iOS,id=" + device_id + "'"
# print xcode_build_call
# os.system( xcode_build_call )

# Test call
xcode_test_call = "xcodebuild test -project " + xcode_folder + "/Unity-iPhone.xcodeproj" + " -scheme Unity-iPhone -destination 'platform=iOS,id=" + device_id + "'"
print xcode_test_call
os.system( xcode_test_call )

# REVERT TEST ARGUMENT
arguments.remove(test_argument)
tree.write( scheme_file )

# REVER SINGING STYLE
with open(project_file, 'w') as file:
  file.write(original_project_file)
