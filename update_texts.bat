rem Script for Windows to update the game texts with localization for every language from the UBI Soft tool OASIS
echo off
echo Updating Texts...

Tools\Ubi.Tools.Oasis.WebServices.TidExtractor\bin\Release\Ubi.Tools.Oasis.WebServices.XmlExtractor.exe -host http://oasis-pdc2.ubisoft.org/HungryDragon -directory Assets\Resources\Localization\

if /I "%ERRORLEVEL%" NEQ "0" (

echo TEXTS NOT Updated!

) else (

rename "Assets\Resources\Localization\simplified chinese.txt" "simplified_chinese.txt"
rem copy Assets\Resources\Localization\*.txt ..\assets\localization\
echo TEXTS Updated!

)
pause
