Dim objFSO
Dim objShell



Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

Set objOutputFile 		= objFSO.CreateTextFile("spawnersProg.txt", 2, true)


REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village.unity")
REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Castle.unity")
Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Dark.unity")
Dim substrToFind

	Do until objInputFile.AtEndOfStream
		substrToFind = "propertyPath: m_Name"
		tmpStr = objInputFile.ReadLine
		If foundStrMatch(tmpStr,substrToFind) = true Then
			tmpStr = objInputFile.ReadLine
			tmpStrToSave = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(tmpStr,"value: ",""),"SP_","SP@"),"_Evade","@Evade"),"_Generic","@Generic"),"_Squid","@Squid"),"_Static","@Static"),"_Flock","@Flock"),"_Dark","@Dark"),"_Mix","@Mix"),"_Random","@Random"),"_Worker","@Worker"),"_Root","@Root")," ",""),"Air_","Air@"),"PF_","PF@"),"BG_","BG@"),"_Path","@Path"),"_Plant","@Plant"),"_",";")
			tmpStr = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(tmpStr,"value: ",""),"SP_","SP"),"_Evade",""),"_Generic",""),"_Squid",""),"_Static",""),"_Flock",""),"_Dark",""),"_Mix",""),"_Random",""),"_Worker",""),"_Root","")," ",""),"PF_","PF"),"BG_",""),"_Path",""),"_Plant",""),"Air_",""),"_",";")
			tmpStr2 = ";"
			If foundStrMatch(tmpStr,tmpStr2) = false Then
				tmpStr = Replace(tmpStrToSave,"@","_") + ";0;0"
			Else
				tmpStr = Replace(tmpStrToSave,"@","_")
			End If
			tmpStr = tmpStr + vbCrLf
			If foundStrMatch(tmpStr,"SP_BG") = false Then
				objOutputFile.Write(tmpStr)
			End If
		End If
	Loop

Function foundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function