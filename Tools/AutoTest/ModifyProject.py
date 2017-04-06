#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import sys
import xml.etree.ElementTree as ET
import time
import subprocess

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
launch_action = root.find("LaunchAction")
arguments = launch_action.find("CommandLineArguments")
if arguments is None:
    arguments = ET.SubElement(launch_action, "CommandLineArguments")

arg = arguments.find("CommandLineArgument[@argument='-start_test']")
if arg is None:
    test_argument = ET.SubElement(arguments,"CommandLineArgument")
    test_argument.set("argument", "-start_test")
    test_argument.set("isEnabled", "YES")
tree.write( scheme_file )
sys.exit(0);

