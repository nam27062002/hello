Dim objFSO
Dim MyFile
Dim MyFolder
Dim objShell

Set objFSO 			= CreateObject("Scripting.FileSystemObject")
Set objShell 		= WScript.CreateObject("WScript.Shell")

Set MyFolder 		= objFSO.GetFolder("D:\Projects\HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs")
Set objOutputFile 	= objFSO.CreateTextFile("D:\Projects\HungryDragon\Docs\Progression\respawnTimes.txt", 2, true)


For Each MyFile In MyFolder.Files
	If Right(MyFile.Path,7) = ".prefab" Then
		Set objInputFile = objFSO.OpenTextFile(MyFile)
		Do until objInputFile.AtEndOfStream
			tmpStr = objInputFile.ReadLine
			If foundStrMatch(tmpStr) = true Then
				spawner = Replace(MyFile.Path,"D:\Projects\HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs\","")
				spawner = Replace(spawner,".prefab","")
				minTime = Replace(objInputFile.ReadLine,"min:","")
				maxTime = Replace(objInputFile.ReadLine,"max:","")
				text = spawner + ";" + minTime + ";" + maxTime + vbCrLf
				objOutputFile.Write(text)
			End If
		Loop
	End If
Next

Function foundStrMatch(tmpStr)
	Dim substrToFind
	substrToFind = "m_spawnTime:"
	If InStr(tmpStr, substrToFind) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function