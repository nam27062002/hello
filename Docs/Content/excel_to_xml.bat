REM Show initial feedback
echo "----------- EXPORTING EXCEL TO XML... ------------"
echo " "

REM Aux vars
set INPUT_FILE=HungryDragonContent.xlsx
set OUTPUT_DIR=xml
set TOOL_EXECUTABLE=xml_content_generator.jar

REM Go to script's dir
cd "%~dp0"

REM Make sure export folder exists
if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%

# Clear previously exported files
rm -rf $OUTPUT_DIR/*
del /F /Q /S "%OUTPUTDIR%\*"

REM Run Java tool
java -jar %TOOL_EXECUTABLE% %INPUT_FILE% %OUTPUT_DIR%

REM Git Auto-commit
REM TODO!!

REM SVN Auto-commit
REM "C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe" /command:commit /path:"%OUTPUT_DIR%\" /logmsg:"Rules: Content - Auto-commit." /closeonend:1

REM Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "

REM Add a pause to be able to see the output before the terminal auto-closes itself
pause
