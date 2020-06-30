@echo off
rem @echo off is used to avoid printing each command (i.e. the REM commands)

rem Show initial feedback
echo "----------- EXPORTING EXCEL TO XML... ------------"
echo " "

rem Aux vars
set EXCEL_TO_EXPORT=HungryDragonContent_Dragons.xlsx
set TMP_DIR=tmp
set OUTPUT_DIR=xml
set PROJECT_DIR=..\..\Assets\Resources\Rules
set TOOL_EXECUTABLE=xml_content_generator.jar

rem Go to script's dir
cd "%~dp0"

rem ---------------------------------------------------------------------------
rem 1. Export Excel tables into xml files in a TMP folder
rem ---------------------------------------------------------------------------

rem Make sure export folder exists
if not exist %TMP_DIR% mkdir %TMP_DIR%

rem Clear previously exported files, if any
del /F /Q /S "%TMP_DIR%\*"

rem Run Java tool for Excel file
echo "    Exporting %%~nf..."
java -jar %TOOL_EXECUTABLE% "%EXCEL_TO_EXPORT%" "%TMP_DIR%"

rem ---------------------------------------------------------------------------
rem 2. Copy xml files to both output dir and project dir
rem ---------------------------------------------------------------------------

rem Make sure both folders exists
if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%
if not exist %PROJECT_DIR% mkdir %PROJECT_DIR%

rem robocopy allows us to exclude hidden .svn folders and other patterns :)
robocopy %TMP_DIR% %OUTPUT_DIR% /E /V /XD .svn* .DS_Store /XF *.meta
robocopy %TMP_DIR% %PROJECT_DIR% /E /V /XD .svn* .DS_Store /XF *.meta

rem ---------------------------------------------------------------------------
rem 3. Clean temp files
rem ---------------------------------------------------------------------------

rem Clear temporal folder and files
rmdir /Q /S "%TMP_DIR%"

rem ---------------------------------------------------------------------------
rem 4. Update Version Control
rem ---------------------------------------------------------------------------

rem Git Auto-commit
rem TODO!!

rem SVN Auto-commit
rem "C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe" /command:commit /path:"%OUTPUT_DIR%\" /logmsg:"Rules: Content - Auto-commit." /closeonend:1

rem ---------------------------------------------------------------------------
rem DONE!
rem ---------------------------------------------------------------------------

rem Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "

rem Add a pause to be able to see the output before the terminal auto-closes itself
pause
