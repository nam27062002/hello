# This script should be used when the build machine fails uploading iOS symbols to Crashlytics
# copy this script into build machine to /Users/dragon/Projects/JenkinsBuilds/archives to manually upload symbol files
# usage: upload-symbols-manually filename.xcarchive
#
/Users/dragon/Projects/JenkinsBuilds/xcode/Pods/Fabric/upload-symbols -a 5f0fbe5d830b7eea805c3f71b01c0c781d270723 -p ios ${1}/dSYMs