Dim objFSO
Dim objShell



Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

Set objOutputFile 		= objFSO.CreateTextFile("spawnersGoodJunk.txt", 2, true)


Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village.unity")
REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Castle.unity")
REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Dark.unity")
Dim substrToFind

	substrToFind = "propertyPath: m_quantity"

	Do until objInputFile.AtEndOfStream
		tmpStr = objInputFile.ReadLine
		If foundStrMatch2(tmpStr,"SP_GoodJunkBottle") = true Then
			objOutputFile.Write("SP_GoodJunkBottle;1" + vbCrLf)
		End if
		If foundStrMatch(tmpStr,substrToFind) = true Then
			tmpStr = objInputFile.ReadLine
			tmpStr = Replace(Replace(tmpStr,"value: ","SP_GoodJunkScore;")," ","")
			tmpStr = tmpStr + vbCrLf
			objOutputFile.Write(tmpStr)
		End If	
	Loop

Function foundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 And Len(tmpStr) = 30 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function

Function foundStrMatch2(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		foundStrMatch2 = true
	Else
		foundStrMatch2 = false
	End If
End Function