REM Show initial feedback
echo "----------- EXPORTING EXCEL TO XML... ------------"
echo " "

REM Aux vars
set OUTPUT_DIR=xml
set TOOL_EXECUTABLE=xml_content_generator.jar

REM Go to script's dir
cd "%~dp0"

REM Make sure export folder exists
if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%

REM Clear previously exported files
del /F /Q /S "%OUTPUT_DIR%\*"

REM Run Java tool for Excel file
echo "    Exporting %%~nf..."
java -jar %TOOL_EXECUTABLE% "HungryDragonContent_Clustering.xlsx" %OUTPUT_DIR%


REM Show initial feedback
echo "----------- COPYING RULES TO CLIENT... -----------"
echo " "

REM Aux vars
set INPUT_DIR=xml
set OUTPUT_DIR=..\..\ServerRules

REM Go to script's dir
cd "%~dp0"

REM Perform the copy. robocopy allows us to exclude hidden .svn folders and other patterns :)
robocopy %INPUT_DIR% %OUTPUT_DIR% /E /V /XD .svn* .DS_Store /XF *.meta

REM Show finish feedback
echo "--------------------- DONE! ----------------------"
echo " "

REM Add a pause to be able to see the output before the terminal auto-closes itself
pause
