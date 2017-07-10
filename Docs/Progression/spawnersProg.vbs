Dim objFSO
Dim objShell



Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

REM Set SpawnersSceneFile	= objFSO.GetFolder("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village.unity")
Set objOutputFile 		= objFSO.CreateTextFile("spawnersProg.txt", 2, true)


Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village.unity")
Dim substrToFind

	Do until objInputFile.AtEndOfStream
		substrToFind = "propertyPath: m_Name"
		tmpStr = objInputFile.ReadLine
		If foundStrMatch(tmpStr,substrToFind) = true Then
			tmpStr = objInputFile.ReadLine
			tmpStrToSave = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(tmpStr,"value: ",""),"SP_","SP@"),"_Evade","@Evade"),"_Generic","@Generic"),"_Squid","@Squid"),"_Static","@Static"),"_Flock","@Flock"),"_Mix","@Mix"),"_Random","@Random"),"_Worker","@Worker")," ",""),"PF_","PF@"),"BG_","BG@"),"_Path","@Path"),"_",";")
			tmpStr = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(tmpStr,"value: ",""),"SP_","SP"),"_Evade",""),"_Generic",""),"_Squid",""),"_Static",""),"_Flock",""),"_Mix",""),"_Random",""),"_Worker","")," ",""),"PF_","PF"),"BG_",""),"_Path",""),"_",";")
			tmpStr2 = ";"
			If foundStrMatch(tmpStr,tmpStr2) = false Then
				tmpStr = Replace(tmpStrToSave,"@","_") + ";0;0"
			Else
				tmpStr = Replace(tmpStrToSave,"@","_")
			End If
			tmpStr = tmpStr + vbCrLf
			objOutputFile.Write(tmpStr)
		End If
	Loop

Function foundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function