rem Script for Windows to update the game texts with localization for every language from the UBI Soft tool OASIS
echo off

rem Geeky color
color 0a

rem Vars
set TOOL_PATH=Tools\Ubi.Tools.Oasis.WebServices.TidExtractor\bin\Release\Ubi.Tools.Oasis.WebServices.XmlExtractor.exe
set EXPORT_DIR=Assets\Resources\Localization
set FINAL_DIR=Assets\Resources\Localization

echo Updating Texts...

rem Invoke the Oasis -> txt tool
"%TOOL_PATH%" -host http://oasis-pdc2.ubisoft.org/HungryDragon -directory "%EXPORT_DIR%"

if /I "%ERRORLEVEL%" NEQ "0" (
	echo TEXTS NOT Updated!
) else (
	rem Perform some custom naming
	rem /y overwrites the destination file if already existing
	move /y "%EXPORT_DIR%\sim chinese.txt" "%EXPORT_DIR%\simplified_chinese.txt"

	rem Delete some unwanted files
	del /q "%EXPORT_DIR%\english proof.txt"
	
	rem If the export directory is not the final one, copy txt files to the final dir
	rem copy %EXPORT_DIR%\*.txt %FINAL_DIR%

	echo TEXTS Updated!
)

rem Prevent the window to be auto-closed
pause
