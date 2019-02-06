@REM ##########################################################
@REM ##
@REM ## UBIPACKAGES
@REM ## INITIALIZER
@REM ##
@REM ##########################################################

echo off

setlocal EnableDelayedExpansion

call :project AddressablesUnitTests
goto :finish

:project
	call :link %1 UbiPackages
	call :link %1 Calety
	call :link %1 CaletyExternalPlugins	
	goto :nofinish

:link		
	set resultMsg=
	set ubiPackagesFolderName=%2
	set pathToUbiPackagesSource=..\Assets\!ubiPackagesFolderName!
	set pathToUbiPackagesDest=%1\Assets\!ubiPackagesFolderName!

	REM Checks if the link already exists 
	if exist !pathToUbiPackagesDest! (
		set resultMsg=Symbolic link [!pathToUbiPackagesDest!] already exists
		goto show_result	
	) 

	REM Searches for UbiPackages folder source
	if exist !pathToUbiPackagesSource! (
		goto foundSource
	) else (
		set resultMsg=ERROR: UbiPackages source not found in !pathToUbiPackagesSource!
		goto :show_result
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
		
:show_result		
	echo !resultMsg!		
	goto :nofinish
	
:finish
	pause
	exit /b 0
	
:nofinish



