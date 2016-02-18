echo "//---------------------------------------------//"
echo "//          COPYING RULES TO GAME...           //"
echo "//---------------------------------------------//"
echo " "

REM Aux vars
set INPUT_DIR=xml
set OUTPUT_DIR=..\..\Assets\Resources\Rules

REM Go to script's dir
cd "%~dp0"

REM Perform the copy. robocopy allows us to exclude hidden .svn folders :)
robocopy %INPUT_DIR% %OUTPUT_DIR% /E /V /XD .svn*

echo "//---------------------------------------------//"
echo "//                    PROFIT!!                 //"
echo "//---------------------------------------------//"
echo " "

REM Add a pause to be able to see the output before the terminal auto-closes itself
pause