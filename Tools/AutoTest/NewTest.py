#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import sys
import xml.etree.ElementTree as ET
import time

script_path = os.path.realpath(__file__)
script_path = os.path.dirname(script_path)
print script_path

xcode_folder = sys.argv[1]
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

os.system("open -a Xcode "+xcode_folder+"/Unity-iPhone.xcodeproj")
time.sleep(10)
os.system("osascript "+script_path+"/AutoRun.applescript")

# REVERT TEST ARGUMENT
#arguments.remove(test_argument)
#tree.write( scheme_file )

# REVER SINGING STYLE
#with open(project_file, 'w') as file:
#  file.write(original_project_file)
