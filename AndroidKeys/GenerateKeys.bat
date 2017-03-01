@REM ##########################################################
@REM ##
@REM ## UBISOFT MOBILE BARCELONA
@REM ## ANDROID KEYS GENERATOR
@REM ##
@REM ##########################################################

@echo off

setlocal EnableDelayedExpansion

cls

@echo.
@echo UBISOFT BUILDING SYSTEM - Generating Android Keys...
@echo.

@SET strPrefixToFind=
@SET strPathToUbiSDK=
@SET strPathToSearch=
@SET maxTries=
@SET MobileToolkit=

@SET strPrefixToFind=/MobileToolkit
@SET strPathToUbiSDK=.
@SET strPathToSearch=!strPathToUbiSDK!!strPrefixToFind!
@SET maxTries=10

@for /l %%x in (1, 1, !maxTries!) do (
	@SET strPathToUbiSDK=!strPathToUbiSDK!/..
	@SET strPathToSearch=!strPathToUbiSDK!!strPrefixToFind!
	
	if exist !strPathToSearch! (
		@SET MobileToolkit=!strPathToSearch!
		goto found
	)
	
	@IF %%x == !maxTries! (goto notFound)
)

:found
@echo.
@echo DEBUG Key data...
@echo.
"!MobileToolkit!/BuildTools/Java/windows/bin/keytool.exe" -genkey -v -keystore debugKey.keystore -alias androidDebugKey -keyalg RSA -keysize 2048 -validity 10000
@echo.
@echo RELEASE Key data...
@echo.
"!MobileToolkit!/BuildTools/Java/windows/bin/keytool.exe" -genkey -v -keystore releaseKey.keystore -alias androidReleaseKey -keyalg RSA -keysize 2048 -validity 10000
goto finish

:notFound
@echo No MobileToolkit found. Checkout UBI Mobile Toolkit and be sure the folder is called MobileToolkit.

:finish

