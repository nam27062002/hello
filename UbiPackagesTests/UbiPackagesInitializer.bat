@REM ##########################################################
@REM ##
@REM ## UBIPACKAGES
@REM ## INITIALIZER
@REM ##
@REM ##########################################################

echo off

setlocal EnableDelayedExpansion

call :link_ubi_packages AssetBundlesUnitTests
exit /b 0

:link_ubi_packages		
	set resultMsg=
	set ubiPackagesFolderName=UbiPackages
	set pathToUbiPackagesSource=..\Assets\!ubiPackagesFolderName!
	set pathToUbiPackagesDest=%1\Assets\!ubiPackagesFolderName!

	REM Checks if the link already exists 
	if exist !pathToUbiPackagesDest! (
		set resultMsg=Symbolic link [!pathToUbiPackagesDest!] already exists
		goto finish	
	) 

	REM Searches for UbiPackages folder source
	if exist !pathToUbiPackagesSource! (
		goto foundSource
	) else (
		set resultMsg=ERROR: UbiPackages source not found in !pathToUbiPackagesSource!
		goto :finish
	)

	:foundSource
		set relativePathToSource=..\..\!pathToUbiPackagesSource!
		echo Symbolic linking !pathToUbiPackagesDest! to !pathToUbiPackagesSource!...
	
		REM Makes the symbolic link
		mklink /d !pathToUbiPackagesDest! !relativePathToSource!
	
		if exist !pathToUbiPackagesDest! (		
			set resultMsg=SUCCESS
		) else (
			set resultMsg=mklink failed		
		)
		
	:finish		
		echo !resultMsg!		
		
		pause
		exit /b 0


