#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import sys
import xml.etree.ElementTree as ET
import time
import subprocess

script_path = os.path.realpath(__file__)
script_path = os.path.dirname(script_path)

xcode_folder = sys.argv[1]

team_id = "Y3J3C97LQ8"

project_file = os.path.abspath(xcode_folder + "/Unity-iPhone.xcodeproj/project.pbxproj")
# SIGNING TO MANUAL
# Read in the file
file = open(project_file, 'r')
original_project_file = file.read()
# Replace the target string
# filedata = filedata.replace('ProvisioningStyle = Automatic;', 'ProvisioningStyle = Manual;')
filedata = original_project_file.replace('DEVELOPMENT_TEAM = "";', 'DEVELOPMENT_TEAM = '+team_id+';')

# Write the file out again
file = open(project_file, 'w')
file.write(filedata)

scheme_file = os.path.abspath(xcode_folder + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme")

# ADD TEST ARGUMENT
tree = ET.parse(scheme_file)
root = tree.getroot()
for arguments in root.findall(".//CommandLineArguments"):
	test_argument = ET.SubElement(arguments,"CommandLineArgument")
	test_argument.set("argument", "-start_test")
	test_argument.set("isEnabled", "YES")
tree.write( scheme_file )

# END ADD TEST ARGUMENT
project_path = os.path.abspath(xcode_folder + "/Unity-iPhone.xcodeproj")
# open_xcode = "open -a Xcode " + project_path
# print open_xcode
# os.system(open_xcode)
subprocess.Popen(['open', '-a', 'Xcode', project_path])
# time.sleep(10)

auto_run_path = os.path.abspath(script_path + "/AutoRun.applescript")
# run_script = "osascript " + auto_run_path
# os.system(run_script)
subprocess.Popen(['osascript', auto_run_path])

# REVERT TEST ARGUMENT
# arguments.remove(test_argument)
# tree.write( scheme_file )

# REVER SINGING STYLE
#with open(project_file, 'w') as file:
#  file.write(original_project_file)

sys.exit(0);
