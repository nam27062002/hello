@REM ##########################################################
@REM ##
@REM ## CALETY
@REM ## INITIALISER
@REM ##
@REM ##########################################################

@REM @echo off

setlocal EnableDelayedExpansion

@REM https://www.codeproject.com/Tips/119828/Running-a-bat-file-as-administrator-Correcting-cur
@setlocal enableextensions
@cd /d "%~dp0"

@echo.
@echo CALETY INITIALISER
@echo Preparing Calety framework and symbolic linkages ...
@echo.

@SET strPrefixToFind=
@SET strPathToCaletySDK=
@SET strPathToSearch=
@SET maxTries=
@SET Calety=

@SET strPrefixToFind=/calety
@SET strPathToCaletySDK=.
@SET strPathToSearch=!strPathToCaletySDK!!strPrefixToFind!
@SET maxTries=10

@for /l %%x in (1, 1, !maxTries!) do @(
	@echo Looking for Calety at path %!strPathToSearch!%
	@if exist !strPathToSearch! @(
		@SET Calety=!strPathToSearch!
		@echo FOUND!
		@echo
		@goto found
	)
	
	@SET strPathToCaletySDK=!strPathToCaletySDK!\..
	@SET strPathToSearch=!strPathToCaletySDK!!strPrefixToFind!
	
	@IF %%x == !maxTries! @(goto notFound)
)

:found

@goto finish

:notFound
@REM @echo No Calety was found. Now GIT is going to checkout the Calety framework. This could last some minutes. Please wait...

@REM @git clone git@bcn-mb-git.ubisoft.org:tools/calety.git ./../../calety

@goto finishNoCalety

:finish

@REM @echo Forcing Game Project branch ...
@echo Finishing installation...

@SET CurrentFolder=%CD%

@cd %strPathToCaletySDK%\calety

@REM @git checkout develop

@REM @git pull

@cd %CurrentFolder%

@echo Generating symbolic linkages to Calety ...

@cd Assets

@IF NOT EXIST Calety (
@echo Creating link to Calety (%strPathToCaletySDK%\..\calety\Calety\UnityProject\Assets\Calety)
MKLINK /d Calety %strPathToCaletySDK%\..\calety\Calety\UnityProject\Assets\Calety
)

cd Editor

@IF NOT EXIST Calety (
@echo Creating link to Editor\Calety (%strPathToCaletySDK%\..\..\calety\Calety\UnityProject\Assets\Editor\Calety)
MKLINK /d Calety %strPathToCaletySDK%\..\..\calety\Calety\UnityProject\Assets\Editor\Calety
)

cd ..

@IF NOT EXIST CaletyExternalPlugins (
	@echo Creating folder CaletyExternalPlugins...

	mkdir CaletyExternalPlugins
	
	cd CaletyExternalPlugins
	
	echo.Plugins>.gitignore
	echo.Plugins.meta>>.gitignore
	
	cd ..
)

cd CaletyExternalPlugins

@IF NOT EXIST Plugins (
@echo Creating link to CaletyExternalPlugins\Plugins (%strPathToCaletySDK%\..\..\calety\Calety\UnityProject\Assets\CaletyExternalPlugins\Plugins)
MKLINK /d Plugins %strPathToCaletySDK%\..\..\calety\Calety\UnityProject\Assets\CaletyExternalPlugins\Plugins
)

@cd ..
@cd ..

@echo Done. Thanks for using Calety.
@pause

:finishNoCalety
@echo Calety was not found. Make sure you have checked out the repository in git!
@pause






