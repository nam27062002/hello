@REM ##########################################################
@REM ##
@REM ## UBIPACKAGES
@REM ## INITIALIZER
@REM ##
@REM ##########################################################

echo off

setlocal EnableDelayedExpansion

call :project AddressablesUnitTests
call :project AddressablesAssetsTests
call :project AddressablesFoldersTests
goto :finish

:project
	call :link %1 UbiPackages
	REM call :link %1 Calety
	REM call :link %1 CaletyExternalPlugins	
	goto :nofinish

:link		
	set resultMsg=
	set folderName=%2
	set pathToFolderNameSource=..\Assets\!folderName!
	set pathToFolderNameDest=%1\Assets\!folderName!

	REM Checks if the link already exists 
	if exist !pathToFolderNameDest! (
		set resultMsg=Symbolic link [!pathToFolderNameDest!] already exists
		goto show_result	
	) 

	REM Searches for folder source
	if exist !pathToFolderNameSource! (
		goto foundSource
	) else (
		set resultMsg=ERROR: source not found in !pathToFolderNameSource!
		goto :show_result
	)

:foundSource		
	set relativePathToSource=..\..\!pathToFolderNameSource!
	echo Symbolic linking !pathToFolderNameDest! to !pathToFolderNameSource!...

	REM Makes the symbolic link
	mklink /d !pathToFolderNameDest! !relativePathToSource!

	if exist !pathToFolderNameDest! (		
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



