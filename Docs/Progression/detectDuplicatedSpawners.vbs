Dim objFSO
Dim objShell



Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

Set objOutputFile 		= objFSO.CreateTextFile("duplicatedSpawners.txt", 2, true)


REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village.unity")
REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Castle.unity")
Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Dark.unity")
Dim substrToFind
Dim exit_y
Dim exit_name

	substrToFind = "propertyPath: m_LocalPosition.x"
	tmpFindPrefab = "Prefab:"
	name = ""
	Do until objInputFile.AtEndOfStream
		tmpStrFindPrefab = objInputFile.ReadLine
		If foundStrMatch(tmpFindPrefab,tmpStrFindPrefab) = true or name = "unknown" Then
			exit_prefab = false
			Do until exit_prefab		
				tmpStr = objInputFile.ReadLine
				If foundStrMatch(tmpStr,substrToFind) = true Then
					pos_x = objInputFile.ReadLine
					substrToFind2 =  "propertyPath: m_LocalPosition.y"
					exit_y = false
					Do until exit_y
						tmpStr2 = objInputFile.ReadLine
						If foundStrMatch(tmpStr2,substrToFind2) = true Then
							pos_y = objInputFile.ReadLine
							exit_y = true
						End If
					Loop
					substrToFind3 =  "propertyPath: m_Name"
					exit_name = false
					Do until exit_name
						tmpStr3 = objInputFile.ReadLine
						If foundStrMatch(tmpStr3,substrToFind3) = true Then
							name = objInputFile.ReadLine
							exit_name = true
							exit_prefab = true
						End If
						If foundStrMatch(tmpFindPrefab,tmpStr3) = true Then
							name = "unknown"
							exit_name = true
							exit_prefab = true						
						End If
					Loop
					objOutputFile.Write(Replace(Replace(name,"value: ","") + " : " + Replace(pos_x,"value: ","") + " ; " + Replace(pos_y,"value: ","") + vbCrLf," ",""))
				End If		
			Loop
		End If
	Loop

Function foundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function